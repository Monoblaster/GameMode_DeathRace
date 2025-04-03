//PUT MAPS, CREDITS, AND ENVIRONMENTS IN "Blockland\config\server\DeathRace\Maps\"
// Environments MUST USE .BLEnvironment format to work (I lied it's just txt, can still use this format)
// Saves obviously must use .bls
// Credits must use .BLCredits format to work
// Make sure the file names MUST be the same as the BLS or it will not recognize them!

//TODO
// Make VoteType setting 3 work

datablock AudioProfile(musicData_JeopardyThemeSong)
{
	filename = "./Jeopardy.ogg";
	description = AudioMusicLooping3d;
	preload = true;
};

exec("./saver.cs");
exec("./centerprintmenu.cs");

function initMapSystem()
{
	//Map system preferences

	// Vote type, please use number code:
	//  0 - Players can vote anytime but at the end of the round they have a chance to make up their mind
	//  1 - You can only vote when the prompt exist, uses the round system
	//  2 - Same as #0 but on a time limit instead, soon as the round ends the map will change, you cannot make up your mind once the timer ends
	//  3 - You can only vote when the prompt exist, uses the time system
	//  4 - Players do not vote at all, the map just rotates. Pretty useless to use this add-on if you're going to do that, but whatever.
	if($Pref::Server::MapSys_VoteType $= "")
		$Pref::Server::MapSys_VoteType = 1;

	if($Pref::server::MapSys_UseBuildingTools $= "")
		$Pref::server::MapSys_UseBuildingTools = 1;

	// Max rounds to change a map if VoteType setting is 0/1/4, please don't use this mode or you're gay - setting 0 is ok though
	if($Pref::Server::MapSys_RoundLimit $= "")
		$Pref::Server::MapSys_RoundLimit = 10;

	// Vote time when VoteType setting is 0 or 1
	// NOT RECOMMENDED TO CHANGE
	if($Pref::Server::MapSys_VoteTimeLimit $= "")
		$Pref::Server::MapSys_VoteTimeLimit = 15;

	// If VoteType setting is 2/3, this will be in use
	//   Number in minutes before the map has to change, which affects right after the new round begins.
	if($Pref::Server::MapSys_MapTimeLimit $= "")
		$Pref::Server::MapSys_MapTimeLimit = 10;

	// This effects how you ghost each map:
	// 0 - Uses the standard method by loading maps normally
	// 1 - Loads all maps but ghosts them differently
	//// NEED TO RESTART SERVER TO TAKE AFFECT
	if($Pref::Server::MapSys_MapGhostMode $= "")
		$Pref::Server::MapSys_MapGhostMode = 0;
	// 1 is obviously WIP because of how tough this will be

	if($Pref::Server::MapSys_MapCenterPrintMenu $= "")
		$Pref::Server::MapSys_MapCenterPrintMenu = 1;

	// STATIC VARIABLES - DO NOT CHANGE OR YOU WILL BREAK THE SERVER
	$Server::MapSys_MapGhostMode = $Pref::Server::MapSys_MapGhostMode;
	$Server::MapSys_Path = "config/server/DeathRace/Maps/";
}
initMapSystem();

//////////////////////////////////////////////////////////////

// This is being commented out because we are going to implement it in our custom minigame system
// if(isPackage("MapSystem"))
// 	deactivatePackage("MapSystem");

// package MapSystem
// {
// 	// We have to package garbage so we don't break anything..
// 	function MinigameSO::Reset(%minigame, %client)
// 	{
// 		// Increment rounds
// 		if($Pref::Server::MapSys_VoteType != 2 && $Pref::Server::MapSys_VoteType != 3)
// 		{
// 			%minigame.round++;
// 			if(%minigame.round > $Pref::Server::MapSys_RoundLimit)
// 			{
// 				%minigame.round = 0;
// 				$Server::Temp::MapSys_Change = 1;
// 			}
// 		}

// 		// Start the tick if VoteType is 2/3
// 		if(!isEventPending($Temp::MapSys_TickSch) && ($Pref::Server::MapSys_VoteType == 2 || $Pref::Server::MapSys_VoteType == 3))
// 		{
// 			%minigame.round = 0;
// 			MapSys_Loop($Pref::Server::MapSys_MapTimeLimit);
// 		}

