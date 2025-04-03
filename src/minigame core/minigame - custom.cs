// Ones you should edit: Replace CustomMinigameSO with your own class

function CustomMinigameSO::onCreate(%mini)
{
	if(!isObject(%mini.games))
		%mini.LoadGames();

	if(!isObject(%mini.mode))
		%mini.setMode("Team DeathMatch");
}

function CustomMinigameSO::onCleanUp(%mini)
{
	
}

function CustomMinigameSO::onModeChange(%mini)
{
	
}

function CustomMinigameSO::onRoundStart(%mini)
{
	%found = 0;
	%foundC = 0;
	for(%i = 0; %i < MainBrickGroup.getCount(); %i++)
	{
		%group = MainBrickGroup.getObject(%i);
		%c = %group.getCount();

		if(%c > %foundC)
		{
			%foundC = %c;
			%found = %group;
		}
	}

	if(%foundC > 0)
		%found.getObject(0).StartDeathRace(%mini.DRBeginTime, 0);
	else
		talk("Unable to start DeathRace. No public bricks are loaded.");
}

function CustomMinigameSO::onRoundEnd(%mini)
{
	
}

function CustomMinigameSO::onFinalRound(%mini)
{

}

function CustomMinigameSO::onMaxRoundLimit(%mini)
{
	
}

function CustomMinigameSO::onPreRoundStart(%mini)
{
	
}

function CustomMinigameSO::onPreSpawn(%mini, %client)
{
	
}

