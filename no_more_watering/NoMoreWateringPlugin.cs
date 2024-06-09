
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;


[BepInPlugin("devopsdinosaur.dinkum.no_more_watering", "No More Watering", "0.0.4")]
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
			logger.LogInfo($"devopsdinosaur.dinkum.no_more_watering v0.0.4{(m_enabled.Value ? "" : " [inactive; disabled in config]")} loaded.");
		} catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e.StackTrace);
		}
	}

	[HarmonyPatch(typeof(TileObjectGrowthStages), "checkIfShouldGrow")]
	class HarmonyPatch_TileObjectGrowthStages_checkIfShouldGrow {

		private static bool Prefix(int xPos, int yPos, TileObjectGrowthStages __instance) {
			try {
				if (m_enabled.Value && __instance.needsTilledSoil) {
					WorldManager.Instance.tileTypeMap[xPos, yPos] = (int) TileTypes.tiles.WetTilledDirtFertilizer;
				}
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_TileObjectGrowthStages_checkIfShouldGrow_Prefix ERROR - " + e.StackTrace);
			}
			return true;
		}
	}
}