// 		// Temp setting to trigger the map changer
// 		if($Server::Temp::MapSys_Change)
// 		{
// 			$Server::Temp::MapSys_Change = 0;

// 			cancel($Temp::MapSys_ErrorSch);
// 			// For whatever reason the map fails to change we have to fix it
// 			$Temp::MapSys_ErrorSch = schedule(10000, 0, "MapSys_ResetMap", "MapChangeError");

// 			// Change the map to the next one - either voted or in an order
// 			MapSys_SetMapNext();
// 			return;
// 		}

// 		if(%mini.inMapChange)
// 			return;

// 		return Parent::Reset(%minigame, %client);
// 	}
// };
// activatePackage("MapSystem");

function MapSys_getDisplayTime(%time, %ignoreSeconds, %timestring)
{
	%days = mFloor(%time / 86400);
	%hours = mFloor(%time / 3600);
	%minutes = mFloor((%time % 3600) / 60);
	%seconds = mFloor(%time % 3600 % 60);

	if(%timeString)
	{
		if(%days > 0)
			%nDays = %days @ " day" @ (%days != 1 ? "s" : "");

		if(%hours > 0)
			%nHours = %hours @ " hour" @ (%hours != 1 ? "s" : "");

		if(%minutes > 0)
			%nMinutes = %minutes @ " minute" @ (%minutes != 1 ? "s" : "");

		if(%seconds > 0 && !%ignoreSeconds)
			%nSeconds = %seconds @ " second" @ (%seconds != 1 ? "s" : "");

		%nTimeString = trim(%nDays TAB %nHours TAB %nMinutes TAB %nSeconds);
		%nTimeStringCount = getFieldCount(trim(%nDays TAB %nHours TAB %nMinutes TAB %nSeconds));

		if(%nTimeStringCount <= 0)
			return "0 seconds";

		if(%nTimeStringCount > 1)
		{
			%nTimeStringLast = getField(%nTimeString, %nTimeStringCount-1);
			%nTimeString = getFields(%nTimeString, 0, %nTimeStringCount-2);
		}
		else
			%nTimeString = getField(%nTimeString, 0);

		%nTimeString = strReplace(%nTimeString, "" TAB "", ", ");
		%nTimeString = %nTimeString @ (%nTimeStringLast !$= "" ? " and " @ %nTimeStringLast : "");

		return %nTimeString;
	}

	return %days TAB %hours TAB %minutes TAB %seconds;
}

// Requires Client_DeathRace
function GameConnection::sendMapData(%client)
{
	if(!%client.Shop_Client)
		return;

	commandToClient(%client, 'DRShop', "ClearMaps");
	for(%i = 1; %i <= $Server::MapSys_MapCount; %i++)
	{
		commandToClient(%client, 'DRShop', "AddMap", $Server::MapSys_MapName[%i], "", $Server::MapSys_MapDescription[%i]);
	}
}

function MaySys_SendDataToClients()
{
	%count = ClientGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%client = ClientGroup.getObject(%i);
		if(%client.Shop_Client)
			%client.sendMapData();
	}
}
// End Client_DeathRace

function MapSys_Discover()
{
	%mPath = $Server::MaySys_Path;
	%path = $Server::MapSys_Path @ "*.bls";
	deleteVariables("$Server::MapSys_Map*");
	$Server::MapSys_MapCount = 0;
	for(%bls = findFirstFile(%path); %bls !$= ""; %bls = findNextFile(%path))
	{
		$Server::MapSys_Map[$Server::MapSys_MapCount++] = %bls;
		$Server::MapSys_MapName[$Server::MapSys_MapCount] = fileBase(%bls);

		if(isFile(%descFile = %mPath @ fileBase(%file) @ ".BLDescription"))
   		{
   			%io = new FileObject();
   			%io.openForRead(%descFile);
   			while(!%io.isEOF())
   			{
   				if(%description $= "")
   					%description = %io.readLine();
   				else
   					%description = %description NL %io.readLine();
   			}
   			%io.close();
   			%io.delete();
   		}

   		$Server::MapSys_MapDescription[$Server::MapSys_MapCount] = %description;
	}
	
	MaySys_SendDataToClients();
}
MapSys_Discover();
if(getBrickCount() == 0)
{
	%mapIndex = getRandom(1,$Server::MapSys_MapCount);
	schedule(33, 0, MapSys_SetMapNext, 2, $Server::MapSys_Map[%mapIndex]);
	schedule(33, 0, loadEnvironmentFromFile, $Server::MapSys_Path @ $Server::MapSys_MapName[%mapIndex] @ ".txt", 1);
}

