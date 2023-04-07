// DeathRace file

package DeathRace_Player
{
	function GameConnection::createPlayer(%this,%transform)
	{
		%this.lastSpawnTime = getSimTime();
		cancel(%this.DR_SpawnSch);
		%this.DR_SpawnSch = %this.schedule(3000, DR_Spawn);
		return Parent::createPlayer(%this,%transform);
	}

	function GameConnection::incScore(%this,%score)
	{
		if(!%this.DR_pointFixOnce && %this.score > %this.DR_totalPoints && %this.score > 0)
		{
			%this.DR_pointFixOnce = 1;
			%this.DR_totalPoints = %this.score;
		}

		if(%score > 0)
			%this.DR_totalPoints += %score;
		
		if(%this.getScore() < -5)
		{
			%this.setScore(-5);
			return;
		}

		return Parent::incScore(%this,%score);
	}

	function serverCmdSuicide(%this)
	{
		if(!%this.player.cannotSuicide && !%this.minigame.cannotSuicide)
			return Parent::serverCmdSuicide(%this);
	}

	function serverCmdUseTool(%this, %slotID)
	{
		if(isObject(%player = %this.player) && $Pref::Server::DeathRace_DrivingTools)
		{
			if(isObject(%mount = %player.getObjectMount()))
				if(%mount.getMountNodeObject(0) == %player)
				{
					%this.chatMessage("\c5You cannot use any tools while driving! (/WhyNoTools)");
					serverCmdUnUseTool(%this);
					return;
				}
		}

		Parent::serverCmdUseTool(%this, %slotID);	
	}

	function Armor::Damage(%data, %obj, %sourceObject, %position, %damage, %damageType)
	{
		%client = %obj.client;
		%r = Parent::Damage(%data, %obj, %sourceObject, %position, %damage, %damageType);
		if(isObject(%sourceObject))
		{
			if(%sourceObject.getClassName() $= "GameConnection")
			{
				%sourceClient = %sourceObject;
			}
			else
			{
				%sourceClient = %sourceObject.client;
			}
		}
		else
		{
			%sourceClient = 0;
		}
		
		if(isObject(%sourceClient))
		{
			%sourceClient.DR_giveDamage += mClampF(%damage, 0, %data.maxDamage);
		}

		if(isObject(%client))
		{
			%client.DR_takeDamage += mClampF(%damage, 0, %data.maxDamage);
		}

		return %r;
	}

	function Armor::onEnterLiquid(%data, %obj, %coverage, %type)
	{
		Parent::onEnterLiquid(%data, %obj, %coverage, %type);
		if(!isObject(%obj.client.minigame) && %obj.getClassName() $= "Player") return;
			%obj.hasShotOnce = true;

		%obj.invulnerable = false;
		%obj.damage(%obj, %obj.getPosition(), 10000, $DamageType::Lava);
	}
};
activatePackage(DeathRace_Player);

//////////////////////////////////////////////////////////////////////

registerOutputEvent(GameConnection, "NoMinigameDeath",  "bool");
registerOutputEvent(GameConnection, "disableSuiciding", "bool");
registerOutputEvent(GameConnection, "incScoreTeam",     "list Dead 0 Alive 1" TAB "int -1000 1000 0");

function GameConnection::addDRKill(%this, %amt)
{
	%this.DRStreakKills += %amt;
	if(%this.DRStreakKills >= 3)
		%this.forceUnlockAchievement("Bloodthirsty", 1);

	cancel(%this.resetDRKillSch);
	%this.resetDRKillSch = %this.schedule(3000, "resetDRKill");
}

function GameConnection::resetDRKill(%this)
{
	if(!isObject(%this))
		return;

	%this.DRStreakKills = 0;
}

function GameConnection::getTotalPlayTime(%this)
{
	return %this.DR_PlayTime + ($Sim::Time - %this.TotalPlayTime);
}

