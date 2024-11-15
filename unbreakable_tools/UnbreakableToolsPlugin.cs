using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public static class PluginInfo {

	public const string TITLE = "Unbreakable Tools";
	public const string NAME = "unbreakable_tools";
	public const string SHORT_DESCRIPTION = "Tools lose no durability or fuel on use and will not lose durability on revive. Miner's helmet has infinite charge";

	public const string VERSION = "0.0.8";

	public const string AUTHOR = "devopsdinosaur";
	public const string GAME_TITLE = "Dinkum";
	public const string GAME = "dinkum";
	public const string GUID = AUTHOR + "." + GAME + "." + NAME;
	public const string REPO = "dinkum-mods";

	public static Dictionary<string, string> to_dict() {
		Dictionary<string, string> info = new Dictionary<string, string>();
		foreach (FieldInfo field in typeof(PluginInfo).GetFields((BindingFlags) 0xFFFFFFF)) {
			info[field.Name.ToLower()] = (string) field.GetValue(null);
		}
		return info;
	}
}

[BepInPlugin(PluginInfo.GUID, PluginInfo.TITLE, PluginInfo.VERSION)]
public class UnbreakableToolsPlugin : DDPlugin {
	private Harmony m_harmony = new Harmony(PluginInfo.GUID);

	private void Awake() {
		logger = this.Logger;
		try {
            this.m_plugin_info = PluginInfo.to_dict();
            Settings.Instance.load(this);
            DDPlugin.set_log_level(Settings.m_log_level.Value);
            this.create_nexus_page();
            this.m_harmony.PatchAll();
            logger.LogInfo($"{PluginInfo.GUID} v{PluginInfo.VERSION} loaded.");
        } catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e);
		}
	}

	[HarmonyPatch(typeof(Inventory), "Update")]
	class HarmonyPatch_Inventory_Update {

		private const float CHECK_FREQUENCY = 1.0f;
		private static float m_elapsed = CHECK_FREQUENCY;

		private static bool Prefix(Inventory __instance) {
			try {
				InventorySlot slot;
				if (!Settings.m_enabled.Value || (m_elapsed += Time.fixedDeltaTime) < CHECK_FREQUENCY) {
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
			return !Settings.m_enabled.Value;
		}
	}

	[HarmonyPatch(typeof(DailyTaskGenerator), "generateNewDailyTasks")]
	class HarmonyPatch_DailyTaskGenerator_generateNewDailyTasks {

		private static bool Prefix(
			DailyTaskGenerator __instance,
			ref int[] ___loadedTaskCompletion
		) {
			try {
				if (!Settings.m_enabled.Value) {
					return true;
				}
				UnityEngine.Random.InitState(NetworkMapSharer.Instance.mineSeed + NetworkMapSharer.Instance.tomorrowsMineSeed);
				__instance.doublesCheck.Clear();
				__instance.currentTasks = new Task[3];
				int taskIdMax = Enum.GetNames(typeof(DailyTaskGenerator.genericTaskType)).Length;
				for (int i = 0; i < 3; i++) {
					for (; ; ) {
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
				__instance.GetType().GetMethod("loadDailyTaskCompletion", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { });
				return false;
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_DailyTaskGenerator_generateNewDailyTasks.Prefix ERROR - " + e.StackTrace);
			}
			return true;
		}
	}
}
