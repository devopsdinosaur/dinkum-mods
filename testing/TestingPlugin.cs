
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
public class UnbreakableToolsPlugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.testing");
	public static ManualLogSource logger;
	private static ConfigEntry<bool> m_enabled;
	
	private void Awake() {
		logger = this.Logger;
		try {
			m_enabled = this.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
			if (m_enabled.Value) {
				this.m_harmony.PatchAll();
			}
			logger.LogInfo($"devopsdinosaur.dinkum.testing v0.0.0{(m_enabled.Value ? "" : " [inactive; disabled in config]")} loaded.");
		} catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e.StackTrace);
		}
	}

	public static bool list_descendants(Transform parent, Func<Transform, bool> callback, int indent) {
		Transform child;
		string indent_string = "";
		for (int counter = 0; counter < indent; counter++) {
			indent_string += " => ";
		}
		for (int index = 0; index < parent.childCount; index++) {
			child = parent.GetChild(index);
			logger.LogInfo(indent_string + child.gameObject.name);
			if (callback != null) {
				if (callback(child) == false) {
					return false;
				}
			}
			list_descendants(child, callback, indent + 1);
		}
		return true;
	}

	public static bool enum_descendants(Transform parent, Func<Transform, bool> callback) {
		Transform child;
		for (int index = 0; index < parent.childCount; index++) {
			child = parent.GetChild(index);
			if (callback != null) {
				if (callback(child) == false) {
					return false;
				}
			}
			enum_descendants(child, callback);
		}
		return true;
	}

	public static void list_component_types(Transform obj) {
		foreach (Component component in obj.GetComponents<Component>()) {
			logger.LogInfo(component.GetType().ToString());
		}
	}

	private class PluginUpdater : MonoBehaviour {

		private static PluginUpdater m_instance = null;
		public static PluginUpdater Instance {
			get {
				return m_instance;
			}
		}
		private class UpdateInfo {
			public string name;
			public float frequency;
			public float elapsed;
			public Action action;
		}
		private List<UpdateInfo> m_actions = new List<UpdateInfo>();

		public static PluginUpdater create(GameObject parent) {
			if (m_instance != null) {
				return m_instance;
			}
			return (m_instance = parent.AddComponent<PluginUpdater>());
		}

		public void register(string name, float frequency, Action action) {
			m_actions.Add(new UpdateInfo {
				name = name,
				frequency = frequency,
				elapsed = frequency,
				action = action
			});
		}

		public void Update() {
			foreach (UpdateInfo info in m_actions) {
				if ((info.elapsed += Time.deltaTime) >= info.frequency) {
					info.elapsed = 0f;
					try {
						info.action();
					} catch (Exception e) {
						logger.LogError($"PluginUpdater.Update.{info.name} Exception - {e.StackTrace}");
					}
				}
			}
		}
	}

	public static void print_stack() {
		for (int index = 0; ; index++) {
			try {
				StackFrame frame = new StackFrame(index);
				logger.LogInfo($"StackFrame[{index}] - file: {frame.GetFileName()}, line: {frame.GetFileLineNumber()}, method: {frame.GetMethod().Name}");
			} catch {
				break;
			}
		}
	}

	[HarmonyPatch(typeof(WorldManager), "Awake")]
	class HarmonyPatch_WorldManager_Awake {

		private static bool Prefix(WorldManager __instance) {
			try {
				PluginUpdater.create(__instance.gameObject);
				PluginUpdater.Instance.register("testing_update", 1f, testing_update);
				return true;
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_WorldManager_Awake.Prefix ERROR - " + e.StackTrace);
			}
			return true;
		}
	}

	private static void testing_update() {
		if (!m_enabled.Value) {
			return;
		}

	}

}