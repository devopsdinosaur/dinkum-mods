using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public static class PluginInfo {

	public const string TITLE = "Weapons Won't Kill Crops";
	public const string NAME = "weapons_wont_kill_crops";
	public const string SHORT_DESCRIPTION = "No more accidentally killing crops with your weapon! Disables all weapons' ability to attack plants and grasses.";

	public const string VERSION = "0.0.7";

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
public class TestingPlugin : DDPlugin {
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

	[HarmonyPatch(typeof(WorldManager), "Start")]
	class HarmonyPatch_WorldManager_Start {

		private static bool Prefix() {
			try {
				string[] match_words = Settings.m_match_words.Value.Split(',');
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

	[HarmonyPatch(typeof(CharInteract), "CheckIfCanDamage")]
	class HarmonyPatch_CharInteract_CheckIfCanDamage {
		private static bool Prefix(CharInteract __instance, Vector2 selectedTile, ref bool __result) {
			try {

				// Barrel is considered a 'small plant', so need to patch over the method that checks for hit.

				if (!Settings.m_enabled.Value) {
					return true;
				}
				bool __check_if_can_damage__(CharInteract __instance, Vector2 selectedTile) {
					int num = (int) selectedTile.x;
					int num2 = (int) selectedTile.y;
					if ((bool) __instance.myEquip.itemCurrentlyHolding && !__instance.myEquip.itemCurrentlyHolding.anyHeight && (!(__instance.myEquip.transform.position.y <= (float) WorldManager.Instance.heightMap[num, num2] + 1.5f) || !(__instance.myEquip.transform.position.y >= (float) WorldManager.Instance.heightMap[num, num2] - 1.5f))) {
						return false;
					}
					if (!__instance.myEquip.itemCurrentlyHolding) {
						return false;
					}
					if (__instance.myEquip.itemCurrentlyHolding.placeOnWaterOnly && !WorldManager.Instance.waterMap[num, num2]) {
						return false;
					}
					if (((bool) __instance.myEquip.itemCurrentlyHolding.placeable && WorldManager.Instance.onTileMap[num, num2] == -1) || ((bool) __instance.myEquip.itemCurrentlyHolding.placeable && WorldManager.Instance.onTileMap[num, num2] == 30) || ((bool) __instance.myEquip.itemCurrentlyHolding.placeable && WorldManager.Instance.onTileMap[num, num2] > -1 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[num, num2]].isGrass)) {
						if (__instance.myEquip.itemCurrentlyHolding.canBePlacedOntoTileType.Length == 0) {
							return true;
						}
						for (int i = 0; i < __instance.myEquip.itemCurrentlyHolding.canBePlacedOntoTileType.Length; i++) {
							if (__instance.myEquip.itemCurrentlyHolding.canBePlacedOntoTileType[i] == WorldManager.Instance.tileTypeMap[num, num2]) {
								return true;
							}
						}
						return false;
					}
					if (!__instance.myEquip.itemCurrentlyHolding.ignoreOnTileObject && WorldManager.Instance.onTileMap[num, num2] > -1 && WorldManager.Instance.onTileMap[num, num2] != 30) {
						TileObjectSettings tileObjectSettings = WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[num, num2]];
						if (__instance.myEquip.itemCurrentlyHolding.placeableTileType > -1 && WorldManager.Instance.tileTypes[__instance.myEquip.itemCurrentlyHolding.placeableTileType].isPath && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[num, num2]].isGrass) {
							return true;
						}
						if (__instance.myEquip.itemCurrentlyHolding.placeableTileType > -1 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[num, num2]].isGrass && (WorldManager.Instance.tileTypes[__instance.myEquip.itemCurrentlyHolding.placeableTileType].isTilledDirt || WorldManager.Instance.tileTypes[__instance.myEquip.itemCurrentlyHolding.placeableTileType].isFertilizedDirt || WorldManager.Instance.tileTypes[__instance.myEquip.itemCurrentlyHolding.placeableTileType].isWetFertilizedDirt || WorldManager.Instance.tileTypes[__instance.myEquip.itemCurrentlyHolding.placeableTileType].isWetTilledDirt)) {
							return true;
						}
						if ((tileObjectSettings.isStone && __instance.myEquip.itemCurrentlyHolding.damageStone) || (tileObjectSettings.isHardStone && __instance.myEquip.itemCurrentlyHolding.damageHardStone) || (tileObjectSettings.isWood && __instance.myEquip.itemCurrentlyHolding.damageWood) || (tileObjectSettings.isHardWood && __instance.myEquip.itemCurrentlyHolding.damageHardWood) || (tileObjectSettings.isMetal && __instance.myEquip.itemCurrentlyHolding.damageMetal) || (tileObjectSettings.isSmallPlant && __instance.myEquip.itemCurrentlyHolding.damageSmallPlants) || (tileObjectSettings.tileObjectId == 188 && __instance.myEquip.itemCurrentlyHolding.weaponDamage > 0)) {
							return true;
						}
						return false;
					}
					if (__instance.myEquip.itemCurrentlyHolding.ignoreOnTileObject || WorldManager.Instance.onTileMap[num, num2] == -1 || WorldManager.Instance.onTileMap[num, num2] == 30 || (WorldManager.Instance.onTileMap[num, num2] > -1 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[num, num2]].isGrass)) {
						if (__instance.myEquip.itemCurrentlyHolding.canDamagePath && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isPath) {
							return true;
						}
						if (__instance.myEquip.itemCurrentlyHolding.grassGrowable && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isGrassGrowable) {
							return true;
						}
						if (__instance.myEquip.itemCurrentlyHolding.canDamageDirt && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isDirt) {
							if (__instance.myEquip.itemCurrentlyHolding.changeToHeightTiles != 0 && (__instance.myEquip.itemCurrentlyHolding.changeToHeightTiles != -1 || WorldManager.Instance.heightMap[num, num2] <= -5)) {
								if (__instance.myEquip.itemCurrentlyHolding.changeToHeightTiles != 1) {
									return true;
								}
								_ = WorldManager.Instance.heightMap[num, num2];
								_ = 15;
							}
							return true;
						}
						if (__instance.myEquip.itemCurrentlyHolding.canDamageStone && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isStone) {
							return true;
						}
						if (__instance.myEquip.itemCurrentlyHolding.canDamageTilledDirt && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isTilledDirt) {
							return true;
						}
						if (__instance.myEquip.itemCurrentlyHolding.canDamageWetTilledDirt && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isWetTilledDirt) {
							return true;
						}
						if (__instance.myEquip.itemCurrentlyHolding.canDamageFertilizedSoil && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isFertilizedDirt) {
							return true;
						}
						if (__instance.myEquip.itemCurrentlyHolding.canDamageWetFertilizedSoil && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isWetFertilizedDirt) {
							return true;
						}
					}
					return false;
				}
				__result = __check_if_can_damage__(__instance, selectedTile);
				return false;
			} catch (Exception e) {
				DDPlugin._error_log("** HarmonyPatch_CharInteract_CheckIfCanDamage.Prefix ERROR - " + e);
			}
			return true;
		}
	}
}