function CustomMinigameSO::onSpawn(%mini, %client)
{
	if(isObject(%player = %client.player))
	{
		if(isObject(%team = %client.team) && isObject(%mini.Teams))
		{
			%client.nameColor = %team.colorHex;
			%player.setShapeNameColor(%team.color);
		}
		else
		{
			%client.nameColor = "";
			%player.setShapeNameColor("0.8 0.8 0.8 1");
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////

// This is so dumb
function CustomMinigameSO::addMember(%mini, %client)
{
	if(%mini.breakSpawnMessages && %mini.RespawnTime == -1)
	{
		%deleteClientPlayer = 1;
		%client.InstantRespawn();
	}

	if(%client.loadnamecolor $= "")
		%client.loadnamecolor = %client.namecolor;

	if(%deleteClientPlayer && isObject(%client.player))
		%client.player.delete();

	Parent::addMember(%mini, %client);

	if(%client.team $= "")
	{
		%client.team = 0;
		%client.teamID = -1;
	}
}

function CustomMinigameSO::removeMember(%mini, %client)
{
	if(isObject(%team = %client.team))
		%team.removeClient(%client, 1, 1);

	if(%client.oldchestcolor !$= "")
		%client.chestColor = %client.oldchestcolor;

	Parent::removeMember(%mini, %client);
	%client.namecolor = %client.loadnamecolor;
}

function CustomMinigameSO::Reset(%mini, %client)
{
	if(%client $= "")
		%client = 0;

	if(getSimTime() - %mini.LastResetTime < 500)
	{
		echo("[" @ %mini.displayName @ "] Resetting too quickly!");
		return Parent::Reset(%mini, %client);
	}


	%mini.round++;
	if($Pref::Server::MapSys_VoteType == 1 && %mini.maxRounds != -1 && %mini.round > %mini.maxRounds)
	{
		if(%mini.numMembers > 1)
		{
			%r = %mini.onMaxRoundLimit();
			$Server::Temp::MapSys_Change = 1;
			if(!isEventPending($Temp::MapSys_TickSch) && ($Pref::Server::MapSys_VoteType == 2 || $Pref::Server::MapSys_VoteType == 3))
			{
				%mini.round = 0;	
				MapSys_Loop($Pref::Server::MapSys_MapTimeLimit);
			}
			// else { do random map change }
		}
		else
			%mini.round = 1;
	}
	// else if(!isEventPending($Temp::MapSys_TickSch) && ($Pref::Server::MapSys_VoteType == 2 || 
	//$Pref::Server::MapSys_VoteType == 3)) //Start the tick if VoteType is 2/3
	// {
	//	%minigame.round = 0;
	//	MapSys_Loop($Pref::Server::MapSys_MapTimeLimit);
	// }

	if($Server::Temp::MapSys_Change)
	{
		$Server::Temp::MapSys_Change = 0;

		cancel($Temp::MapSys_ErrorSch);
		// For whatever reason the map fails to change we have to fix it
		$Temp::MapSys_ErrorSch = schedule(10000, 0, "MapSys_ResetMap", "MapChangeError");

		// Change the map to the next one - either voted or in an order
		MapSys_SetMapNext();
		return;
	}

	cancel(%mini.WaveSpawnSch);

	%mini.roundDisplayName = "";
	if(isObject(%curSpecial = %mini.special))
	{
		if(isFunction(%curSpecial.class, "onCleanUp"))
			%curSpecial.onCleanUp(%mini);
	}

	// edit for map changers
	if(%mini.inMapChange)
		return;

	echo("[" @ %mini.displayName @ "] Reset complete");
	if(%mini.enableMusic && !isObject(%song = findMusicByName(%mini.tempSong)))
		%song = MusicDataCache.item[getRandom(0, MusicDataCache.itemCount-1)];

	%mini.tempSong = "";
	if(isObject(%mini.playMusicData)) // overrides enable music option
		%song = %mini.playMusicData;

	if(isObject(%song))
	{
		%mini.setMusic(%song, 0.9);
		%mini.messageAll('', '\c6Now playing: \c3%1', %song.uiName);
	}

	%mini.isPreRound = 1;
	%mini.special = 0;
	%mini.specialName = "";

	%mini.lives = %mini._lives;
	%mini.RespawnTime = %mini._RespawnTime;
	%mini.TimeLimit = %mini._TimeLimit;

	%mini.WaveSpawn = %mini._WaveSpawn;
	%mini.WaveSpawnTime = %mini._WaveSpawnTime;

	if(%mini.allowSpecials && (getRandom(1, %mini.specialChance) == 1 || %mini.specialOverride !$= ""))
	{
		if(%mini.specialOverride == 1)
		{
			%name = %mini.specialList.getRowText(getRandom(0, %mini.specialList.rowCount()-1));
			%special = %mini.specials.find(%name);
			if(isObject(%special))
				%mini.setSpecial(%special);
		}
		else
			%mini.setSpecial(%mini.specialOverride);

		%mini.specialOverride = "";
	}

	%old = %mini.mode;
	if(!isObject(%mini.mode))
	{
		%max = %mini._maxRounds;
		%mini.maxRounds = "";

		if(%mini.lockedMode !$= "")
			%mini.setMode(%mini.lockedMode, %mini.round);
		if(%mini.modeName !$= "")
			%mini.setMode(%mini.modeName, %mini.round);
		else
			%mini.setMode("DeathMatch", 1);

		if(%mini.lockedMaxRound > 0)
			%mini.maxRounds = %mini.lockedMaxRound;
		else if(%mini.maxRounds $= "")
			%mini.maxRounds = %mini._maxRounds;

		%mini.onModeChange();
	}
	else if(%mini.maxRound != -1 && %mini.round == %mini.maxRound)
	{
		%mini.onFinalRound();
	}

	if(%mini.lives == 1)
		%mini.RespawnTime = -1;
	else if(%mini.lives !$= "" && %mini.lives >= 0)
		%mini.RespawnTime = 5000;

	if(%mini.WaveSpawn && %mini.WaveSpawnTime > 3)
	{
		%mini.WaveSpawnSch = %mini.schedule(%mini.WaveSpawnTime * 1000, CheckWaveSpawn);
		%mini.WaveSpawnCount = 0;
		%mini.LastWaveSpawn = $Sim::Time;
	}

	if($Pref::Server::MapSys_Enabled && %mini.maxRounds > 0 && $Pref::Server::MapSys_VoteType == 1)
	{
		%lft = (%mini.maxRounds - %mini.round);
		if(%lft == 0)
			%voteMsg = " \c6(Map vote after this round)";
		else
			%voteMsg = " \c6(Map vote in \c3" @ %lft @ "\c6 more round" @ (%lft == 1 ? "" : "s") @ ")";
	}

	if(isObject(%special = %mini.special))
	{
		%specialStr = %special.uiName @ " ";
		if(%mini.roundDisplayName !$= "")
			%specialStr = %mini.roundDisplayName @ " ";
	}

	if(%mini.maxRounds != -1)
		%mini.messageAll('', "\c6" @ (%mini.lives >= 1 ? "[\c3Minigame - " @ %mini.lives @ " " 
		@ (%mini.lives == 1 ? "Life" : "Lives") @ "\c6] " : "[\c3Minigame\c6] ") @ %mini.displayName @ " - \c3" @ %specialStr @ %mini.modeName @ %voteMsg);
	else
		%mini.messageAll('', "\c6" @ (%mini.lives >= 1 ? "[\c3Minigame - " @ %mini.lives 
		@ " " @ (%mini.lives == 1 ? "Life" : "Lives") @ "\c6] " : "[\c3Minigame\c6] ") @ %mini.displayName @ " (Round \c3" @ %mini.round 
		@ "\c6/\c3" @ %mini.maxRounds @"\c6) - \c3" @ %specialStr @ %mini.modeName @ %voteMsg);

	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%cl = %mini.member[%i];
		%cl.roundKills = 0;
		%cl.roundPoints = 0;
	}

	//determine active and inactive teams
	%teams = %mini.teams;
	%count = %teams.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%team = %teams.getObject(%i);
		if(%team.name $= "Admin Team")
		{
			continue;
		}

		%team.avoidAutoJoin = true;
	}

	%activeTeamCount = mClamp((mCeil(%mini.numMembers / 6)),2,%count-1);
	for(%i = 0; %i < %activeTeamCount; %i++)
	{
		%team = %teams.getObject(getRandom(0,%count-1));
		if(%team.name $= "Admin Team")
		{
			%activeTeamCount++;
			continue;
		}

		%team.avoidAutoJoin = false;
	}

	%mini.onPreRoundStart();
	%mini.mode.onPreRoundStart(%mini);

	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%mini.member[%i].Deathrace_Save();
	}

	%p = Parent::Reset(%mini, %client);

	%mini.isPreRound = 0;
	//%mini.spawnItems();
	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%cl = %mini.member[%i];
		%cl.Deathrace_Load();
	}

	%mini.onRoundStart();
	%mini.mode.onRoundStart(%mini);

	if(isObject(%special = %mini.special))
		%special.onRoundStart(%mini);

	%mini.resetting = 0;
	return %p;
}

