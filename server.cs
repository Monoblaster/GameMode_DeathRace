// DeathRace file
$DR::SaveSlot = 1;
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
exec("add-ons/GameMode_DeathRace/support/print.cs");
exec("add-ons/GameMode_DeathRace/support/datainstance.cs");

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

$Server::Name = "DeathRace";
$Pref::Server::MaxPlayers = 48;

HorseArmor.maxForwardSpeed = 30;
gc_PumaVehicle.maxWheelSpeed = 30;

function createGameModeMusicDataBlocks ()
{
	%i = 0;
	while (%i < $GameMode::MusicCount)
	{
		%filename = "Add-Ons/Music/" @ $GameMode::Music[%i] @ ".ogg";
		%base = fileBase (%filename);
		%uiName = strreplace (%base, "_", " ");
		%varName = getSafeVariableName (%base);
		if (!$Server::Dedicated)
		{
			if (getRealTime () - $lastProgressBarTime > 200)
			{
				LoadingProgress.setValue (%i / %fileCount);
				$lastProgressBarTime = getRealTime ();
				Canvas.repaint ();
			}
		}
		if (!isFile (%filename))
		{
			error ("ERROR: createGameModeMusicDataBlocks() - file \'" @ %filename @ "\' does not exist");
		}
		else if (!isValidMusicFilename (%filename))
		{
			
		}
		// else if (getFileLength (%filename) > 1048576)
		// {
		// 	error ("ERROR: createGameModeMusicDataBlocks() - Music file \"" @ %filename @ "\" > 1mb - ignoring");
		// }
		else 
		{
			%dbName = "musicData_" @ %varName;
			%command = "datablock AudioProfile(" @ %dbName @ ") {" @ "filename = \"" @ %filename @ "\";" @ "description = AudioMusicLooping3d;" @ "preload = true;" @ "uiName = \"" @ %uiName @ "\";" @ "};";
			eval (%command);
			if (%dbName.isStereo ())
			{
				error ("ERROR: createGameModeMusicDataBlocks() - Stereo sound detected on \"" @ %dbName.getName () @ "\" - Removing datablock.");
				schedule (1000, 0, MessageAll, '', "Stereo sound detected on  \"" @ fileName (%dbName.fileName) @ "\" - Removing datablock.");
				%dbName.uiName = "";
				%dbName.delete ();
				if (getBuildString () $= "Ship")
				{
					fileDelete (%filename);
				}
				else 
				{
					warning ("WARNING: \'" @ %filename @ "\' is a stereo music block and would be deleted if this was the public build!");
				}
			}
		}
		%i += 1;
	}
}

//load music
$GameMode::MusicCount = 0;
$file = findFirstFile("Add-ons/music/*.ogg");
while(isFile($file))
{
	$GameMode::Music[$GameMode::MusicCount] = fileBase($file);
	$GameMode::MusicCount += 1;
	$file = findNextFile("Add-ons/music/*.ogg");
}
createGameModeMusicDataBlocks();