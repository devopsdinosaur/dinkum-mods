
using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.IO;


[BepInPlugin("unbreakable_tools", "Unbreakable Tools", "0.0.1")]
public class Plugin : BaseUnityPlugin {

	public static string NAME = "unbreakable_tools";
	public static string TITLE = "Unbreakable Tools";
	public static string VERSION = "0.0.1";
	public static string AUTHOR = "0.0.1";

	private Harmony m_harmony = new Harmony(NAME);
	public static ManualLogSource logger;

	private ConfigEntry<bool> m_is_enabled;


	public Plugin() {
	}

	private void info_log(string text) {
		Plugin.logger.LogInfo((object) text);
	}

	private void Awake() {
		Plugin.logger = this.Logger;
		this.info_log(string.Format("'%s' v%s plugin loaded.", TITLE, VERSION));
		this.m_harmony.PatchAll();
		this.m_is_enabled = this.Config.Bind<bool>("General", "Enabled", true, "Set this to false to disable this mod.");

	}

	private void Start() {
		
	}
}