function CustomMinigameSO::timeLimitTick(%mini, %echo)
{
	if(%mini.inMapChange)
		return;

	%elapsedTime = getSimTime() - %mini.lastResetTime;
	%timeRemaining = %mini.TimeLimit * 1000 - %elapsedTime;
	cancel(%mini.timeLimitSchedule);

	if(%mini.numMembers == 0)
	{
		%mini.timeLimitSchedule = %mini.schedule(1000, timeLimitTick, 1);
		return;
	}

	if (%timeRemaining <= 10.0)
	{
		if(isObject(%mini.Teams) && %mini.Teams.getCount() > 0)
		{
			for(%i = 0; %i < %mini.numMembers; %i++)
			{
				%cl = %mini.member[%i];
				%clTeam[%cl.team] += (%cl.roundKills + %cl.roundPoints);

				if(%clTeam[%cl.team] > %highScore && isObject(%pl = %cl.player) && %pl.getState() !$= "dead")
				{
					%highScore = %clTeam[%cl.team];
					%highScoreClient = %cl;
					%highScoreTeam = %cl.team;
				}
			}

			if(isObject(%highScoreTeam))
				%mini.messageAll('', '\c6Times up! Team \c3%2%1 \c6won the match by highest kills! Resetting!', %highScoreTeam.uiName, %highScoreTeam.color);
			else
				%mini.messageAll('', '\c6Times up! Nobody wins! Resetting!');
		}
		else
		{
			for(%i = 0; %i < %mini.numMembers; %i++)
			{
				%cl = %mini.member[%i];
				%pts = %cl.roundKills + %cl.roundPoints;
				if(%pts > %highScore && isObject(%pl = %cl.player) && %pl.getState() !$= "dead")
				{
					%highScore = %pts;
					%highScoreClient = %cl;
				}
			}

			if(isObject(%highScoreClient))
				%mini.messageAll('', '\c6Times up! \c3%1 \c6won the match! Resetting!', %highScoreClient.getPlayerName());
			else
				%mini.messageAll('', '\c6Times up! Nobody wins! Resetting!');
		}

		%mini.scheduleReset();
		%mini.resetting = 1;
		%mini.onRoundEnd();
		return;
	}
	else
	{
		if(%timeRemaining <= 10000)
		{
			%mini.timeLimitSchedule = %mini.schedule(1000, timeLimitTick, 1);
		}
		else if(%timeRemaining <= 30000)
		{
			%mini.timeLimitSchedule = %mini.schedule(%timeRemaining - 10000, timeLimitTick, 1);
		}
		else if(%timeRemaining <= 60000)
		{
			%mini.timeLimitSchedule = %mini.schedule(%timeRemaining - 30000, timeLimitTick, 1);
		}
		else
		{
			%mini.timeLimitSchedule = %mini.schedule(%timeRemaining - 60000, timeLimitTick, 1);
		}
	}

	if(%echo)
	{
		%timeLeft = mFloor(%timeRemaining / 1000 + 0.5);
		if(%timeLeft < 60)
			%timeStr = %timeLeft @ " second" @ (%timeLeft != 1 ? "s" : "");
		else
			%timeStr = mCeil(%timeLeft / 60) @ " minute" @ (%timeLeft / 60 != 1 ? "s" : "");

		if(%timeLeft >= 10)
			%mini.messageAll('', "\c3" @ %timeStr @ " \c6remaining.");
	}
}

