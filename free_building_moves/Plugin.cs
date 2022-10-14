
using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Reflection;


[BepInPlugin("devopsdinosaur.dinkum.free_building_moves", "Free Building Moves", "0.0.1")]
public class Plugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.free_building_moves");

	public static ManualLogSource logger;

	public Plugin() {
	}

	private void Awake() {
		logger = this.Logger;
		logger.LogInfo((object) "devopsdinosaur.dinkum.free_building_moves v0.0.1 loaded.");
		//try {
			this.m_harmony.PatchAll();
		//} catch (System.Reflection.ReflectionTypeLoadException e) {
		//	logger.LogError((object) "there was an exception...");
		//}
	}

	private void Start() {
	}
}