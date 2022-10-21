
using BepInEx;
using HarmonyLib;
using System;


[BepInPlugin("devopsdinosaur.dinkum.unbreakable_tools", "Unbreakable Tools", "0.0.2")]
public class Plugin : BaseUnityPlugin {

	private const int ITEM_FIX_FREQUENCY_MS = 1000;

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.unbreakable_tools");
	private DateTime m_last_update_time = new DateTime(0);
	
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

	private void Update() {
		InventorySlot slot;

		if ((int) ((DateTime.Now - this.m_last_update_time).TotalMilliseconds) < ITEM_FIX_FREQUENCY_MS) {
			return;
		}
		for (int i = 0; i < Inventory.inv.invSlots.Length; i++) {
			slot = Inventory.inv.invSlots[i];
			if (slot.itemNo != -1 && Inventory.inv.allItems[slot.itemNo].isATool) {
				if (slot.stack < slot.itemInSlot.fuelMax) {
					slot.updateSlotContentsAndRefresh(slot.itemNo, slot.itemInSlot.fuelMax);
				}
			}
		}
		this.m_last_update_time = DateTime.Now;
	}

	[HarmonyPatch(typeof(Inventory), "damageAllTools")]
	class HarmonyPatch_Inventory_damageAllTools {

		private static bool Prefix(ref Inventory __instance) {
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