function CustomMinigameSO::checkLastManStanding(%mini)
{
	if(%mini.inMapChange)
		return;
	
	if(%mini.RespawnTime > 0)
		return;

	if(isEventPending(%mini.resetSchedule))
		return;

	%livingTeams = 0;
	%livingTeam = 0;
	%livePlayerCount = 0;
	%liveClient = 0;

	if(isObject(%teams = %mini.Teams) && (%teamCount = %teams.getCount()) > 0)
	{
		for(%i = 0; %i < %teamCount; %i++)
		{
			%team = %teams.getObject(%i);
			%living = %team.getLiving();
			if(%living > 0)
			{
				%livingTeams++;
				%livingTeam = %team;
			}
		}

		if(%livingTeams == 1)
		{
			for(%i = 0; %i < %livingTeam.numMembers; %i++)
			{
				%member = %livingTeam.member[%i];
				%member.dataInstance($DR::SaveSlot).DR_totalWins++;
			}

			%mini.chatMessageAll(0, "<font:arial bold:22>\c6Team \c3" @ %livingTeam.colorHexStr @ %livingTeam.name @ " \c6won the match! Resetting!");
			%mini.scheduleReset();
			%mini.resetting = 1;
			%mini.onRoundEnd();
		}
		else if(%livingTeams == 0)
		{
			%mini.scheduleReset();
			%mini.resetting = 1;
			%mini.onRoundEnd();
		}
	}
	else
	{
		for(%i = 0; %i < %mini.numMembers; %i++)
		{
			%client = %mini.member[%i];
			if(isObject(%player = %client.player) && %player.getDamagePercent() < 1)
			{
				%livePlayerCount++;
				%liveClient = %client;
			}
		}

		if(%livePlayerCount <= 0)
		{
			%mini.scheduleReset();
			%mini.resetting = 1;
			%mini.onRoundEnd();
		}
		else
		{
			if(%livePlayerCount == 1)
			{
				%mini.chatMessageAll(0, "\c4" @ %liveClient.getPlayerName() @ "\c6 won the match! Resetting!");
				if(!%mini.doTimeScale)
				{
					%player = %liveClient.Player;
					for(%i = 0; %i < %mini.numMembers; %i++)
					{
						%client = %mini.member[%i];
						%camera = %client.Camera;
						if (isObject(%camera) && !isObject(%camera.getControlObject()) && %client != %liveClient)
						{
							%camera.setOrbitMode(%player, %camera.getTransform(), 0, 8, 8);
							%camera.mode = "Corpse";
						}
					}
				}

				%mini.scheduleReset();
				%mini.resetting = 1;
				%mini.onRoundEnd();
			}
		}
	}
}