function MapSys_onMapChanged()
{
	$CACHE::MapChanger::DoRTVChange = 0;
	$Temp::MapSys_Changing = 0;
	$DefaultMinigame.round = 0;
	$DefaultMinigame.inMapChange = 0;
	$DefaultMinigame.scheduleReset();
	cancel($Temp::MapSys_TickSch);
}

function MapSys_Loop(%tick)
{
	cancel($Temp::MapSys_TickSch);
	if(%tick <= 0)
	{
		announce("\c6[\c3MP\c6] Time is up! When the round ends you must vote your next map!");
		$Server::Temp::MapSys_Change = 1;
		return;
	}

	$Temp::MapSys_TickSch = schedule(60 * 1000, 0, MapSys_Loop, %tick--); 
}

function MapSys_SetMapNext(%stage, %file)
{
	if(%stage $= "" || %stage == 0)
	{
		$DefaultMinigame.inMapChange = 1;
		if($Pref::Server::MapSys_VoteType == 0 || $Pref::Server::MapSys_VoteType == 1 || $Pref::Server::MapSys_VoteType == 3)
		{
			$DefaultMinigame.setMusic(nameToID(musicData_JeopardyThemeSong), 0.7);

			cancel($Temp::MapSys_ErrorSch);
			$Temp::MapSys_ErrorSch = schedule(5000 + 1000 * $Pref::Server::MapSys_VoteTimeLimit, 0, "MapSys_ResetMap", "MapChangeError");
			
			announce("\c6[\c3MP\c6] Vote for your map NOW! Say \c3/v map# \c6to vote! Maps in \c7grey \c6are not allowed to be voted!");
			deleteVariables("$Temp::MapSys_Map*");
			%mapCount = $Server::MapSys_MapCount;
			%selectionCount = 3;
			for(%i = 1; %i <= %mapCount; %i++)
			{
				if(%data $= "")
					%data = %i;
				else 
					%data = %data TAB %i;
			}
			%removeCount = %mapCount - %selectionCount;
			for(%i = 0; %i < %removeCount; %i++)
			{
				%data = removeField(%data,getRandom(0,(%mapCount-%i)-1));
			}
			%count = getFieldCount(%data);
			for(%i = 0; %i < %count; %i++)
			{
				%map = $Server::MapSys_MapName[getField(%data,%i)];
				announce(" \c6#\c3" @ %i + 1 @ " \c7- \c3" @ %map);
			}

			// for(%i = 0; %i < clientGroup.getCount(); %i++)
			// {
			// 	%cl = clientGroup.getObject(%i);
			// 	// if(%cl.Shop_Client && %cl.dataInstance($DR::SaveSlot).DR_MapGUI)
			// 	// 	commandToClient(%cl, 'DRShop', "OpenMap");
			// 	// else if(!$Server::MapSys::Temp::IgnoreCPM[%cl.getBLID()])
			// 	// 	%cl.displayCenterPrintMenu(%data, "onMapSelectCPM");
			// }

			$Temp::MapSys_CanVote = 1;
			$Temp::MapSys_MapSelection = %data;
			MapSys_SetMapVote();
		}
		else
		{
			cancel($Temp::MapSys_ErrorSch);

			announce("ERROR: Wrong preset for map changer, or this was never implemented.");
			$DefaultMinigame.round = 0;
			$DefaultMinigame.scheduleReset();
			cancel($Temp::MapSys_TickSch);
		}
	}
	else if(%stage == 2)
	{
		cancel($Temp::MapSys_ErrorSch);

		if(%file $= $Temp::MapSys_CurrentMap)
		{
			if(isFile(%envFile = $Server::MapSys_Path @ fileBase(%file) @ ".txt"))
				loadEnvironmentFromFile(%envFile);
			else if(isFile(%path = $Server::MapSys_Path @ "default.txt"))
			{
				messageAll('',"\c3No environment found for \c1" @ fileBase(%file) @ "\c3, attempting set default environment.");
				loadEnvironmentFromFile(%path);
			}

			MapSys_onMapChanged();
			return;
		}

		if(!isFile(%file) || fileExt(%file) !$= ".bls")
		{
			announce("MapSys_SetMapNext(stage 2) | NO SAVE FOUND - " @ %file);
			return;
		}

		$Temp::MapSys_CurrentMap = %file;
		$Temp::MapSys_CurrentMapName = fileBase(%file);
		$Temp::MapSys_Changing = 1;
		
		//messageAll('',"\c6Clearing bricks for \c3" @ fileBase(%file));
		if(isFile(%colorsetFile = $Server::MapSys_Path @ fileBase(%file) @ ".BLColorset"))
			setColorsetFromFile(%colorsetFile);
		else
			setColorsetFromFile("Add-ons/Gamemode_DeathRace/colorset.txt");

		if(isFile(%envFile = $Server::MapSys_Path @ fileBase(%file) @ ".txt"))
			loadEnvironmentFromFile(%envFile, 1);
		else if(isFile(%path = $Server::MapSys_Path @ "default.txt"))
		{
			messageAll('',"\c3No environment found for \c1" @ fileBase(%file) @ "\c3, attempting set default environment.");
			loadEnvironmentFromFile(%path, 1);
		}
	   
		if(getBrickCount() <= 0)
			MapSys_SetMapNext(3, %file);
		else
		{
			%curCount = -1;
			%curBrickgroup = -1;
			%group = nameToID(MainbrickGroup);
			for(%g = 0; %g < %group.getCount(); %g++)
			{
				%brickGroup = %group.getObject(%g);
				if(%brickgroup.getCount() > %curCount)
				{
					%curCount = %brickgroup.getCount();
					%curBrickgroup = %brickgroup;
				}
			}

			if(isObject(%curBrickgroup))
			{
				for(%g = 0; %g < %group.getCount(); %g++)
				{
					%brickGroup = %group.getObject(%g);
					if(%brickgroup != %curBrickgroup)
						%brickGroup.chainDeleteAll();
				}

				%curBrickgroup.chainDeleteCallback = "schedule(1000, 0, MapSys_SetMapNext, 3, \"" @ %file @ "\");";
				%curBrickgroup.chainDeleteAll();
			}
			else
				MapSys_SetMapNext(3, %file);
		}
	}
	else if(%stage == 3)
	{
		$GameModeDisplayName = "M: " @ $Temp::MapSys_CurrentMapName;
   		webcom_postServer();

   		if(isFile(%creditsFile = $Server::MapSys_Path @ fileBase(%file) @ ".BLCredits"))
   		{
   			%io = new FileObject();
   			%io.openForRead(%creditsFile);
   			while(!%io.isEOF())
   			{
   				%name[%names++] = %io.readLine();
   			}
   			%io.close();
   			%io.delete();

   			%creditsStr = " \c7| \c6Made by: ";
   			for(%i = 1; %i <= %names; %i++)
   			{
   				if(%i == %names)
   					%creditsStr = %creditsStr @ %name[%i];
   				else
   					%creditsStr = %creditsStr @ %name[%i] @ ", ";
   			}
   		}

		messageAll('', "\c6Now loading: \c3" @ $Temp::MapSys_CurrentMapName @ %creditsStr);
		schedule(100, 0, serverDirectSaveFileLoad, %file, 3, "", 2, 1);
	}
}

