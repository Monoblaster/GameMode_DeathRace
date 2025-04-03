//Mode type functions to know:
// ::onRoundStart(%mode, %mini)          - Called when the minigame starts a new round
// ::onRoundEnd(%mode, %mini)            - Called when the minigame end a round
// ::onInit(%mode, %mini)                - Called when the minigame starts a new game type - good for things like setting up teams
// ::onReset(%mode, %mini)               - Called when the minigame pre-resets the round - not called during ::onInit
// ::onSpawn(%mode, %mini, %client)      - Called when a client spawns

function MinigameSO::setMode(%mini, %modeName, %setRound)
{
	if(!isObject(%mini.games))
		return;

	if(%modeName $= "")
	{
		%lastMode = %mini.mode;
		for(%i = 0; %i < %mini.games.getCount(); %i++)
		{
			%nMode = %mini.games.getObject(%i);
			if(%nMode != %lastMode)
			{
				//must be eligible..
				if(%nMode.minPlayersPerTeam > 0 && %mini.numMembers < %nMode.minPlayersPerTeam * 2)
					continue;

				if(%modeList $= "")
					%modeList = %nMode;
				else
					%modeList = %modeList SPC %nMode;
			}
		}

		if(getWordCount(%modeList) == 0)
			%mode = %lastMode;
		else
			%mode = getWord(%modeList, getRandom(0, getWordCount(%modeList)-1));

		echo("[" @ %mini.displayName @ "] Mode randomly set to " @ %mode.uiName);
	}
	else if(!isObject(%mode = %mini.games.find(%modeName)))
	{
		warn("WARNING: [" @ %mini.displayName @ "] - Cannot find mode " @ %modeName @ "!");
		backtrace();
		return;
	}
	else
	{
		echo("[" @ %mini.displayName @ "] Mode set to " @ %mode.uiName);
	}

	if(isObject(%mini.Teams))
	{
		while(%mini.Teams.getCount() > 0)
			%mini.Teams.getObject(0).delete();

		%mini.Teams.delete();
		%mini.Teams = 0;
	}

	%mini.mode = %mode;
	%mini.modeName = %mode.uiName;
	if(%mini.lockedMaxRound <= 0)
		%mini.maxRounds = getRandom((%mode.minRandomRounds > 0 ? %mode.minRandomRounds : 1), (%mode.maxRandomRounds > 0 ? %mode.maxRandomRounds : 4));

	%mini.round = (%setRound > 0 ? mClampF(%setRound, 1, %mini.maxRounds) : 1);

	%mode.onInit(%mini);
}

//////////////////////////////////////////////////////////////////////////////////////////

function MinigameSO::LoadGames(%mini, %newGames)
{
	//Add your game types and special rounds here
	//Fields to know (Game/Special): Remember, special will always override!
	//  playerDatablock <PlayerData data>          -- When players spawn this will set their datablock - name is highly recommended
	//  playerSpeed <float speed>                  -- Speed factor based on what datablock they currently have
	//  playerWeapon[<int slot>] <ItemData item>   -- (DO NOT USE ShapeBaseImageData) Forces an item to equip

	if(%newGames)
	{
		if(isObject(%mini.games))
			%mini.games.delete();

		if(isObject(%mini.specials))
			%mini.specials.delete();
	}

	if(!isObject(%mini.games))
	{
		%mini.games = new ScriptGroup(%mini @ "_Games")
		{
			superclass = "MinigameGroup";
			class = "MiniGames"; // You may edit the class
			isGameGroup = 1;
		};
	}

	if(!isObject(%mini.specials))
	{
		%mini.specials = new ScriptGroup(%mini @ "_Specials")
		{
			superclass = "MinigameGroup";
			class = "MiniSpecials"; // You may edit the class
			isSpecialGroup = 1;
		};
	}

	//You can add custom games through text files (eventually) and make your functions under --> ./Games/*
	//These are default and MUST be default, if you remove these make sure you at least have a gamemode

	//DeathMatch
	%mini.games.add(new ScriptObject()
	{
		parent = %mini.games;
		superclass = "MiniGame";
		class = "DM";

		uiName = "DeathMatch";
		description = "Everyone against each other!";
		ver = 1;
	});

	//Team DeathMatch
	%mini.games.add(new ScriptObject()
	{
		parent = %mini.games;
		superclass = "MiniGame";
		class = "TDM";
		
		uiName = "Team DeathMatch";
		description = "Fight together against other teams!";
		ver = 1;
	});

	// Map exclusive - unfinished feature
	if(!isObject(%mini.specialMapList))
		%mini.specialMapList = new GuiTextListCtrl();

	// Used by the default - gamemodes will override
	if(!isObject(%mini.specialList))
		%mini.specialList = new GuiTextListCtrl();

	%mini.specialList.clear();

	// example
	// %mini.specials.add(new ScriptObject()
	// {
	// 	parent = %mini.specials;
	// 	superclass = "RndSpecial";
	// 	class = "RndSpeed";
		
	// 	uiName = "Speed";
	// 	description = "Player speed increases";

	// 	playerSpeed = 1.5;
	// 	ver = 1;
	// });
}

