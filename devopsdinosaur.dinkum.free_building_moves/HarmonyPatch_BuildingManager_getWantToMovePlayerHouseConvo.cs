using HarmonyLib;

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
