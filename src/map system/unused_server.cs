//+=========================================================================================================+\\
//|			Made by..									    |\\
//|        ____   ____  _                __          _                                                      |\\
//|       |_  _| |_  _|(_)              [  |        / |_                                                    |\\
//|         \ \   / /  __   .--.   .--.  | |  ,--. `| |-' .--.   _ .--.                                     |\\
//|          \ \ / /  [  | ( (`\]/ .'`\ \| | `'_\ : | | / .'`\ \[ `/'`\]                                    |\\
//|           \ ' /    | |  `'.'.| \__. || | // | |,| |,| \__. | | |                                        |\\
//|            \_/    [___][\__) )'.__.'[___]\'-;__/\__/ '.__.' [___]                                       |\\
//|                             BL_ID: 20490                                                                |\\
//|				Forum Profile: http://forum.blockland.us/index.php?action=profile;u=40877;  |\\
//|													    |\\
//|				-o-o-o- Modified by Clay Hanson [15144] -o-o-o-				    |\\
//|													    |\\
//+=========================================================================================================+\\

exec("./vote.cs");
exec("./Support_CenterPrintMenu.cs");

if(!strLen($Server::MapChanger::VoteInit)) MapChanger_ResetVotes(); //Minutes
if(!$Server::MapChanger::CheckVals)
{
	if(isFunction(ORBS_registerPref))
	{
		ORBS_registerPref("Debugging (advanced)", "Map Changer", "$Pref::Server::MapChanger::Debug", "bool", "Support_MapChanger", false, 0, 0);
		ORBS_registerPref("Disable voting", "Map Changer", "$Pref::Server::MapChanger::DisableMapChanging", "bool", "Support_MapChanger", false, 0, 0);
		ORBS_registerPref("Map changing time", "Map Changer", "$Pref::Server::MapChanger::Time", "int 1 40320", "Support_MapChanger", 60, 0, 0);
		ORBS_registerPref("Save path", "Map Changer", "$Pref::Server::MapChanger::Path", "string 100 100", "Support_MapChanger", "saves/Map Changer/", 0, 0);
		ORBS_registerPref("RTV Fail Delay (ms)", "Map Changer", "$Pref::MapChanger::RTVFailWait", "int 1 900000", "Support_MapChanger", 60000, 0, 0);
		ORBS_registerPref("Rounds before map change", "Map Changer", "$Pref::Server::MapChanger::MaxWinsBeforeRoundChange", "int 1 90000", "Support_MapChanger", 10, 0, 0);
	}
	else if(isFunction(RTB_registerPref))
	{
		RTB_registerPref("Debugging (advanced)", "Map Changer", "$Pref::Server::MapChanger::Debug", "bool", "Support_MapChanger", false, 0, 0);
		RTB_registerPref("Disable voting", "Map Changer", "$Pref::Server::MapChanger::DisableMapChanging", "bool", "Support_MapChanger", false, 0, 0);
		RTB_registerPref("Map changing time", "Map Changer", "$Pref::Server::MapChanger::Time", "int 1 40320", "Support_MapChanger", 60, 0, 0);
		RTB_registerPref("Save path", "Map Changer", "$Pref::Server::MapChanger::Path", "string 100 100", "Support_MapChanger", "saves/Map Changer/", 0, 0);
		RTB_registerPref("RTV Fail Delay (ms)", "Map Changer", "$Pref::MapChanger::RTVFailWait", "int 1 900000", "Support_MapChanger", 60000, 0, 0);
		RTB_registerPref("Rounds before map change", "Map Changer", "$Pref::Server::MapChanger::MaxWinsBeforeRoundChange", "int 1 900000", "Support_MapChanger", 10, 0, 0);
	}
	else
	{
		if(!strLen($Pref::Server::MapChanger::Time))
			$Pref::Server::MapChanger::Time = 30; //Minutes

		if(!strLen($Pref::Server::MapChanger::Path))
			$Pref::Server::MapChanger::Path = "saves/Map Changer/"; //Minutes

		if(!strLen($Pref::Server::MapChanger::Debug))
			$Pref::Server::MapChanger::Debug = false;

		if(!strLen($Pref::Server::MapChanger::DisableMapChanging))
			$Pref::Server::MapChanger::DisableMapChanging = false;
		
		if(!strLen($Pref::MapChanger::RTVFailWait))
			$Pref::MapChanger::RTVFailWait = 60000; // Milliseconds
		
		if(!strLen($Pref::Server::MapChanger::MaxWinsBeforeRoundChange))
			$Pref::Server::MapChanger::MaxWinsBeforeRoundChange = 10;
	}

	//If this is execed again, we shouldn't mess up stuff.
	$Server::MapChanger::CheckVals = 1;
}

function MapChanger_CreateBrickgroup()
{
	%id = "BrickGroup_" @ getNumKeyID();
	if(!isObject(%id))
	{
		%group = new SimGroup(%id)
		{
			client = -1;
			bl_id = getNumKeyID();
			name = $Pref::Player::NetName;
			isAdmin = 1;
			doNotDelete = 1;
			brickgroup = %id;
		};
		MainBrickgroup.add(%group);
	}
}

