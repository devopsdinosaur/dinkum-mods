using HarmonyLib;

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
