
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;


[BepInPlugin("devopsdinosaur.dinkum.no_more_watering", "No More Watering", "0.0.2")]
public class NoMoreWateringPlugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.no_more_watering");
	public static ManualLogSource logger;
	private static ConfigEntry<bool> m_enabled;
	
	private void Awake() {
		logger = this.Logger;
		try {
			m_enabled = this.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
			if (m_enabled.Value) {
				this.m_harmony.PatchAll();
			}
			logger.LogInfo("devopsdinosaur.dinkum.no_more_watering v0.0.2 " + (m_enabled.Value ? "" : "[inactive; disabled in config]") + " loaded.");
		} catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e);
		}
	}

	[HarmonyPatch(typeof(TileObjectGrowthStages), "checkIfShouldGrow")]
	class HarmonyPatch_TileObjectGrowthStages_checkIfShouldGrow {

		private static bool Prefix(int xPos, int yPos, ref TileObjectGrowthStages __instance) {
			try {
				if (!m_enabled.Value) {
					return true;
				}
				if (__instance.needsTilledSoil) {
					WorldManager.manageWorld.tileTypeMap[xPos, yPos] = (int) TileTypes.tiles.WetTilledDirtFertilizer;
				}
				return true;
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_TileObjectGrowthStages_checkIfShouldGrow_Prefix ERROR - " + e);
			}
			return true;
		}
	}
}