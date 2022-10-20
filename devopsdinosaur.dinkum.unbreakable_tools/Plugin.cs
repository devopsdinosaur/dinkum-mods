
using BepInEx;
using HarmonyLib;
using System;


[BepInPlugin("devopsdinosaur.dinkum.unbreakable_tools", "Unbreakable Tools", "0.0.1")]
public class Plugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.unbreakable_tools");
	
	public Plugin() {
	}

	private void Awake() {
		this.m_harmony.PatchAll();
	}

	private void Start() {
		foreach (InventoryItem item in Inventory.inv.allItems) {
			if (item.isATool) {
				item.fuelOnUse = 0;
			}
		}
	}

	[HarmonyPatch(typeof(Inventory), "damageAllTools")]
	class HarmonyPatch_Inventory_damageAllTools {

		private static bool Prefix(ref Inventory __instance) {
			return false;
		}
	}

	[HarmonyPatch(typeof(Inventory), "checkIfToolNearlyBroken")]
	class HarmonyPatch_Inventory_checkIfToolNearlyBroken {

		private static bool Prefix(ref bool __result, ref Inventory __instance) {
			for (int i = 0; i < __instance.invSlots.Length; i++) {
				if (__instance.invSlots[i].itemNo != -1 && __instance.allItems[__instance.invSlots[i].itemNo].isATool) {
					__instance.invSlots[i].stack = __instance.invSlots[i].itemInSlot.fuelMax;
				}
			}
			__result = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(DailyTaskGenerator), "generateNewDailyTasks")]
	class HarmonyPatch_DailyTaskGenerator_generateNewDailyTasks {

		private static bool Prefix(ref int[] ___loadedTaskCompletion, ref DailyTaskGenerator __instance) {
			
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
		}
	}
}