function serverCmdSetMap(%client, %a1, %a2, %a3, %a4)
{
	if(!%client.isAdmin)
		return;

	for(%i = 1; %i <= 4; %i++)
	{
		if(%a[%i] !$= "")
		{
			if(%map $= "")
				%map = %a[%i];
			else
				%map = %map SPC %a[%i];
		}
	}


	%mappath = findFirstFile($Server::MapSys_Path @ %map @ ".bls");
	if(!isFile(%mappath))
	{
		%client.chatMessage("Invalid map \c3" @ %map);
		return;
	}

	MapSys_SetMapNext(2, %mappath);
	echo(%client.getPlayerName() @ " made an override to set the map to " @ fileBase(%mappath) @ ".");
	announce(%client.getPlayerName() @ " \c6made an override to set the map to " @ fileBase(%mappath) @ ".");
}

function serverCmdNextMap(%client)
{
	if(!%client.isAdmin)
		return;

	if($Server::Temp::MapSys_Change)
		return;

	$Server::Temp::MapSys_Change = 1;
	echo(%client.getPlayerName() @ " made an override to toggle a map change/vote after this round.");
	announce(%client.getPlayerName() @ " \c6made an override to toggle a map change/vote after this round.");
}

function MapSys_SetMapVote(%time)
{
	if(%time $= "")
		%time = $Pref::Server::MapSys_VoteTimeLimit;

	cancel($Temp::MapSys_VoteTickSch);
	if(%time <= 0)
	{
		%votes = 0;
		%mapTieCount = 0;
		announce("\c6[\c3MP\c6] Vote results:");
		%data = $Temp::MapSys_MapSelection;
		%count = getFieldCount(%data);
		for(%i = 1; %i <= %count; %i++)
		{
			%cur = $Temp::MapSys_MapVotes[%i];
			%index = getField(%data,%i - 1);
			announce("  - \c3" @ $Server::MapSys_MapName[%index] @ " (" @ mFloor(%cur) @ (%cur != 1 ? " votes" : " vote") @ ")");
			if(%cur > %votes && %cur != 0)
			{
				%mapTieCount = 0;
				%votes = %cur;
				%mapName = $Server::MapSys_MapName[%index];
				%map = $Server::MapSys_Map[%index];
			}
			else if(%cur == %votes && %votes != 0)
			{
				%tieVotes = %cur;
				if(%mapTieCount == 0)
				{
					%mapTieCount++;
					%mapTie[%mapTieCount] = %map;
					%mapTieName[%mapTieCount] = %mapName;
				}

				%mapTieCount++;
				%mapTieName[%mapTieCount] = $Server::MapSys_MapName[%index];
				%mapTie[%mapTieCount] = $Server::MapSys_Map[%index];
			}
		}

		if(%mapTieCount >= 2)
		{
			announce("\c6[\c3MP\c6] Time is up! Found a tie, randomizing the map between the tied maps!");
			%r = getRandom(1, %mapTieCount);
			%mapName = %mapTieName[%r];
			%map = %mapTie[%r];
		}
		else if(%mapName $= "")
		{
			announce("\c6[\c3MP\c6] Time is up! No votes were counted! Randomizing map!");
			%r = getField(%data,getRandom(0, %count-1));
			%mapName = $Server::MapSys_MapName[%r];
			%map = $Server::MapSys_Map[%r];
		}
		else
			announce("\c6[\c3MP\c6] Time is up! Most voted for " @ %mapName @ "!");
		
		for(%i = 0; %i < clientGroup.getCount(); %i++)
		{
			%cl = clientGroup.getObject(%i);
			if(%cl.Shop_Client)
				commandToClient(%cl, 'DRShop', "CloseMap");
			
			// %cl.displayCenterPrintMenu("stop");
		}

		$DefaultMinigame.setMusic(0, 1);
		schedule(1000, 0, MapSys_SetMapNext, 2, %map);
		$Temp::MapSys_CanVote = 0;
		return;
	}

	for(%i = 1; %i <= $Server::MapSys_MapCount; %i++)
	{
		%cur = $Temp::MapSys_MapVotes[%i];
		%map = $Server::MapSys_MapName[%i];

		if(%cur > 0)
			%voteStr = %voteStr @ "\c7(\c3" @ %cur @ "\c7) \c3" @ %map @ "\n";
	}

	// centerPrintAll("<sPush><font:impact:25>\c6Vote now! - \c3" @ %time @ "<sPop>\n" @ %voteStr, 1.1);

	$Temp::MapSys_VoteTickSch = schedule(1000, 0, MapSys_SetMapVote, %time--); 
}

