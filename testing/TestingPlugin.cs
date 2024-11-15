
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

[BepInPlugin("devopsdinosaur.dinkum.testing", "Testing", "0.0.0")]
public class TestPlugin : DDPlugin {
	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.testing");
	private static ConfigEntry<bool> m_enabled;
	
	private void Awake() {
		logger = this.Logger;
		try {
			m_enabled = this.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
			DDPlugin.set_log_level(DDPlugin.LogLevel.Debug);
			this.m_harmony.PatchAll();
			logger.LogInfo((object) $"devopsdinosaur.dinkum.testing v0.0.0{(m_enabled.Value ? "" : " [inactive; disabled in config]")} loaded.");
		} catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e.StackTrace);
		}
	}

	[HarmonyPatch(typeof(Inventory), "getAmountOfItemInAllSlots")]
	class HarmonyPatch_Inventory_getAmountOfItemInAllSlots {
		private static bool Prefix(ref int __result, int itemId) {
			if (itemId == Inventory.Instance.getInvItemId(Inventory.Instance.minePass) || itemId == MineEnterExit.mineEntrance.rubyShard.getItemId() || itemId == MineEnterExit.mineEntrance.emeraldShard.getItemId()) {
				__result = 9999;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(AnimalAI), "setUp")]
	class HarmonyPatch_AnimalAI_setUp {
		private static void Postfix(ref AnimalAI_Attack ___attacks) {
			___attacks = null;
		}
	}

	/*
	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			try {

			} catch (Exception e) {
				DDPlugin._error_log("** HarmonyPatch_.Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			try {

			} catch (Exception e) {
				DDPlugin._error_log("** HarmonyPatch_.Postfix ERROR - " + e);
			}
		}
	}
	*/
}