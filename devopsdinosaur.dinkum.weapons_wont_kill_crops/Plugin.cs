
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;


[BepInPlugin("devopsdinosaur.dinkum.weapons_wont_kill_crops", "Weapons Won't Kill Crops", "0.0.2")]
public class Plugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.weapons_wont_kill_crops");
	private ConfigEntry<string> m_config_match_words;
	
	public Plugin() {
	}

	private void Awake() {
		this.m_harmony.PatchAll();
		this.m_config_match_words = this.Config.Bind<string>("Global", "Tool Word Matches", "Axe,Chainsaw,Compactor,Scythe,Shovel", "If tool/weapon name contains any of these words it will be considered valid for hitting plants; separate by commas, case sensitive, no spaces!");
	}

	private void Start() {
		string[] match_words = this.m_config_match_words.Value.Split(',');
		bool is_match;

		foreach (InventoryItem item in Inventory.inv.allItems) {
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
	}
}