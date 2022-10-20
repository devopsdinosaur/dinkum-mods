
using BepInEx;
using HarmonyLib;


[BepInPlugin("devopsdinosaur.dinkum.only_scythe_kills_crops", "Only Scythe Kills Crops", "0.0.1")]
public class Plugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.only_scythe_kills_crops");

	public Plugin() {
	}

	private void Awake() {
		this.m_harmony.PatchAll();
	}

	private void Start() {
		foreach (InventoryItem item in Inventory.inv.allItems) {
			if (item.isATool) {
				if (!item.itemName.Contains("Scythe")) {
					item.damageSmallPlants = false;
				}
			}
		}
	}
}