// DeathRace file

registerInputEvent("fxDTSBrick", "onBotTouch", "Self fxDTSBrick" TAB "Bot Bot" TAB "Player Player" TAB "Client GameConnection" TAB "MiniGame MiniGame");
registerOutputEvent("Player", "addNewItem","string 50 50");

exec("add-ons/GameMode_DeathRace/support/Support_AntiMulticlient.cs");
exec("add-ons/GameMode_DeathRace/support/Support_FindItemByName.cs");
exec("add-ons/GameMode_DeathRace/support/Support_FindPlayertypeByName.cs");
exec("add-ons/GameMode_DeathRace/support/Support_MissingFunctions.cs");
exec("add-ons/GameMode_DeathRace/support/Support_NewHealth.cs");
exec("add-ons/GameMode_DeathRace/support/Support_NewStereo.cs");
exec("add-ons/GameMode_DeathRace/support/Support_NoTools.cs");
exec("add-ons/GameMode_DeathRace/support/Support_PlayerEmitter.cs");
exec("add-ons/GameMode_DeathRace/support/Support_Shop.cs");
exec("add-ons/GameMode_DeathRace/support/Support_SimObject.cs");
exec("add-ons/GameMode_DeathRace/support/Support_SpeedFactor.cs");

exec("add-ons/GameMode_DeathRace/src/prefs.cs");
exec("add-ons/GameMode_DeathRace/src/packages.cs");
exec("add-ons/GameMode_DeathRace/src/datablocks.cs");
exec("add-ons/GameMode_DeathRace/src/commands.cs");
exec("add-ons/GameMode_DeathRace/src/functions.cs");
exec("add-ons/GameMode_DeathRace/src/functions - bricks.cs");
exec("add-ons/GameMode_DeathRace/src/functions - minigame.cs");
exec("add-ons/GameMode_DeathRace/src/functions - player.cs");
exec("add-ons/GameMode_DeathRace/src/functions - vehicle.cs");
exec("add-ons/GameMode_DeathRace/src/player data.cs");
exec("add-ons/GameMode_DeathRace/src/minigame - loop.cs");
exec("add-ons/GameMode_DeathRace/src/minigame - specials.cs");
exec("add-ons/GameMode_DeathRace/src/shop/main.cs");

exec("add-ons/GameMode_DeathRace/src/minigame core/main.cs");
function DR_NewMinigame()
{
	if(isObject($DefaultMinigame) && !$DefaultMinigame.isCustomMini)
	{	
		$DefaultMinigame.endGame();
		$DefaultMinigame.delete();
	}

	if(!isObject($DefaultMinigame))
		createCustomMinigame("DRMini", "Deathrace", "CustomMinigameSO", "PlayerMultiSlotNoJetArmor");
	else
		echo("DeathRace Minigame already exists.");
}
schedule(0, 0, DR_NewMinigame);

exec("add-ons/GameMode_DeathRace/src/map system/main.cs");

exec("add-ons/GameMode_DeathRace/src/player - titles.cs");
exec("add-ons/GameMode_DeathRace/src/player - achievements.cs");
exec("add-ons/GameMode_DeathRace/src/player - leaderboard.cs");

PlayerMultiSlotNoJetArmor.useCustomPainEffects = 1;
PlayerMultiSlotNoJetArmor.deathSound           = DeathCrySound;
PlayerMultiSlotNoJetArmor.painSound            = PainCrySound;

