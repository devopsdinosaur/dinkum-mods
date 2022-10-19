
using BepInEx;
using HarmonyLib;


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
}