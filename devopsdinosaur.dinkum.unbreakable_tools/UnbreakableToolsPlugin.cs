
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;


[BepInPlugin("devopsdinosaur.dinkum.unbreakable_tools", "Unbreakable Tools", "0.0.3")]
public class UnbreakableToolsPlugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.unbreakable_tools");
	public static ManualLogSource logger;
	private static ConfigEntry<bool> m_enabled;
	const float CHECK_FREQUENCY = 1.0f;
	static float m_elapsed = CHECK_FREQUENCY;

	private void Awake() {
		logger = this.Logger;
		try {
			m_enabled = this.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
			if (m_enabled.Value) {
				this.m_harmony.PatchAll();
			}
			logger.LogInfo("devopsdinosaur.dinkum.unbreakable_tools v0.0.3 " + (m_enabled.Value ? "" : "[inactive; disabled in config]") + " loaded.");
		} catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e);
		}
	}

	private void Start() {
		foreach (InventoryItem item in Inventory.inv.allItems) {
			if (item.isATool) {
				item.fuelOnUse = 0;
			}
		}
	}

	private void Update() {
		try {
			if (!m_enabled.Value || (m_elapsed += Time.fixedDeltaTime) < CHECK_FREQUENCY) {
				return;
			}
			m_elapsed = 0f;
			for (int i = 0; i < Inventory.inv.invSlots.Length; i++) {
				InventorySlot slot = Inventory.inv.invSlots[i];
				if (slot.itemNo != -1 && Inventory.inv.allItems[slot.itemNo].isATool) {
					if (slot.stack < slot.itemInSlot.fuelMax) {
						slot.updateSlotContentsAndRefresh(slot.itemNo, slot.itemInSlot.fuelMax);
					}
				}
			}
			return;
		} catch (Exception e) {
			logger.LogError("** Update ERROR - " + e);
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

		private static bool Prefix(ref int[] ___loadedTaskCompletion, ref DailyTaskGenerator __instance) {
			try {
				// Need to make sure BreakATool is not selected as a daily task.

				UnityEngine.Random.InitState(NetworkMapSharer.share.mineSeed + NetworkMapSharer.share.tomorrowsMineSeed);
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
				if (___loadedTaskCompletion != null) {
					for (int i = 0; i < ___loadedTaskCompletion.Length; i++) {
						__instance.currentTasks[i].points = ___loadedTaskCompletion[i];
						__instance.taskIcons[i].fillWithDetails(__instance.currentTasks[i]);
					}
				}
				if (___loadedTaskCompletion != null) {
					for (int j = 0; j < ___loadedTaskCompletion.Length; j++) {
						___loadedTaskCompletion[j] = 0;
					}
				}
				return false;
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_DailyTaskGenerator_generateNewDailyTasks_Prefix ERROR - " + e);
			}
			return true;
		}
	}
}