//if(isPackage(MapChanger))
//	deactivatepackage(MapChanger);

function MapChanger_DeleteBackgroundMusic()
{
	for(%a = 0; %a < MainBrickgroup.getCount(); %a++)
	{
		%bg = MainBrickgroup.getObject(%a);
	    for(%i = %bg.getCount() - 1; %i > 0; %i--)
	    {
	    	%b = %bg.getObject(%i);
	    	//If there is an audio glitch turn it off. - This helps disable those other range music datablocks that break the music
	    	if(isObject(%b.audioEmitter) && %b.getDatablock().getName() !$= "brickMusicData")
	    		%b.setMusic(-1);
	    }
	}
}

function MapChanger_onMapChanged()
{
	MapChanger_DeleteBackgroundMusic();

	if($Server::MapChanger::Changing)
	{
		messageAll('', "\c3Map has been loaded successfully!");
		$Server::MapChanger::Changing = 0;
		if(isObject(Slayer) && Slayer.Minigames.getCount() > 0)
		{
			for(%i = 0; %i < Slayer.Minigames.getCount(); %i++)
				Slayer.Minigames.getObject(%i).scheduleReset();
		}
		
		if(isObject(%mg = $DefaultMiniGame))
			%mg.scheduleReset(); //don't do it instantly, to give people a little bit of time to ghost
	}
	else
		messageAll('', "\c3Map has been loaded successfully!");

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		if(isObject(%player = %client.player))
		{
			if(isObject(%camera = %client.camera))
				%client.setControlObject(%player);
		}
		else
		{
			if(isObject(%camera = %client.camera))
			{
				%camera.setFlyMode();
				%client.setControlObject(%camera);
			}
		}
	}
}

package Server_MapChanger
{
	function ServerLoadSaveFile_End()
   	{
      	Parent::ServerLoadSaveFile_End();
      	MapChanger_onMapChanged();
   	}

   	function MinigameSO::Reset(%this, %client)
   	{
		if($Server::MapChanger::Changing)
			$Server::MapChanger::Changing = 0;
		else
		{
			cancel($MapChanger::changeSch);
			$MapChanger::changeSch = schedule(500, 0, "MapChanger_Switch");
			Parent::Reset(%this, %client);
		}
   	}

   	function Slayer_MinigameSO::Reset(%this, %client)
   	{
		if($Server::MapChanger::Changing)
			$Server::MapChanger::Changing = 0;
		else
		{
			cancel($MapChanger::changeSch);
			$MapChanger::changeSch = schedule(500, 0, "MapChanger_Switch");
			Parent::Reset(%this, %client);
		}
   	}
};
activatepackage("Server_MapChanger");

function MapChanger_Switch()
{
	if(!isObject("BrickGroup_" @ getNumKeyID()))
	{
		announce("No host brickgroup found, creating one anyway.");
		MapChanger_CreateBrickgroup();
	}

	if($Server::MapChanger::Changing)
		return;

	if($Server::MapChanger::DoChange)
   	{
   		deleteVariables("$Server::MapChanger::Votes*");
   		deleteVariables("$Server::MapChanger::LastVote*");
   		MapChanger_ResetVotes();
   		MapChanger_LoadTrack($Server::MapChanger::ChangeTo);

		for(%i = 0; %i < ClientGroup.getCount(); %i++)
		{
			%client = ClientGroup.getObject(%i);
			if(isObject(%camera = %client.camera))
			{
				%camera.setFlyMode();
				if(isObject(%player = %client.player))
					%camera.setMode("Corpse", %player);
				else
					%camera.setMode("Orbit");
				%client.setControlObject(%camera);
			}
		}
	}
}

function MapChanger_Debug(%type,%msg)
{
	if(%type == 0) //Critical
		%type = 'MsgAdminForce';
	else if(%type == 1) //Error
		%type = 'MsgClearBricks';
	else //Yea nothing is here to hear, not important, but useful
		%type = '';
	if($Pref::Server::MapChanger::Debug)
	{
		messageAll(%type,"\c3[\c7Debugger\c3]\c0 " @ %msg);
		echo("[MapChange Debugger] > " @ %msg);
	}
}

function MapChanger_getDisplayTime(%time, %ignoreSeconds, %timestring)
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

if(isFile("config/server/MapChanger.cs"))
	exec("config/server/MapChanger.cs");

function MapChanger_findMap(%map)
{
	if(trim(%map) $= "")
		return "ERROR_BLANKMAP";

	%count = getFileCount($Pref::Server::MapChanger::Path @ "*.bls");
	for(%i=0;%i<%count;%i++)
	{
		%file = findNextFile($Pref::Server::MapChanger::Path @ "*.bls");
		if(%map $= fileBase(%file))
			return fileBase(%file);
	}
	return "ERROR_NONEXISTANTMAP";
}

