using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using System.Reflection;

public static class PluginInfo {

	public const string TITLE = "Unbreakable Tools";
	public const string NAME = "unbreakable_tools";

	public const string VERSION = "0.0.7";
	public static string[] CHANGELOG = new string[] {
		"v0.0.7 - Fixed issue with mining helmet not retaining charge"
	};

	public const string AUTHOR = "devopsdinosaur";
	public const string GAME = "dinkum";
	public const string GUID = AUTHOR + "." + GAME + "." + NAME;
}

[BepInPlugin(PluginInfo.GUID, PluginInfo.TITLE, PluginInfo.VERSION)]
public class UnbreakableToolsPlugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony(PluginInfo.GUID);
	public static ManualLogSource logger;
	private static ConfigEntry<bool> m_enabled;
	
	private void Awake() {
		logger = this.Logger;
		try {
			m_enabled = this.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
			this.m_harmony.PatchAll();
			logger.LogInfo($"{PluginInfo.GUID} v{PluginInfo.VERSION} loaded.");
		} catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e.StackTrace);
		}
	}

	[HarmonyPatch(typeof(Inventory), "Update")]
	class HarmonyPatch_Inventory_Update {

		private const float CHECK_FREQUENCY = 1.0f;
		private static float m_elapsed = CHECK_FREQUENCY;

		private static bool Prefix(Inventory __instance) {
			try {
				InventorySlot slot;
				if (!m_enabled.Value || (m_elapsed += Time.fixedDeltaTime) < CHECK_FREQUENCY) {
					return true;
				}
				m_elapsed = 0f;
				for (int i = 0; i < Inventory.Instance.invSlots.Length; i++) {
					slot = Inventory.Instance.invSlots[i];
					if (slot.itemNo != -1 && Inventory.Instance.allItems[slot.itemNo].isATool) {
						if (slot.stack < slot.itemInSlot.fuelMax) {
							slot.updateSlotContentsAndRefresh(slot.itemNo, slot.itemInSlot.fuelMax);
						}
					} else if (EquipWindow.equip.hatSlot.itemNo == EquipWindow.equip.minersHelmet.getItemId() || EquipWindow.equip.hatSlot.itemNo == EquipWindow.equip.emptyMinersHelmet.getItemId()) {
						EquipWindow.equip.hatSlot.stack = EquipWindow.equip.hatSlot.itemInSlot.fuelMax;
					}
				}
				return true;
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_Inventory_Update.Prefix ERROR - " + e.StackTrace);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Inventory), "damageAllTools")]
	class HarmonyPatch_Inventory_damageAllTools {

		private static bool Prefix(ref Inventory __instance) {
			return !m_enabled.Value;
		}
	}

	[HarmonyPatch(typeof(DailyTaskGenerator), "generateNewDailyTasks")]
	class HarmonyPatch_DailyTaskGenerator_generateNewDailyTasks {

		private static bool Prefix(
			DailyTaskGenerator __instance,
			ref int[] ___loadedTaskCompletion
		) {
			try {
				if (!m_enabled.Value) {
					return true;
				}
				UnityEngine.Random.InitState(NetworkMapSharer.Instance.mineSeed + NetworkMapSharer.Instance.tomorrowsMineSeed);
				__instance.doublesCheck.Clear();
				__instance.currentTasks = new Task[3];
				int taskIdMax = Enum.GetNames(typeof(DailyTaskGenerator.genericTaskType)).Length;
				for (int i = 0; i < 3; i++)
				{
					for (;;) {
						__instance.currentTasks[i] = new Task(taskIdMax);
						if (__instance.currentTasks[i].taskTypeId != (int) DailyTaskGenerator.genericTaskType.BreakATool) {
							break;
						}
					}
					__instance.doublesCheck.Add(__instance.currentTasks[i].taskTypeId);
					__instance.taskIcons[i].fillWithDetails(__instance.currentTasks[i]);
					__instance.taskIcons[i].gameObject.SetActive(value: true);
				}
				CurrencyWindows.currency.closeJournal();
				__instance.GetType().GetMethod("loadDailyTaskCompletion", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] {});
				return false;
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_DailyTaskGenerator_generateNewDailyTasks.Prefix ERROR - " + e.StackTrace);
			}
			return true;
		}
	}
}