function GameConnection::DeathRace_Save(%this)
{
    if(!isObject(%this))
        return;
    
    %path = $DeathRace::Profiles @ %this.getBLID() @ ".DeathRaceProfile";
    
    %file = new FileObject();
    %file.openForWrite(%path);
    
    %file.writeLine(%this.score TAB %this.getPlayerName());
    %file.writeLine("takeDamage"		TAB %this.DR_takeDamage			TAB "// How much damage was received");
    %file.writeLine("giveDamage"		TAB %this.DR_giveDamage			TAB "// How much damage was given");
    %file.writeLine("totalKills"		TAB %this.DR_totalKills			TAB "// How much damage was taken");
    %file.writeLine("totalDeaths"		TAB %this.DR_totalDeaths			TAB "// How many times they died");
    %file.writeLine("totalWins"			TAB %this.DR_totalWins			TAB "// How many wins they have");
    %file.writeLine("totalWinsByButton"	TAB %this.DR_totalWinsByButton	TAB "// How many wins they have by pressing button (counts as being alive with team too)");
    %file.writeLine("totalRounds"		TAB %this.DR_totalRounds			TAB "// How many rounds played");
    %file.writeLine("totalPoints"		TAB %this.DR_totalPoints			TAB "// How many points they have received, this counts into players 'cheating' outside of minigame");
    %file.writeLine("totalItemsBought"	TAB %this.DR_totalItemsBought		TAB "// How many items bought");
    %file.writeLine("FirstWin"			TAB %this.DR_FirstWin				TAB "// If they won at least one game");
    %file.writeLine("PlayTime"			TAB %this.getTotalPlayTime() 					TAB "// How much they have played");
    %file.writeLine("pointFixOnce"		TAB %this.DR_pointFixOnce 		TAB "// Point fix");

    %file.writeLine("HUD"				TAB %this.DR_HUD					TAB "// Setting");
    %file.writeLine("GUIHUD"			TAB %this.DR_GUIHUD				TAB "// Setting");
    %file.writeLine("GUIPerRound"		TAB %this.DR_GUIPerRound 			TAB "// Setting");
    %file.writeLine("MapGUI"			TAB %this.DR_MapGUI				TAB "// Setting");
    %file.writeLine("HUDPassenger"		TAB %this.DR_HUDPassenger 		TAB "// Setting");

    // Map data
    for(%i = 1; %i <= $Server::MapSys_MapCount; %i++)
    {
    	%name = $Server::MapSys_MapName[%i];
    	%varName = "totalButtonWinsOn" @ getSafeVariableName(%name);
    	%file.writeLine(%varName		TAB %this.DeathRaceData[%varName] 				TAB "// Map data - wins");
    }

    echo("\'" @ %this.name @ "(" @ %this.getBLID() @ ")\' profile has been saved.");
    %file.close();
    %file.delete();
}

function GameConnection::DeathRace_Load(%this) {
	if(!isObject(%this)) return;
	
	%bl_id = %this.getBLID();
	if(isFile(%file = "config/server/Achievements/" @ %bl_id @ ".cs"))
		exec(%file);

	if(%this.DR_totalPoints $= "")
		%this.DR_totalPoints = %this.score;

	if(%this.DR_HUD $= "")
		%this.DR_HUD = 1;

	if(%this.DR_GUIHUD $= "")
		%this.DR_GUIHUD = 1;

	if(%this.DR_GUIPerRound $= "")
		%this.DR_GUIPerRound = 1;

	if(%this.DR_MapGUI $= "")
		%this.DR_MapGUI = 1;

	%path = $DeathRace::Profiles @ %this.getBLID() @ ".DeathRaceProfile";
	if(!isFile(%path))
	{
		echo("\'" @ %this.name @ "(" @ %this.getBLID() @ ")\' does not have a profile \'" @ %path @ "\'. Creating one on leave.");
		return;
	}
	%file = new FileObject();
	%file.openForRead(%path);
	
	echo("'" @ %this.name @ "' profile has been loaded.");
	
	%this.setScore(getField(%file.readLine(), 0));
	while(!%file.isEOF()) {
		%line = %file.readLine();
		%this.DeathRaceData[getField(%line,0)] = getField(%line, 1);
	}
	%file.close();
	%file.delete();
}

function GameConnection::SendLeaderboard(%client)
{
	if(!%client.Shop_Client)
		return;

	// name totalPoints giveDamage totalKills totalDeaths totalWins totalRounds totalItemsBought PlayTime placeScore
	commandToClient(%client, 'DRShop', "ClearLeaderboard");
	%count = DR_LeaderboardList.rowCount();
	for(%i = 0; %i < %count; %i++)
	{
		%string = getFields(DR_LeaderboardList.getRowText(%i), 0, 8);
		%id = DR_LeaderboardList.getRowID(%i);

		if(getField(%string, 0) !$= "" && %id >= 0)
		{
			commandToClient(%client, 'DRShop', "AddLeaderboardClient", %id, %string TAB %i+1);
		}
	}
}

function GameConnection::incScoreTeam(%this, %mode, %score, %bool)
{
	if(!isObject(%mini = %this.minigame)) return; // no minigame
	if(!isObject(%team = %this.getTeam())) return; // no team

	%team.incScoreAll(%mode, %score, (%bool ? %client : 0));
}