//////////////////////////////////////////////////////////////////////////////////////////

function MiniGame::onRoundStart(%game, %mini)
{

}

function MiniGame::onPreRoundStart(%game, %mini)
{

}

function MiniGame::onAdd(%game)
{
	if(!isObject(%game.parent))
	{
		echo("Game is invalid - you must put it in a group: [" @ %game @ "] " @ %game.uiName);
		backtrace();
		%game.schedule(0, "delete");
		return 0;
	}

	if(%game.uiName $= "")
	{
		%game.schedule(0, "delete");
		return 0;
	}

	if(isObject(%found = %game.parent.find(%game.uiName, %game)) && %found.ver >= %game.ver)
	{
		echo("Game already exists: " @ %game.uiName);
		%game.schedule(0, "delete");
		return 0;
	}

	echo("Added game: " @ %game.uiName);
	return 1;
}

function MiniGame::onInit(%game, %mini)
{
	if(!isObject(%mini))
		return 0;

	echo("[" @ %mini.displayName @ "] [" @ %game.uiName @ "] Mode initiate success");

	return 1;
}

function MiniGame::onPreSpawn(%mode, %mini, %client)
{
	if(!isObject(%mini))
		return 0;

	if(!isObject(%player = %client.player))
		return;
}

function MiniGame::onSpawn(%mode, %mini, %client)
{
	if(!isObject(%mini))
		return 0;

	if(!isObject(%player = %client.player))
		return;

	if(getSimTime() - %client.lastPlayedNewRound > 500)
	{
		%client.lastPlayedNewRound = getSimTime();
		if(isObject(%sound = %mode.initRoundSound))
			%client.play2D(%sound);
		else if(isObject(%sound = %mini.initRoundSound))
			%client.play2D(%sound);
	}

	%player.clearTools();

	%player.setInvulnerbilityTime(8);
	if(%mini.WaveSpawnCount <= 1 && %mini.overhealHealth > 0)
		%player.vAddOverheal(%mini.overhealHealth, %mini.overhealDecay);

	return 1;
}

////////////////////////////////////

//DEATHMATCH

function DM::onInit(%mode, %mini)
{
	//Usually this is handled but just to be sure
	if(!isObject(%mini))
		return 0;

	Parent::onInit(%mode, %mini);
}

function DM::onSpawn(%mode, %mini, %client)
{
	if(!isObject(%mini))
		return 0;

	Parent::onSpawn(%mode, %mini, %client);

	return 1;
}

function DM::onRoundStart(%mode, %mini)
{
	//Usually this is handled but just to be sure
	if(!isObject(%mini))
		return 0;

	//%mini.messageAll('', "\c6Round [\c3" @ %mode.uiName @ "\c6] has begun!");
	echo("[" @ %mini.displayName @ "] [" @ %mode.uiName @ "] Round start success");

	return 1;
}

//TEAM DEATHMATCH

package CustomMinigameCore_Teams
{
	function brickSpawnPointData::onLoadPlant(%this, %obj)
	{
		Parent::onLoadPlant(%this, %obj);
		if(isObject(%obj) && %obj.isPlanted)
		{
			%id = %obj.colorID;

			%name = "TDMSpawnGroup_" @ $DefaultMinigame._teamIDFromColorID[%id];
			if(!isObject(%name))
			{
				%name = new SimSet(%name);
			}
			else
			{
				%name = nameToID(%name);
			}

			%name.add(%obj);
		}
	}

	function brickSpawnPointData::onPlant(%this, %obj)
	{
		Parent::onPlant(%this, %obj);
		if(isObject(%obj) && %obj.isPlanted)
		{
			%id = %obj.colorID;

			%name = "TDMSpawnGroup_" @ $DefaultMinigame._teamIDFromColorID[%id];
			if(!isObject(%name))
			{
				%name = new SimSet(%name);
			}
			else
			{
				%name = nameToID(%name);
			}

			%name.add(%obj);
		}
	}

	function fxDtsBrick::setColor(%brick, %id)
	{
		if(%brick.isPlanted && %brick.getDatablock().getName() $= "brickSpawnPointData" && %brick.colorid != %id && isObject($DefaultMinigame))
		{
			if($DefaultMinigame._teamIDFromColorID[%id] !$= "") // new
			{
				%name = "TDMSpawnGroup_" @ $DefaultMinigame._teamIDFromColorID[%id];
				if(!isObject(%name))
				{
					%name = new SimSet(%name);
				}
				else
				{
					%name = nameToID(%name);
				}

				for(%i = 0; %i < $DefaultMinigame._teams; %i++)
				{
					if(%i == %id)
						continue;
					
					%cname = "TDMSpawnGroup_" @ %i;
					if(isObject(%cname = nameToID(%cname)) && %cname.isMember(%brick))
						nameToID(%cname).remove(%brick);
				}

				%name.add(%brick);
			}
			else
			{
				for(%i = 0; %i < $DefaultMinigame._teams; %i++)
				{
					if(%i == %brick.colorid)
						continue;

					%name = "TDMSpawnGroup_" @ %i;
					if(isObject(%name = nameToID(%name)) && %name.isMember(%brick))
						nameToID(%name).remove(%brick);
				}
			}
		}

		Parent::setColor(%brick, %id);
	}
};
activatePackage(CustomMinigameCore_Teams);