function serverCmdSaveMap(%client, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9)
{
	if(!%client.isSuperAdmin)
		return;

	if($Server::MCSaver::IsInUse)
	{
		%client.chatMessage("Saver is already in use.");
		return;
	}

	for(%a = 0; %a < 10; %a++)
		%map = %map @ %a[%a] @ " ";

	%map = trim(%map);
	if(%map $= "")
	{
		%map = $Temp::MapSys_CurrentMapName;
	}

	%map = stripChars(stripMLControlChars(%map), "`~!@#^&*=+{}\\|;:\'\",<>/?[].");

	messageAll('MsgAdminForce', '\c3%1 \c6is attempting to save the current map as %2.', %client.getPlayerName(),%map);
	MC_Save1_begin(%map);
}

function serverCmdSaveEnvMap(%client, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9)
{
	if(!%client.isSuperAdmin)
		return;

	for(%a = 0; %a < 10; %a++)
		%map = %map @ %a[%a] @ " ";

	%map = trim(%map);
	if(%map $= "")
	{
		%map = $Temp::MapSys_CurrentMapName;
	}
	%path = $Server::MapSys_Path @ stripChars(stripMLControlChars(%map), "`~!@#^&*=+{}\\|;:\'\",<>/?[].") @ ".txt";

	saveEnvironment(%path);
	announce("\c6(\c3" @ %client.getPlayerName() @ "\c6) \c6Current environment saved into the map changer for %2. (\c3" @ %path @ "\cr)");
}