function MapChanger_LoadTrack(%fileName)
{
	if(!isFile(%fileName) || fileExt(%fileName) !$= ".bls")
	{
		announce("MapChanger_LoadTrack() - NO SAVE FOUND - " @ %fileName);
		MapChanger_Debug(1,"MapChanger_LoadTrack() \c0- \c3Error: \crFile \"\c4" @ %fileName @ "\cr\" not found. Map will not change.");
		return;
	}

	$Server::MapChanger::DoChange = 0;
   	$Server::MapChanger::ChangeTo = "";

	$Server::MapChanger::CurrentMap = "";
	$Server::MapChanger::Changing = 1;
	
	if(isEventPending($MapChanger_RTVFailSchedule)) { cancel($MapChanger_RTVFailSchedule); MapChanger_ResetRTV(); }
	
	MapChanger_LoadTrack_Phase1(%fileName);
}

function MapChanger_LoadTrack_Phase1(%filename)
{
	if(!$Server::MapChanger::Changing)
		return;

   	MapChanger_Debug('',"MapChanger_LoadTrack_Phase1() \c2+ \c3Found save file: \c4" @ fileBase(%filename) @ "\c3. Clearing bricks..");
   	messageAll('',"\c3Clearing bricks for \c1" @ fileBase(%filename));
   	//put everyone in observer mode
   	%mg = $DefaultMiniGame;
   	if(!isObject(%mg) && Slayer.Minigames.getCount() > 0 && isObject(%slyrMini = Slayer.Minigames.getObject(0)))
   		%mg = %slyrMini; //Forcing

   	if(isObject(Slayer) && Slayer.Minigames.getCount() > 0)
	{
		for(%i = 0; %i < Slayer.Minigames.getCount(); %i++)
		{
			%mg = Slayer.Minigames.getObject(%i);
	      	for(%a = 0; %a < %mg.numMembers; %a++)
	      		if(isObject(%client = %mg.member[%a]))
	      		{
	      			if(isObject(%pl = %client.player))
	      				%pl.delete();

	      			if(isObject(%camera = %client.camera))
			      	{
				      	%camera.setFlyMode();
				      	%camera.setMode("Orbit");
				      	%client.setControlObject(%camera);
			      	}
	      		}
	    }
	}
	
	if(isObject(%mg = $DefaultMiniGame))
	{
		for(%a = 0; %a < %mg.numMembers; %a++)
			if(isObject(%client = %mg.member[%a]))
	      	{
	      		if(isObject(%pl = %client.player))
	      			%pl.delete();

      			if(isObject(%camera = %client.camera))
		      	{
			      	%camera.setFlyMode();
			      	%camera.setMode("Orbit");
			      	%client.setControlObject(%camera);
		      	}
      		}
	}

   	//Thanks to whoever made GameMode_SpeedKart
   	if(isFile(%envFile = $Pref::Server::MapChanger::Path @ fileBase(%filename) @ ".txt"))
		loadEnvironmentFromFile(%envFile);
	else if(isFile(%path = $Pref::Server:MapChanger::Path @ "default.txt"))
	{
		messageAll('',"\c3No environment found for \c1" @ fileBase(%filename) @ "\c3, attempting set default environment.");
		loadEnvironmentFromFile(%path);
	}
   
	//clear all bricks 
	// note: this function is deferred, so we'll have to set a callback to be triggered when it's done
	if(getBrickCount() <= 0)
		MapChanger_LoadTrack_Phase2(%filename);
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

			%curBrickgroup.chainDeleteCallback = "MapChanger_LoadTrack_Phase2(\"" @ %filename @ "\");";
			%curBrickgroup.chainDeleteAll();
		}
		else
			MapChanger_LoadTrack_Phase2(%filename);
	}
}

function MapChanger_LoadTrack_Phase2(%filename)
{
	cancel($MapChangerSch);
	$MapChangerSch = schedule(5000, 0, MapChanger_LoadTrack_Phase3, %filename);
}

function MapChanger_LoadTrack_Phase3(%filename)
{
   	MapChanger_Debug('',"MapChanger_LoadTrack_Phase3() \c2+ \c3Loading save file: \c4" @ fileBase(%filename));
   	$Server::MapChanger::CurrentMap = fileBase(%filename);
   	%displayName = $Server::MapChanger::CurrentMap;
   	%displayName = strReplace(%displayName, ".bls", "");
   	%displayName = strReplace(%displayName, "_", " ");
   	
   	%loadMsg = "\c3Now loading \c3" @ %displayName;

   	messageAll('', %loadMsg);

   	$GameModeDisplayName = %displayName;
   	webcom_postServer();
   	//load save file
   	MapChanger_CreateBrickgroup();
   	schedule(10, 0, serverDirectSaveFileLoad, %fileName, 3, "", ($Pref::Server::MapChanger::UseHost ? 0 : 2), 1);
}

