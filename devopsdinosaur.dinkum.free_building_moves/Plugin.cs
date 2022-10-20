
using BepInEx;
using HarmonyLib;


[BepInPlugin("devopsdinosaur.dinkum.free_building_moves", "Free Building Moves", "0.0.1")]
public class Plugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.dinkum.free_building_moves");

	public Plugin() {
	}

	private void Awake() {
		this.m_harmony.PatchAll();
	}

	private void Start() {
	}

	[HarmonyPatch(typeof(BuildingManager), "confirmWantToMoveBuilding")]
	public class HarmonyPatch_BuildingManager_confirmWantToMoveBuilding {

		private static bool Prefix(ref bool ___movingHouse, ref int ___currentlyMoving, ref int ___talkingAboutMovingBuilding, ref BuildingManager __instance) {
			if (!___movingHouse) {
				NetworkMapSharer share = NetworkMapSharer.share;
				share.NetworktownDebt = share.townDebt + 0;
				___currentlyMoving = ___talkingAboutMovingBuilding;
				NetworkMapSharer.share.NetworkmovingBuilding = ___talkingAboutMovingBuilding;
				__instance.giveDeedForBuildingToBeMoved(___talkingAboutMovingBuilding);
			} else {
				Inventory.inv.changeWallet(0);
				__instance.giveDeedForHouseToMove();
				___currentlyMoving = ___talkingAboutMovingBuilding;
				NetworkMapSharer.share.NetworkmovingBuilding = ___talkingAboutMovingBuilding;
			}
			___movingHouse = false;
			return false;
		}

	}

	[HarmonyPatch(typeof(BuildingManager), "getWantToMovePlayerHouseConvo")]
	public class HarmonyPatch_BuildingManager_getWantToMovePlayerHouseConvo {

		private static bool Prefix(
			ref bool ___movingHouse,
			ref int ___currentlyMoving,
			ref Conversation ___houseIsBeingUpgraded,
			ref Conversation ___alreadyMovingABuilding,
			ref Conversation ___noRoomInInv,
			ref Conversation ___wantToMovePlayerHouseNotEnoughMoney,
			ref Conversation ___wantToMovePlayerHouse,
			ref BuildingManager __instance
		) {
			if (TownManager.manage.checkIfHouseIsBeingUpgraded()) {
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ___houseIsBeingUpgraded);
			} else if (___currentlyMoving != -1) {
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ___alreadyMovingABuilding);
			} else if (!Inventory.inv.checkIfItemCanFit(0, 1)) {
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ___noRoomInInv);
			}
			___movingHouse = true;
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ___wantToMovePlayerHouse);
			return false;
		}

	}
}