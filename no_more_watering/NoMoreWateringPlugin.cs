using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;

public static class PluginInfo {

	public const string TITLE = "No More Watering";
	public const string NAME = "no_more_watering";
	public const string SHORT_DESCRIPTION = "Crops are automatically watered and fertilized during the night, making the watering can and fertilizer obsolete!";

	public const string VERSION = "0.0.6";

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
public class NoMoreWateringPlugin : DDPlugin {
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

	[HarmonyPatch(typeof(TileObjectGrowthStages), "checkIfShouldGrow")]
	class HarmonyPatch_TileObjectGrowthStages_checkIfShouldGrow {
		private static bool Prefix(int xPos, int yPos, TileObjectGrowthStages __instance) {
			try {
				if (Settings.m_enabled.Value && __instance.needsTilledSoil) {
					WorldManager.Instance.tileTypeMap[xPos, yPos] = (int) TileTypes.tiles.WetTilledDirtFertilizer;
				}
			} catch (Exception e) {
				DDPlugin._error_log("** HarmonyPatch_TileObjectGrowthStages_checkIfShouldGrow.Prefix ERROR - " + e.StackTrace);
			}
			return true;
		}
	}
}
