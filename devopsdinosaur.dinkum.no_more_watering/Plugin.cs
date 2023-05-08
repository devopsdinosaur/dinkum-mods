
using BepInEx;
using HarmonyLib;


[BepInPlugin("devopsdinosaur.dinkum.no_more_watering", "No More Watering", "0.0.2")]
public class Plugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.no_more_watering");
	
	public Plugin() {
	}

	private void Awake() {
		this.m_harmony.PatchAll();
	}

	[HarmonyPatch(typeof(TileObjectGrowthStages), "checkIfShouldGrow")]
	class HarmonyPatch_TileObjectGrowthStages_checkIfShouldGrow {

		private static bool Prefix(int xPos, int yPos, ref TileObjectGrowthStages __instance) {
			if (__instance.needsTilledSoil) {
				WorldManager.manageWorld.tileTypeMap[xPos, yPos] = (int) TileTypes.tiles.WetTilledDirtFertilizer;
			}
			return true;
		}
	}
}