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
		if(!%this.dataInstance($DR::SaveSlot).DR_pointFixOnce && %this.score > %this.dataInstance($DR::SaveSlot).DR_totalPoints && %this.score > 0)
		{
			%this.dataInstance($DR::SaveSlot).DR_pointFixOnce = 1;
			%this.dataInstance($DR::SaveSlot).DR_totalPoints = %this.score;
		}

		if(%score > 0)
			%this.dataInstance($DR::SaveSlot).DR_totalPoints += %score;
		
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
			%sourceClient.dataInstance($DR::SaveSlot).DR_giveDamage += mClampF(%damage, 0, %data.maxDamage);
		}

		if(isObject(%client))
		{
			%client.dataInstance($DR::SaveSlot).DR_takeDamage += mClampF(%damage, 0, %data.maxDamage);
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
	return %this.dataInstance($DR::SaveSlot).DR_PlayTime + ($Sim::Time - %this.TotalPlayTime);
}

function GameConnection::DeathRace_Save(%this)
{
	%this.dataInstance($DR::SaveSlot).DR_PlayTime = %this.getTotalPlayTime() ;
    %this.dataInstance($DR::SaveSlot).DR_Score = %this.score;
	%this.dataInstance_ListSave();

    echo("\'" @ %this.name @ "(" @ %this.getBLID() @ ")\' profile has been saved.");
}

function GameConnection::DeathRace_Load(%this) {
	%this.dataInstance_ListLoad();

	if(%this.dataInstance($DR::SaveSlot).DR_totalPoints $= "")
		%this.dataInstance($DR::SaveSlot).DR_totalPoints = %this.score;

	if(%this.dataInstance($DR::SaveSlot).DR_noHUD $= "")
		%this.dataInstance($DR::SaveSlot).DR_noHUD = 0;

	if(%this.dataInstance($DR::SaveSlot).DR_GUIPerRound $= "")
		%this.dataInstance($DR::SaveSlot).DR_GUIPerRound = 1;

	if(%this.dataInstance($DR::SaveSlot).DR_MapGUI $= "")
		%this.dataInstance($DR::SaveSlot).DR_MapGUI = 1;

	if(%this.dataInstance($DR::SaveSlot).savedLoadout[0] $= "")
		%this.dataInstance($DR::SaveSlot).savedLoadout[0] = $DR::DefaultLoadout;
	
	if(%this.dataInstance($DR::SaveSlot).LastLoadOut $= "")
		%this.dataInstance($DR::SaveSlot).LastLoadOut = $DR::DefaultLoadout;

	%this.setScore(%this.dataInstance($DR::SaveSlot).DR_Score + 0);

	%bl_id = %this.getBLID();

	%count = Server_TitleGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%titleObj 	= Server_TitleGroup.getObject(%i);
		if(hasItemOnList(%titleObj.bl_idList, %bl_id) || %this.isAdmin && %titleObj.bl_idList $= "admin")
			%this.unlockTitle(%titleObj);
	}

	if(isObject(%title = Server_TitleGroup.find(%this.dataInstance($DR::SaveSlot).title)))
		%this.setTitle(%title, 1);

	echo("'" @ %this.name @ "' profile has been loaded.");
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

	%this.getTotalPlayTime();
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
	%this.maxOutOfVehicleTimer = mFloor(%maxLimit);
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