// function serverCmdVoteMapByName(%client, %mapName)
// {
// 	if(!$Temp::MapSys_CanVote)
// 		return;

// 	if($Temp::MapSys_MapVoteID[%client.getBLID()] !$= "")
// 	{
// 		%client.chatMessage("\c6Sorry, unfortunately you cannot change maps right now.");
// 		return;
// 	}

// 	if($Sim::Time - %client.lastMapVote < 0.2)
// 		return;

// 	%client.lastMapVote = $Sim::Time;
// 	for(%i = 1; %i <= $Server::MapSys_MapCount; %i++)
// 	{
// 		%mapName[%i] = $Server::MapSys_MapName[%i];

// 		if(%mapName[%i] $= %mapName)
// 		{
// 			%mapSel = %i;
// 			%mapName = %mapName[%i];
// 			break;
// 		}
// 	}

// 	if(%mapSel $= "")
// 	{
// 		%client.chatMessage("\c6Invalid map.");
// 		return;
// 	}

// 	if($Temp::MapSys_MapCannotVote[%mapSel] == 1)
// 	{
// 		%client.chatMessage("\c6Sorry, you cannot vote for that map.");
// 		return;
// 	}

// 	if($Temp::MapSys_MapVoteID[%client.getBLID()] !$= "")
// 	{
// 		%client.chatMessage("\c6Sorry, unfortunately you cannot change maps right now.");
// 		return;
// 	}

// 	$Temp::MapSys_MapVoteID[%client.getBLID()] = $Server::MapSys_MapName[%mapSel];
// 	$Temp::MapSys_MapVotes[%mapSel]++;

// 	%client.chatMessage("\c6You voted for \c3" @ $Server::MapSys_MapName[%mapSel]);
// }

function GameConnection::onMapSelectCPM(%client, %num)
{
	serverCmdVote(%client, %num+1);
}

function serverCmdVote(%client, %mapNum) { serverCmdV(%client, %mapNum); }