function serverCmdMap(%this, %command, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
{
	if(!%this.isAdmin)
		return;

	switch$(%command)
	{
		case "override":
			if(!%this.isAdmin)
				return;

			for(%a = 0; %a < 10; %a++)
				%map = %map @ %a[%a] @ " ";

			%map = $Pref::Server::MapChanger::Path @ stripChars(stripMLControlChars(trim(%map)), "`~!@#^&*=+{}\\|;:\'\",<>/?[].") @ ".bls";

			if(!isFile(%map))
			{
				%this.chatMessage(%map @ " does not exist!");
				return;
			}

			$Server::MapChanger::OverrideMap = %map;
			messageAll('MsgAdminForce', '\c3%1 \c6has put an override to change the next map to \c3%2 \c6regardless of votes.', %this.getPlayerName(), fileBase(%map));			

		case "saveEdit":
			if(!%this.isSuperAdmin)
				return;

			if($Server::MCSaver::IsInUse)
			{
				%this.chatMessage("Saver is already in use.");
				return;
			}

			for(%a = 0; %a < 10; %a++)
				%map = %map @ %a[%a] @ " ";
			%map = stripChars(stripMLControlChars(trim(%map)), "`~!@#^&*=+{}\\|;:\'\",<>/?[].");

			messageAll('MsgAdminForce', '\c3%1 \c6is attempting to save the current map.', %this.getPlayerName());
			MC_Save1_begin(%map);

		case "SaveEnvMap":
			if(!%this.isSuperAdmin)
				return;

			for(%a = 0; %a < 10; %a++)
				%map = %map @ %a[%a] @ " ";
			%map = stripMLControlChars(trim(%map));
			%path = $Pref::Server::MapChanger::Path @ stripChars(%map, "`~!@#^&*=+{}\\|;:\'\",<>/?[].") @ ".txt";

			saveEnvironment(%path);
			announce("\c6(\c3" @ %this.getPlayerName() @ "\c6) \c6Current environment saved into the map changer. (\c3" @ %path @ "\cr)");

		case "set" or "change":
			for(%a = 0; %a < 10; %a++)
				%map = %map @ %a[%a] @ " ";
			%map = stripMLControlChars(trim(%map));

			%map = MapChanger_findMap(%map);

			if(%map $= "ERROR_NONEXISTANTMAP" || %map $= "ERROR_BLANKMAP")
			{
				%path = $Pref::Server::MapChanger::Path @ "*.bls";
				%count = getFileCount(%path);

				if(%count <= 0)
				{
					%this.chatMessage("Uh oh, looks like they're aren't any maps found! :(");
					return "ERROR_NOMAPS";
				}

				if(%map $= "ERROR_NONEXISTANTMAP")
					%this.chatMessage("\c6Sorry, you have entered the wrong map. \c3Here are the maps found:");
				else
					%this.chatMessage("\c6Current maps found:");

				for(%file = findFirstFile(%path); %file !$= ""; %file = findNextFile(%path))
				{
					if(fileBase(%file) !$= "")
						%this.chatMessage("\c6 + \c3" @ fileBase(%file));
				}

				%this.chatMessage("\c6/setMap \c3Map \c6- Changes the map instantly.");
			}
			else
			{
				announce(%this.getPlayerName() @ " \c6has changed the map to \c3" @ %map @ "\c6.");
				echo("[MapChanger] " @ %this.getPlayerName() @ " has changed the map to " @ %map @ ".");

				$Server::MapChanger::DoChange = 1;
				$Server::MapChanger::ChangeTo = $Pref::Server::MapChanger::Path @ %map @ ".bls";

		  		MapChanger_Switch();
			}

		case "size":
			if(!%this.isSuperAdmin)
				return;

			%thing = trim(%a1 SPC %a2);
			switch$(%thing)
			{
				case "Very small":
					%col = "\c4";
					%thing = "Very small";

				case "Small":
					%col = "\c2";
					%thing = "Small";

				case "Medium":
					%col = "\c3";
					%thing = "Medium";

				case "Large":
					%col = "\c0";
					%thing = "Large";

				case "Very large":
					%col = "\c0";
					%thing = "Very large";

				case "Planet":
					%col = "\c6";
					%thing = "Planet";

				case "Unknown":
					%col = "\c7";
					%thing = "Unknown";

				default:
					%this.chatMessage("\c6- \c3Map sizes \c6-");
					%this.chatMessage("  \c7Unknown \c6- If you aren't sure, you can just set it to this one.");
					%this.chatMessage("  \c4Very small \c6- Very small map.. WHY DID YOU DO THIS");
					%this.chatMessage("  \c2Small \c6- Small map, 6+ players");
					%this.chatMessage("  \c3Medium \c6- Medium map, probably average for 12 players");
					%this.chatMessage("  \c0Large \c6- Large map, 20+");
					%this.chatMessage("  \c0Very large \c6- Very large map, 28+ players");
					%this.chatMessage("  \c6Planet \c6- It's 99 players mang");
					return;
			}

			%mapStr = getSafeVariableName(stripMLControlChars($Server::MapChanger::CurrentMap));
			$MapChanger::MapSize[%mapStr] = %thing;
			announce(%this.getPlayerName() @ " \c6has set \c3" @ strReplace($Server::MapChanger::CurrentMap @ "'s", "s's", "s'") @ " \c6map size to " @ %col @ %thing @ "\c6.");

			export("$MapChanger::*", "config/server/MapChanger.cs");

		case "difficult" or "level" or "diff":
			if(!%this.isSuperAdmin)
				return;

			%difficult = trim(%a1 SPC %a2);
			switch$(%difficult)
			{
				case "Unknown":
					%col = "\c7";
					%difficult = "Unknown";

				case "Very easy":
					%col = "\c4";
					%difficult = "Very easy";

				case "Easy":
					%col = "\c2";
					%difficult = "Easy";

				case "Medium":
					%col = "\c3";
					%difficult = "Medium";

				case "Hard":
					%col = "\c0";
					%difficult = "Hard";

				case "Very hard":
					%col = "\c0";
					%difficult = "Very hard";

				case "Nightmare":
					%col = "\c6";
					%difficult = "Nightmare";

				default:
					%this.chatMessage("\c6- \c3Map sizes \c6-");
					%this.chatMessage("  \c7Unknown \c6- If you aren't sure, you can just set it to this one.");
					%this.chatMessage("  \c4Very easy \c6- Very small map.. WHY DID YOU DO THIS");
					%this.chatMessage("  \c2Easy \c6- Small map, 6+ players");
					%this.chatMessage("  \c3Medium \c6- Medium map, probably average for 12 players");
					%this.chatMessage("  \c0Hard \c6- Large map, 20+");
					%this.chatMessage("  \c0Very hard \c6- Very large map, 28+ players");
					%this.chatMessage("  \c6Nightmare \c6- It's 99 players mang");
					return;
			}

			%mapStr = getSafeVariableName(stripMLControlChars($Server::MapChanger::CurrentMap));

			$MapChanger::MapDifficult[%mapStr] = %difficult;
			announce(%this.getPlayerName() @ " \c6has set \c3" @ strReplace($Server::MapChanger::CurrentMap @ "'s", "s's", "s'") @ " \c6difficulty on " @ %col @ %difficult @ "\c6.");

			export("$MapChanger::*", "config/server/MapChanger.cs");

		case "max" or "players":
			if(!%this.isSuperAdmin)
				return;

			%players = mClampF(mFloor(%a1), 0, 99);

			%mapStr = getSafeVariableName(stripMLControlChars($Server::MapChanger::CurrentMap));

			$MapChanger::MapRequirePlayers[%mapStr] = %players;
			if(%players >= 1)
				announce(%this.getPlayerName() @ " \c6has set \c3" @ strReplace($Server::MapChanger::CurrentMap @ "'s", "s's", "s'") @ " \c6max players at \c3" @ %players @ "\c6.");
			else
				announce(%this.getPlayerName() @ " \c6has set \c3" @ strReplace($Server::MapChanger::CurrentMap @ "'s", "s's", "s'") @ " \c6map to not have a max player limit.");

			export("$MapChanger::*", "config/server/MapChanger.cs");

		default:
			%this.chatMessage("\c6- \c3/Map commands \c6-");
			%this.chatMessage("\c3 \c3set/change \c4map name here \c6- Changes the map to whatever, if it exists.");
			if(%this.isSuperAdmin)
			{
				%this.chatMessage("\c3 \c3SaveEnvMap \c4map name here \c6- Saves the current environment.");
				%this.chatMessage("\c3 \c3difficult/level/diff \c6- Sets the map difficulty, if needed one.");
				%this.chatMessage("\c3 \c3size \c6- Sets the map size.");
				%this.chatMessage("\c3 \c3max/players \c4amount \c6- Requires # of players to play on this map.");
				%this.chatMessage("\c3 \c3saveEdit \c4map name \c6- Saves/overwrites a map in the map changer. Be careful with this.");
			}
	}
}

if(isEventPending($MapChanger_TickSch))
{
//	messageAll('',"Map changer has been reloaded.");
	if($Pref::Server::MapChanger::Time >= 40320)
	{
		$Time = $Pref::Server::MapChanger::Time / 40320;
		$MTime = "week";
	}
	else if($Pref::Server::MapChanger::Time >= 10080)
	{
		$Time = $Pref::Server::MapChanger::Time / 10080;
		$MTime = "week";
	}
	else if($Pref::Server::MapChanger::Time >= 1440)
	{
		$Time = $Pref::Server::MapChanger::Time / 1440;
		$MTime = "day";
	}
	else if($Pref::Server::MapChanger::Time >= 60)
	{
		$Time = $Pref::Server::MapChanger::Time / 60;
		$MTime = "hour";
	}
	else
	{
		$Time = $Pref::Server::MapChanger::Time;
		$MTime = "minute";
	}
	MapChanger_Debug(0,"\c2server.cs \c3- Pref detected \c4save path\c3:\c4 " @ $Pref::Server::MapChanger::Path);
	MapChanger_Debug(0,"\c2server.cs \c3- Pref detected \c4time switch\c3:\c4 " @ $Time SPC $MTime @ "(s)");
}

function strCapFirst(%str)
{
	%str = trim(%str);
	for(%i=0; %i < getWordcount(%str); %i++)
	{
		%word = getWord(%str, %i);
		%first = getSubStr(%word, 0, 1);
		%first = strUpr(%first);
		%rest = getSubStr(%word, 1, strLen(%word));
		%finish = %finish SPC %first @ %rest;
	}

	%finish = trim(%finish);
	return %finish;
}

schedule(0, 0, MapChanger_Tick);

function loadEnvironmentFromFile(%file)
{
	if(!isFile(%file))
		return -1;
	%res = GameModeGuiServer::ParseGameModeFile(%file, 1);
	
	if(fileBase(%file) !$= $Server::MapChanger::CurrentMap && fileBase(%file) !$= "Night")
		announce("Loading environment: \c4" @ fileBase(%file));

	EnvGuiServer::getIdxFromFilenames();
	EnvGuiServer::SetSimpleMode();

	if(!$EnvGuiServer::SimpleMode)     
	{
		EnvGuiServer::fillAdvancedVarsFromSimple();
		EnvGuiServer::SetAdvancedMode();
	}
}

//Ripped from my own MapChangerSaver
function MC_Save1_begin(%name)
{
	%name = trim(stripMLControlChars(%name));
	if(%name $= "")
		return;

	if($Server::MCSaver::IsInUse)
		return;

	cancel($MC_Save_schedule);
	MC_Save_SetState("(AS1) Save init");
	echo("[MapChangerSaver] - Attempting to save bricks...");
	echo("  - Ownership");
	echo("  - Events");

	deleteVariables("$Server::MCSaverGps::group*");
	if(!isObject($Server::MCSaver::SaveObject))
		$Server::MCSaver::SaveObject = new GuiTextListCtrl("Server_MapChangerSaverList");
	else
		$Server::MCSaver::SaveObject.clear();

	if(getBrickCount() <= 0)
	{
		MC_Save_SetState("(AS1) No bricks");
		announce("\c6There are no bricks to save.");
		echo("[MapChangerSaver] - No bricks to save.");
		$MC_Save_schedule = schedule($Pref::Server::AS::Interval * 60 * 1000, 0, "MC_Save1_begin");
		return;
	}

	$Server::MCSaver::IsInUse = 1;

	announce("\c6(\c3MapChanger\c6) \c6Saving bricks... ");

	$time_beg = $Sim::Time;

	$Server::MCSaver::SaveName = %name;
	$Server::MCSaverGps::group_count = 0;
	$Server::MCSaverGps::cur_group = 0;
	$Server::MCSaverGps::brick_count = 0;
	$Server::MCSaverGps::event_count = 0;

	MC_Save_SetState("(AS1) Save - Collecting");
	for(%i = 0; %i < mainBrickGroup.getCount(); %i++)
	{
		%g = mainBrickGroup.getObject(%i);
		%b = %g.getCount();
		if(%b > 0)
		{
			%g.MC_Save_stop = %b;
			$Server::MCSaverGps::group[$Server::MCSaverGps::group_count] = %g;
			$Server::MCSaverGps::group_count++;
		}
	}

	MC_Save_SetState("(AS1) Save - Finished collecting");

	if(isObject($Server::MCSaverGps::group[0]))
	{
		MC_Save_SetState("(AS1) Begin to save " @ $Server::MCSaverGps::group_count @ " group" @ ($Server::MCSaverGps::group_count != 1 ? "s" : ""));
		MC_Save1_nextGroup();
	}
	else
	{
		announce("\c6There are no bricks to save.");
		echo("[MapChangerSaver] - No bricks to save.");
	}
}

function MC_Save1_nextGroup()
{
	if(!$Server::MCSaver::IsInUse)
		return;

	if($Server::MCSaverGps::cur_group == $Server::MCSaverGps::group_count)
	{
		%count = $Server::MCSaver::SaveObject.rowCount();
		MC_Save_SetState("(AS1) Saved " @ %count @ " brick" @ (%count != 1 ? "s" : "") @ ", sorting");
		$Server::MCSaver::SaveObject.sortNumerical(0, 1);
		return MC_Save2_begin();
	}

	%g = $Server::MCSaverGps::group[$Server::MCSaverGps::cur_group];
	$Server::MCSaverGps::cur_group++;

	MC_Save1_nextBrick(%g, 0);
}

function MC_Save1_nextBrick(%g, %c)
{
	if(!$Server::MCSaver::IsInUse)
		return;

	if(!isObject(%g))
		return;

	if(%c >= %g.getCount())
		return MC_Save1_nextGroup();

	%brick = nameToID(%g.getObject(%c));

	if(!isObject($Server::MCSaver::SaveObject))
		return;

	if(%brick.isPlanted)
	{
		if($Server::MCSaver::SaveObject.getRowNumByID(%brick) == -1)
			$Server::MCSaver::SaveObject.addRow(%brick, %brick.getDistanceFromGround());

		$Server::MCSaverGps::brick_count += 1;
	}
	else if(isObject(%brick))
	{
		%del = 1;
		for(%i = 0; %i < ClientGroup.getCount(); %i++)
		{
			if(ClientGroup.getObject(%i).player.tempBrick == %brick)
				%del = 0;
		}

		if(%del) //This helps delete unwanted temp bricks that don't belong to anyone
			%brick.schedule(0, "delete");
	}

	//if($Pref::Server::AS::SlowDownAfter > 0 && %c > $Pref::Server::AS::SlowDownAfter)
	//	return schedule(1, 0, "MC_Save1_nextBrick", %g, %c + 1);
		
	return schedule(0, 0, "MC_Save1_nextBrick", %g, %c + 1);
}

function MC_Save2_begin()
{
	$Server::MCSaver::LastPrint = "";
	MC_Save_SetState("(AS2) Write init");

	%dir = $Pref::Server::MapChanger::Path;

	if(isObject($Server::MCSaver::TempB))
	{
		$Server::MCSaver::TempB.close();
		$Server::MCSaver::TempB.delete();
	}

	if(!$Server::MCSaver::IsInUse)
		return;

	$Server::MCSaver::TempB = new FileObject();

	$Server::MCSaver::TempB.path = %dir @ "SAVETEMP.bls";

	$Server::MCSaver::TempB.openForWrite($Server::MCSaver::TempB.path);
	$Server::MCSaver::TempB.writeLine("This is a Blockland save file.  You probably shouldn't modify it cause you'll mess it up.");
	$Server::MCSaver::TempB.writeLine("1");
	$Server::MCSaver::TempB.writeLine(%desc);

	for(%i = 0; %i < 64; %i++)
		$Server::MCSaver::TempB.writeLine(getColorIDTable(%i));

	$Server::MCSaver::TempB.writeLine("Linecount " @ $Server::MCSaver::SaveObject.rowCount());

	$Server::MCSaverGps::brick_count = 0;
	MC_Save2_nextLine($Server::MCSaver::TempB, 0);
}

function MC_Save2_nextLine(%f, %c)
{
	if(!$Server::MCSaver::IsInUse)
	{
		if(isObject(%f))
		{
			%f.close();
			%f.delete();
		}

		return;
	}

	%events = 1;
	%ownership = 1;
	%count = $Server::MCSaver::SaveObject.rowCount();

	if(%c < %count)
	{
		%brick = $Server::MCSaver::SaveObject.getRowID(%c);
		if(isObject(%brick))
		{
			$Server::MCSaverGps::brick_count++;

			//next
			if(%brick.getDataBlock().hasPrint)
			{
				%texture = getPrintTexture(%brick.getPrintId());
				%path = filePath(%texture);
				%underscorePos = strPos(%path, "_");
				%name = getSubStr(%path, %underscorePos + 1, strPos(%path, "_", 14) - 14) @ "/" @ fileBase(%texture);
				if($printNameTable[%name] !$= "")
					%print = %name;
			}

			%f.writeLine(%brick.getDataBlock().uiName @ "\" " @ %brick.getPosition() SPC %brick.getAngleID() SPC %brick.isBasePlate() SPC %brick.getColorID() 
				SPC %print SPC %brick.getColorFXID() SPC %brick.getShapeFXID() SPC %brick.isRayCasting() SPC %brick.isColliding() SPC %brick.isRendering());

			if(%ownership && !$Server::LAN)
				%f.writeLine("+-OWNER " @ getBrickGroupFromObject(%brick).bl_id);

			if(%events)
			{
				if(%brick.getName() !$= "")
					%f.writeLine("+-NTOBJECTNAME " @ %brick.getName());

				for(%b = 0; %b < %brick.numEvents; %b++)
				{
					$Server::MCSaverGps::event_count++;
					//Get rid of this garbage code
					//%targetClass = %brick.eventTargetIdx[%b] >= 0 ? getWord(getField($InputEvent_TargetListfxDTSBrick_[%brick.eventInputIdx[%b]], %brick.eventTargetIdx[%b]), 1) : "fxDtsBrick";
					//%paramList = $OutputEvent_parameterList[%targetClass, %brick.eventOutputIdx[%b]];
					//for(%j = 1; %j < 4; %j++)
					//{
					//	%curParam = %brick.eventOutputParameter[%b, %j];
					//	if(getWordCount(%curParam) == 1 && isObject(%curParam))
					//		if((%curParamName = %curParam.getName()) !$= "")
					//			%curParam = %curParamName;

					//	%params = %params TAB %curParam;
					//}

					%params = getFields(%brick.serializeEventToString(%b), 7, 10);
					%f.writeLine("+-EVENT" TAB %b TAB %brick.eventEnabled[%b] TAB %brick.eventInput[%b] TAB %brick.eventDelay[%b] TAB %brick.eventTarget[%b] 
						TAB %brick.eventNT[%b] TAB %brick.eventOutput[%b] TAB %params);
				}
			}
			
			if(isObject(%emitter = %brick.emitter) && isObject(%emitterData = %emitter.getEmitterDatablock()) && (%emitterName = %emitterData.uiName) !$= "")
				%f.writeLine("+-EMITTER " @ %emitterName @ "\" " @ %brick.emitterDirection);

			if(isObject(%light = %brick.getLightID()) && isObject(%lightData = %light.getDataBlock()) && (%lightName = %lightData.uiName) !$= "")
				%f.writeLine("+-LIGHT " @ %lightName @ "\" "); // Not sure if something else comes after the name

			if(isObject(%item = %brick.item) && isObject(%itemData = %item.getDataBlock()) && (%itemName = %itemData.uiName) !$= "")
				%f.writeLine("+-ITEM " @ %itemName @ "\" " @ %brick.itemPosition SPC %brick.itemDirection SPC %brick.itemRespawnTime);

			if(isObject(%audioEmitter = %brick.audioEmitter) && isObject(%audioData = %audioEmitter.getProfileID()) && (%audioName = %audioData.uiName) !$= "")
				%f.writeLine("+-AUDIOEMITTER " @ %audioName @ "\" "); // Not sure if something else comes after the name

			if(isObject(%spawnMarker = %brick.vehicleSpawnMarker) && (%spawnMarkerName = %spawnMarker.uiName) !$= "")
				%f.writeLine("+-VEHICLE " @ %spawnMarkerName @ "\" " @ %brick.reColorVehicle);
		}

		//if($Pref::Server::AS::SlowDownAfter > 0 && %c > $Pref::Server::AS::SlowDownAfter)
		//	return schedule(1, 0, "MC_Save2_nextLine", %f, %c + 1);
		
		return schedule(0, 0, "MC_Save2_nextLine", %f, %c + 1);
	}
	else
	{
		MC_Save_SetState("(AS2) Finalize save writing");
		schedule(0, 0, MC_Save2_end, %f);
	}
}

function MC_Save2_end(%f)
{
	%f.close();

	%dir = $Server::MCSaver::SaveName;
	if(%dir $= "")
		%dir = "save";

	%direc = $Pref::Server::MapChanger::Path @ %dir @ ".bls";

	if(isFile(%direc)) //Overwrite it, just delete it for sure
	{
		fileDelete(%direc);
		%overwrite = " \c6(\c0OVERWRITING\c6)";
	}

	if(isFile(%f.path))
		fileCopy(%f.path, %direc);

	if(isFile(%f.path))
		fileDelete(%f.path);

	%f.delete();

	$Server::MCSaver::IsInUse = 0;

	%diff = ($Sim::Time - $time_beg);
	%time = %diff;
	%msg = "in \c3"@ mFloatLength(%time, 2) @" \c6second" @ (%time == 1 ? "" : "s");
	if(%time < 1)
		%msg = "\c3instantly";

	%bGroups = $Server::MCSaverGps::cur_group;

	$Pref::Server::MapChanger["LastMCSaver"] = %direc;
	if(isFile(%f.path))
		fileCopy(%f.path, "base/server/temp/temp.bls");

	%saveMsg = " Saved as \c3" @ $Pref::Server::MapChanger["LastMCSaver"] @ "\c6.";

	announce("\c6Saved \c3" @ $Server::MCSaverGps::brick_count @" \c6brick" @ ($Server::MCSaverGps::brick_count == 1 ? "" : "s")
		@ " " @ %msg @ %overwrite @ "\c6." @ %saveMsg);

	announce("  \c6- \c3" @ $Server::MCSaverGps::event_count @ " event" @ ($Server::MCSaverGps::event_count == 1 ? "" : "s") @
		" \c6and \c3" @ %bGroups @ " group" @ (%bGroups == 1 ? "" : "s") @ " \c6have been saved.");
	
	echo("[MapChangerSaver] - Saved " @ $Server::MCSaverGps::brick_count @ " bricks " @ %msg @ ". Saved as " @ $Pref::Server::AS["LastMCSaver"] @ %overwrite @ ".");

	if($Pref::Server::AS::Report)
	{
		if(!$Server::MCSaver::RelatedBrickCount)
			echo("   - Saved " @ $Server::MCSaverGps::event_count @ " event" @ ($Server::MCSaverGps::event_count == 1 ? "" : "s") @
				" and " @ %bGroups @ " group" @ (%bGroups == 1 ? "" : "s") @ ".");
	}

	MC_Save_SetState("(AS2) - Write complete");
	MC_Save3_ClearList(1); //Clear list will call the MC_Save1_begin
	setModPaths(getModPaths());
}

function MC_Save3_ClearList(%a)
{
	if(%a)
		MC_Save_SetState("(AS3) - Clearing list");

	if(isObject(%brickList = $Server::MCSaver::SaveObject))
	{
		$Server::MCSaver::DoNotSave = true;
		if(%brickList.rowCount() == 0)
			%brickList.delete();
		else
			%brickList.removeRow(0);
	}
	else
	{
		MC_Save_SetState("(AS3) - Cleared list");
		$Server::MCSaver::DoNotSave = false;
		return;
	}

	$Server::MCSaver::ClearSch = schedule(0, 0, "MC_Save3_ClearList");
}

function MC_Save_SetState(%state)
{
	if(%state $= "")
		return;

	if($Server::MCSaver::State $= %state)
		return;

	$Server::MCSaver::State = %state;
	if($Server::MCSaver::Debug)
	{
		announce("Server_AS_SetState() - Mode set to \c2" @ %state);
		echo("Server_AS_SetState() - Mode set to \c2" @ %state);
	}
}