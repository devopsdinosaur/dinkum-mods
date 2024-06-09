
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;


[BepInPlugin("devopsdinosaur.dinkum.weapons_wont_kill_crops", "Weapons Wont Kill Crops", "0.0.4")]
public class WeaponsWontKillCropsPlugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.weapons_wont_kill_crops");
	public static ManualLogSource logger;
	private static ConfigEntry<bool> m_enabled;
	private static ConfigEntry<string> m_match_words;
	
	private void Awake() {
		logger = this.Logger;
		try {
			m_enabled = this.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
			m_match_words = this.Config.Bind<string>("Global", "Tool Word Matches", "Axe,Chainsaw,Compactor,Scythe,Shovel", "If tool/weapon name contains any of these words it will be considered valid for hitting plants; separate by commas, case sensitive, no spaces!");
			if (m_enabled.Value) {
				this.m_harmony.PatchAll();
			}
			logger.LogInfo($"devopsdinosaur.dinkum.weapons_wont_kill_crops v0.0.4{(m_enabled.Value ? "" : " [inactive; disabled in config]")} loaded.");
		} catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e);
		}
	}

	[HarmonyPatch(typeof(WorldManager), "Start")]
	class HarmonyPatch_WorldManager_Start {

		private static bool Prefix() {
			try {
				string[] match_words = m_match_words.Value.Split(',');
				bool is_match;
				foreach (InventoryItem item in Inventory.Instance.allItems) {
					if (item.isATool) {
						if (item.damageSmallPlants) {
							is_match = false;
							foreach (string word in match_words) {
								if (item.itemName.Contains(word)) {
									is_match = true;
									break;
								}
							}
							item.damageSmallPlants = is_match;
						}
					}
				}
				return true;
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_WorldManager_Start.Prefix ERROR - " + e.StackTrace);
			}
			return true;
		}
	}
}