function TDM::pickSpawnPoint(%mode, %mini, %client)
{
	%pos = "0 0 0";
	%name = "TSpawn_" @ %client.teamID;
	for(%i = 0; %i < MainBrickGroup.getCount(); %i++)
	{
		%group = MainBrickGroup.getObject(%i);
		if((%teleBrickCount = %group.NTObjectCount_[%name]) > 0)
			%pos = %group.NTObject_[%name, getRandom(0, %teleBrickCount-1)].getTopPosition();
	}

	if(%pos $= "0 0 0")
	{
		%name = nameToID("TDMSpawnGroup_" @ %client.teamID);
		if(isObject(%name) && (%c = %name.getCount()) > 0)
		{
			%brick = %name.getObject(getRandom(0, %c-1));
			%pos = %brick.getSpawnPoint();
		}
	}

	return %pos;
}

function TDM::onInit(%mode, %mini)
{
	//Usually this is handled but just to be sure
	if(!isObject(%mini))
		return 0;

	Parent::onInit(%mode, %mini);

	if(%mini._teams > 0)
	{
		for(%i = 0; %i < %mini._teams; %i++)
		{
			%str = %mini._teamName[%i] TAB %mini._teamColor[%i] TAB %mini._teamNoAutoJoin[%i];

			if(%teamList $= "")
				%teamList = %str;
			else
				%teamList = %teamList NL %str;
		}

		%mini.createTeamListFromString(%teamList);
		echo("[" @ %mini.displayName @ "] Creating teams from core preferences.");
	}
	else
	{
		%div = %mode.minPlayersPerTeam;
		%c = mFloor(%mini.numMembers / %mode.minPlayersPerTeam) * %mode.minPlayersPerTeam;

		%teamList = "Red\t1 0 0 1" NL "Blue\t0 0 1 1";

		%teamList[1] = "Green\t0.2 1 0.2 1";
		%teamList[2] = "Yellow\t1 1 0.2 1";
		%teamList[3] = "Pink\t1 0.2 1 1";
		%teamList[4] = "Cyan\t0.2 1 1 1";

		%maxTeams = 2;
		for(%i = 1; %i <= 4; %i++)
		{
			if(%c / (%div + %i) >= %div)
				%maxTeams++;
		}

		%teams = getRandom(2, %maxTeams);
		for(%i = 1; %i <= %teams; %i++)
		{
			%teamList = %teamList NL %teamList[%i];
		}
		echo("[" @ %mini.displayName @ "] Creating teams from default preferences.");

		%mini.createTeamListFromString(trim(%teamList));
	}

	return 1;
}

function TDM::onPreRoundStart(%mode, %mini)
{
	if(!isObject(%mini))
		return 0;

	%mini.sortTeams();

	return 1;
}

function TDM::onRoundStart(%mode, %mini)
{
	//Usually this is handled but just to be sure
	if(!isObject(%mini))
		return 0;

	//%mini.messageAll('', "\c6Round [\c3" @ %mode.uiName @ "\c6] has begun!");
	echo("[" @ %mini.displayName @ "] [" @ %mode.uiName @ "] Round start success");

	return 1;
}

function TDM::onSpawn(%mode, %mini, %client)
{
	if(!isObject(%mini))
		return 0;

	Parent::onSpawn(%mode, %mini, %client);
	%client.chatMessage("<sPush><font:impact:25>\c6You are on team " @ %client.team.colorHexStr @ %client.team.name @ "\c6!<sPop>");
	// %client.schedule(3500, centerprint, "<sPush><font:impact:28>\c6You are on team " @ %client.team.colorHexStr @ %client.team.name @ "\c6!<sPop>\n\c5Do not shoot at your teammates!", 1);
	// %client.schedule(3800, centerprint, "<sPush><font:impact:28>\c6You are on team " @ %client.team.name @ "\c6!<sPop>\n\c5Do not shoot at your teammates!", 1);
	// %client.schedule(4100, centerprint, "<sPush><font:impact:28>\c6You are on team " @ %client.team.colorHexStr @ %client.team.name @ "\c6!<sPop>\n\c5Do not shoot at your teammates!", 1);
	// %client.schedule(4400, centerprint, "<sPush><font:impact:28>\c6You are on team " @ %client.team.name @ "\c6!<sPop>\n\c5Do not shoot at your teammates!", 1);
	// %client.schedule(4700, centerprint, "<sPush><font:impact:28>\c6You are on team " @ %client.team.colorHexStr @ %client.team.name @ "\c6!<sPop>\n\c5Do not shoot at your teammates!", 1);

	return 1;
}