function serverCmdV(%client, %mapNum)
{
	if(!$Temp::MapSys_CanVote)
		return;

	if($Temp::MapSys_MapVoteID[%client.getBLID()] !$= "")
	{
		%client.chatMessage("\c6Sorry, unfortunately you cannot change maps right now.");
		return;
	}

	// if($Temp::MapSys_MapCannotVote[%mapNum] == 1)
	// {
	// 	%client.chatMessage("\c6Sorry, you cannot vote for that map.");
	// 	return;
	// }

	%mapNum |= 0;
	%mapCount = getFieldCount($Temp::MapSys_MapSelection);
	if(%mapNum > %mapCount || %mapNum <= 0)
	{
		%client.chatMessage("\c6Invalid map number!");
		return;
	}
	%mapName = $Server::MapSys_MapName[getField($Temp::MapSys_MapSelection,%mapNum - 1)];
	$Temp::MapSys_MapVoteID[%client.getBLID()] = %mapName;
	$Temp::MapSys_MapVotes[%mapNum]++;

	%client.chatMessage("\c6You voted for \c3" @ %mapName);
}

function MapChanger_ResetRTV() {
	$Server::MapChanger::RTV_Vote_Active = 0;
	$Server::MapChanger::RTVCount = 0;
	for(%i=0;%i<clientGroup.getCount();%i++) clientGroup.getObject(%i).rtvVote = 0;
}

function MapChanger_RTVFail() {
	echo("The RTV failed (not enough people voted).");
	
	messageAll('', "\c0RTV\c6: Not enough people did \c7/rtv\c6.");
	$CACHE::MapChanger::LastRTVFail = getSimTime();
	MapChanger_ResetRTV();
}

function MapChanger_getRTVPercent() {
	%max     = ClientGroup.getCount() / 2;
	%percent = mClamp(($Server::MapChanger::RTVCount / %max) * 100, 0, 100);
	
	return %percent;
}

function serverCmdRTV(%this)
{
	if(getSimTime() - $CACHE::MapChanger::LastRTVFail < $Pref::MapChanger::RTVFailWait)
	{
		%timeMS = $Pref::MapChanger::RTVFailWait - (getSimTime() - $CACHE::MapChanger::LastRTVFail);
		%timeH  = mFloor(%timeMS / (3600000|0));
		%timeMS = %timeMS % (3600000|0);
		%timeM  = mFloor(%timeMS / 60000);
		%timeMS = %timeMS % 60000;
		%timeS  = mFloor(%timeMS / 1000);
		%time   = getSubStr("00" @ %timeH, strLen("00" @ %timeH) - 2, 2);
		%time   = %time @ ":" @ getSubStr("00" @ %timeM, strLen("00" @ %timeM) - 2, 2);
		%time   = %time @ ":" @ getSubStr("00" @ %timeS, strLen("00" @ %timeS) - 2, 2);
		
		messageClient(%this, '', "\c0RTV\c6: Time remaining before next RTV: " @ %time);
		return;
	}
	
	if($DefaultMinigame.round <= 3) 
	{
		%this.chatMessage("\c0RTV\c6: Please wait a few rounds before using RTV.");
		return;
	}
	
	if($CACHE::MapChanger::DoRTVChange)
	{
		%this.chatMessage("\c0RTV\c6: The map will change after this round. Be patient!");
		return;
	}
	
	if($Server::Temp::MapSys_Change)
	{
		%this.chatMessage("\c0RTV\c6: You currently cannot vote right now. (Changing soon)");
		return;
	}
	
	if($Temp::MapSys_Changing)
	{
		%this.chatMessage("\c0RTV\c6: You currently cannot vote right now. (Currently changing)");
		return;
	}
	
	if(!%this.hasSpawnedOnce)
	{
		%this.chatMessage("\c0RTV\c6: Please wait until you spawn in.");
		return;
	}
	
	if(%this.rtvVote)
	{
		%this.chatMessage("\c0RTV\c6: You already rocked the vote!");
		return;
	}
	
	$Server::MapChanger::RTVCount++;
	
	%percent = MapChanger_getRTVPercent();
	
	if(!isEventPending($MapChanger_RTVFailSchedule))
	{
		echo(%this.getSimpleName() SPC "(BL_ID:" @ %this.getBLID() @ ") started a RTV chain.");
		$Server::MapChanger::RTV_Vote_Active = 1;
		$MapChanger_RTVFailSchedule = schedule(60000, 0, MapChanger_RTVFail);
	} 
	else
	{
		echo(%this.getSimpleName() SPC "(BL_ID:" @ %this.getBLID() @ ") rocked the vote (" @ %percent @ "%)");
	}
	
	%this.rtvVote = 1;
	
	messageAll('MsgAdminForce', "\c0RTV\c6: " @ %this.getPlayerName() @ " wants to rock the vote! Do \c7/rtv\c6 to rock the vote! (" @ %percent @ "%)");
	
	if(%percent >= 100)
	{
		messageAll('', "\c0RTV\c6: RTV succeeded! Map change will happen next round.");
		cancel($MapChanger_RTVFailSchedule);
		
		for(%i=0;%i<clientGroup.getCount();%i++) clientGroup.getObject(%i).rtvVote = 0;
		
		$Server::MapChanger::RTVCount = 0;
		$CACHE::MapChanger::DoRTVChange = 1;
		$Server::Temp::MapSys_Change = 1;
	}
}

