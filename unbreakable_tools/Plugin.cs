
using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.IO;


[BepInPlugin("devopsdinosaur.dinkum.unbreakable_tools", "Unbreakable Tools", "0.0.1")]
public class Plugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.unbreakable_tools");
	public static ManualLogSource logger;

	private ConfigEntry<bool> m_is_enabled;
	private ConfigEntry<float> m_durability_loss_multiplier;


	public Plugin() {
	}

	private void Awake() {
		Plugin.logger = this.Logger;
		logger.LogInfo((object) "devopsdinosaur.dinkum.unbreakable_tools v0.0.1 loaded.");
		this.m_harmony.PatchAll();
		this.m_is_enabled = this.Config.Bind<bool>("General", "Enabled", true, "Set this to false to disable this mod.");
		this.m_durability_loss_multiplier = this.Config.Bind<float>("Options", "Durability Loss Multiplier", 0.0f, "Multiplied times 'fuelOnUse' field to calculate amount of durability loss for each use of the tool.\n- A value of 1 indicates normal behavior\n- Less than one reduces durability loss (ex: 0.5 reduces loss to 1/2)\n- 0 (default) removes all durability loss.");
	}

	private void Start() {
		if (!this.m_is_enabled.Value) {
			return;
		}
		foreach (InventoryItem item in Inventory.inv.allItems) {
			if (item.isATool) {
				logger.LogInfo((object) item.itemName);
				//logger.LogInfo((object) this.m_durability_loss_multiplier.Value.ToString());
				//item.fuelOnUse = (int) Math.Floor((float) item.fuelOnUse * this.m_durability_loss_multiplier.Value);
				item.fuelOnUse = 0;
				//logger.LogInfo((object) item.fuelOnUse.ToString());
			}
		}
	}
}