function GameConnection::DR_Spawn(%this)
{
	cancel(%this.DR_SpawnSch);

	if(!isObject(%player = %this.player))
		return;

	%this.UpdateToLeaderboard();
	%player.DR_SpawnPosition = %player.getPosition();
	%this.spyObj = 0;

	%this.DR_PlayTime += ($Sim::Time - %this.TotalPlayTime);
	%this.TotalPlayTime = $Sim::Time;
	%this.unlockAchievement("Deathrace Marathon");
	%this.unlockAchievement("Deathrace Addiction");

	if(%this.TitleData["Dank"] && getRandom(1, 6) == 1)
		%player.ThreadLoop("HeadSide", 500);

	if(isObject(%mini = %this.minigame))
	{
		if(%mini.playerScale !$= "")
			%player.setPlayerScale(%mini.playerScale);

		if(%mini.tempPlayerData != 0 && isObject(%mini.tempPlayerData))
			%player.changeDatablock(%mini.tempPlayerData);

		if(%mini.DR_doubleHealth)
			%player.addMaxHealth(100);

		%player.setVehicleLimit($Pref::Server::VehicleLimitTime, %this.minigame.DR_time + 10);
	}
}

function GameConnection::DR_toTeamLivingString(%this)
{
	if(!isObject(%mini = %this.minigame))
		return "";

	if(!%mini.isCustomMinigame)
		return "";

	if(%mini.lives <= 0 || %mini.lives $= "")
		return "";

	if(!isObject(%team = %this.team))
	{
		if(%mini.getLiving() == 1)
			return "\c3Last man";
		else
			return "\c6Alive: \c3" @ %mini.getLiving() @ "\c6/\c3" @ %mini.numMembers;
	}

	if(%team.getLiving() == 1)
		return "\c3Last man on team";
	else
		return "\c6Team alive: \c3" @ %team.getLiving() @ "\c6/\c3" @ %team.numMembers;
}

function GameConnection::NoMinigameDeath(%this,%bool)
{
	if(!isObject(%mini = %this.minigame))
		return;
	
	if(%bool && !%mini.cannotSuicide)
		%mini.messageAll('MsgUploadEnd',%this.getPlayerName() @ " has entered into the no suicide zone. No one cannot suicide.");
	else if(!%bool && %mini.cannotSuicide)
		%mini.messageAll('MsgUploadEnd',%this.getPlayerName() @ " has exited out of the no suicide zone. Everyone is able to suicide.");
	%mini.cannotSuicide = %bool;
}

function GameConnection::disableSuiciding(%this,%bool)
{
	if(isObject(%pl = %this.player))
		%pl.cannotSuicide = %bool;
}

/////////////////////////////////////////////////////////////////////

registerOutputEvent(Player, resetMovementSpeed);
registerOutputEvent(Player, setBaseMovementSpeed, "int 0 200");

registerOutputEvent(Bot, resetMovementSpeed);
registerOutputEvent(Bot, setBaseMovementSpeed, "int 0 200");

function Player::setVehicleLimit(%this,%maxLimit,%time)
{
	if(!isObject(%this)) return;
	cancel(%this.VehicleLimitSchA);

	%this.maxVehicleLimit = mFloor(%maxLimit);
	%this.vehicleLimitTime = %this.maxVehicleLimit;
}

//Reset the speed to their datablock's max speed
function Player::resetMovementSpeed(%this)
{
	%data = %this.getDatablock();
	%this.setMaxForwardSpeed(%data.MaxForwardSpeed);
	%this.setMaxBackwardSpeed(%data.MaxBackwardSpeed);
	%this.setMaxSideSpeed(%data.MaxSideSpeed);
	%this.setMaxCrouchForwardSpeed(%data.MaxCrouchForwardSpeed);
	%this.setMaxCrouchBackwardSpeed(%data.MaxCrouchBackwardSpeed);
	%this.setMaxCrouchSideSpeed(%data.MaxCrouchSideSpeed);
}

//Set the player's total movement speed
function Player::setBaseMovementSpeed(%this,%value)
{
	if(%value < 0) %value = 0;
	if(%value > 200) %value = 200;
	%this.setMaxForwardSpeed(%value);
	%this.setMaxBackwardSpeed(%value);
	%this.setMaxSideSpeed(%value);
	%this.setMaxCrouchForwardSpeed(%value);
	%this.setMaxCrouchBackwardSpeed(%value);
	%this.setMaxCrouchSideSpeed(%value);
}

function Player::ThreadLoop(%this, %thread, %time)
{
	if(!isObject(%this)) return;
	if(%thread $= "root") return;
	cancel(%this.ThreadLoop);
	//if(%this.getState() $= "dead") return;
	%this.playThread(0, %thread);
	%this.playThread(1, %thread);
	%this.playThread(2, %thread);
	%this.playThread(3, %thread);
	%this.ThreadLoop = %this.schedule(%time, "ThreadLoop", %thread, %time);
}

function Player::DR_toHealthString(%player)
{
	if(!isObject(%player))
		return "";

	if(!isObject(%client = %player.client))
		return "";

	return "\c6Health: \c3" @ mCeil(%player.getHealth() / %player.getMaxHealth() * 100) @ "\c6%";
}

function Player::DR_toSongString(%this)
{
	return "";
}

/////////////////////////////////////////////////////////////////////

function AIPlayer::DR_toSongString(%this)
{
	return "";
}