////////////////////////////////////////////////////////////////

function MapSys_ResetMap(%errorType)
{
	if(%errorType $= "MapChangeError")
	{
		announce("\c6[\c3MP\c6] An error has been discovered and is currently being fixed by the automated system.");
		$DefaultMinigame.scheduleReset();
		$Temp::MapSys_CanVote = 0;
		$Server::Temp::MapSys_Change = 0;
		$CACHE::MapChanger::DoRTVChange = 0;
	}
}

//////////////////////////////////////////////////////////////

function loadEnvironmentFromFile(%file, %sil)
{
	if(!isFile(%file))
		return -1;
	%res = GameModeGuiServer::ParseGameModeFile(%file, 1);
	if(!%sil)
		announce("\c6  Loading environment: \c3" @ fileBase(%file));

	EnvGuiServer::getIdxFromFilenames();
	EnvGuiServer::SetSimpleMode();

	if(!$EnvGuiServer::SimpleMode)     
	{
		EnvGuiServer::fillAdvancedVarsFromSimple();
		EnvGuiServer::SetAdvancedMode();
	}
}

function setColorsetFromFile(%filename)
{
	if(!isFile(%filename))
		return 0;

	%file = new FileObject();
	%file.openForRead(%filename);
	%i = 0;
	%divCount = 0;
	while(!%file.isEOF())
	{
		%line = %file.readLine();
		if (getSubStr(%line, 0, 4) $= "DIV:")
		{
			%divName = getSubStr(%line, 4, strlen(%line) - 4);
			setSprayCanDivision(%divCount, %i - 1, %divName);
			%divCount++;
		}
		else
		{
			if (%line !$= "")
			{
				if(%i <= 63)
				{
					%r = mAbs(getWord(%line, 0));
					%g = mAbs(getWord(%line, 1));
					%b = mAbs(getWord(%line, 2));
					%a = mAbs(getWord(%line, 3));
					if (mFloor(%r) != %r || mFloor(%g) != %g || mFloor(%b) != %b || mFloor(%a) != %a || (%r <= 1 && %g <= 1 && %b <= 1 && %a <= 1))
					{
						setSprayCanColor(%i, %r SPC %g SPC %b SPC %a);
						%i++;
					}
					else
					{
						setSprayCanColorI(%i, %r SPC %g SPC %b SPC %a);
						%i++;
					}
				}
			}
		}
	}
	%file.close();
	%file.delete();
	$maxSprayColors = %i;

	for(%j = %divCount + 1; %j < 16; %j++)
	{
		setSprayCanDivision(%j, 0, "");
	}

	for(%b = %i + 1; %b < 64; %b++)
	{
		setColorTable(%b, "1.0 0.0 1.0 0.0");
	}

	for(%v = 0; %v < ClientGroup.getCount(); %v++)
	{
		%cl = ClientGroup.getObject(%v);
		%cl.transmitStaticBrickData();
		%cl.transmitDataBlocks(1);
	}
	commandToAll('PlayGui_LoadPaint');

	return 1;
}