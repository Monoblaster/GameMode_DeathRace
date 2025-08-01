function serverCmdRejoin(%client)
{
	if(!isObject(%client.minigame))
	{
		$DefaultMinigame.addMember(%client);
		if(isObject(%client.minigame))
			%client.minigame = $DefaultMinigame.getID();
	}
}

///////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////

function SimObject::Mini_CanDamage(%this, %target)
{
	if(!isObject(%this) || !isObject(%target))
		return false;


	switch$(%mainClass = %this.getClassName())
	{
		case "Player" or "AIPlayer":
			%mainObj = %this;
			%mainClient = %this.client;

		case "Projectile":
			%projectile = %this;
			%mainObj = %this.sourceObject;
			%mainClient = %this.sourceClient;
			if(!isObject(%mainClient))
				%mainClient = %this.client;

		case "GameConnection":
			%mainObj = %this.player;
			%mainClient = %this;

		case "fxDTSBrick":
			%mainObj = %this;
			%mainClient = %this.client;
			%mainIsBrick = 1;

		case "Vehicle" or "FlyingWheeledVehicle":
			%mainObj = %this;
			%mainClient = 0;
			%mainIsVehicle = 1;
	}

	switch$(%targetClass = %target.getClassName())
	{
		case "Player" or "AIPlayer":
			%targetObj = %target;
			%targetClient = %target.client;

		case "Projectile":
			%targProjectile = %target;
			%targetObj = %target.sourceObject;
			%targetClient = %target.sourceClient;
			if(!isObject(%targetClient))
				%targetClient = %target.client;

		case "GameConnection":
			%targetObj = %target.player;
			%targetClient = %target;

		case "fxDTSBrick":
			%targetObj = %target;
			%targetClient = %target.client;
			%targetIsBrick = 1;

		case "WheeledVehicle" or "FlyingWheeledVehicle":
			%targetObj = %target;
			%targetIsVehicle = 1;
	}

	if(!isObject(%mainObj) || !isObject(%targetObj))
	{
		return -1;
	}
	

	if(isObject(%mini1 = %mainClient.minigame) && isObject(%mini2 = %targetClient.minigame) && nameToID(%mini1) == nameToID(%mini2) && !%targetIsBrick && !%mainIsBrick)
	{
		%team1 = %mainClient.team;
		%team2 = %targetClient.team;
		if(isObject(%team1) && isObject(%team2))
		{
			if(!%mini.teamFriendlyFire && %team1 == %team2 && !%mainClient.friendlyFire && !%mainObj.friendlyFire && %mainClient != %targetClient)
				return 0;

			return 1;
		}

		return 1;
	}

	if((%targetObj.getType() & $TypeMasks::VehicleObjectType) && getMinigameFromObject(%targetObj) == %mini1 && isObject(%mainClient))
	{
		%team1 = 0;
		%team2 = 0;

		if(isObject(%driver = %targetObj.getMountNodeObject(0)) && isObject(%driverClient = %driver.client))
			%team1 = %driverClient.team;

		%team2 = %mainClient.team;

		if(%team1 != %team2 || !isObject(%team1))
			return 1;

		return 0;
	}

	if((%mainObj.getType() & $TypeMasks::VehicleObjectType) && getMinigameFromObject(%mainObj) == %mini2 && isObject(%targetClient))
	{
		%team1 = 0;
		%team2 = 0;

		if(isObject(%driver = %mainObj.getMountNodeObject(0)) && isObject(%driverClient = %driver.client))
			%team1 = %driverClient.team;

		%team2 = %targetClient.team;

		if(%team1 != %team2 || !isObject(%team1))
			return 1;

		return 0;
	}

	if(%mainClient.override)
		return 1;

	return -1;
}

///////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////

$Server::LastAttackTime = 3000;
function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
{
	if(%client.deathDebug)
		talk(%client.getPlayerName() @ " -> DeathID " @ %damageType @ "(" @ $DamageType_Array[%damageType] @ ")");

	if (%sourceObject.sourceObject.isBot)
	{
		%sourceClientIsBot = 1;
		%sourceClient = %sourceObject.sourceObject;
	}
	%player = %client.Player;
	if (isObject(%player))
	{
		%player.setShapeName("", 8564862);
		if (isObject(%player.tempBrick))
		{
			%player.tempBrick.delete();
			%player.tempBrick = 0;
		}
		%player.client = 0;
	}
	else
	{
		warn("WARNING: No player object in GameConnection::onDeath() for client \'" @ %client @ "\'");
	}
	if (isObject(%client.Camera) && isObject(%client.Player))
	{
		if (%client.getControlObject() == %client.Camera && %client.Camera.getControlObject() > 0.0)
		{
			%client.Camera.setControlObject(%client.dummyCamera);
		}
		else
		{
			%client.Camera.setMode("Corpse", %client.Player);
			%client.setControlObject(%client.Camera);
			%client.Camera.setControlObject(0);
		}
	}

	%colorStr = (%client.nameColor !$= "" ? "<color:" @ %client.nameColor @ ">" : "");
	if ($Damage::Direct[%damageType] != 1.0)
	{
		if (getSimTime() - %player.lastDirectDamageTime < 100.0)
		{
			if (%player.lastDirectDamageType !$= "")
			{
				%damageType = %player.lastDirectDamageType;
			}
		}
	}
	if (%damageType == $DamageType::Impact)
	{
		if (isObject(%player.lastPusher))
		{
			if (getSimTime() - %player.lastPushTime <= 1000.0)
			{
				%sourceClient = %player.lastPusher;
			}
		}
	}

	%message = "%2 killed %1";
	if (%sourceClient == %client || %sourceClient == 0)
	{
		%message = getTaggedString($DeathMessage_Suicide[%damageType]);
	}
	else
	{
		%message = getTaggedString($DeathMessage_Murder[%damageType]);
	}
	%yourDeathMessage = %message;

	%clientName = %client.getPlayerName();
	if (isObject(%sourceClient))
	{
		%sourceColorStr = (%sourceClient.nameColor !$= "" ? "<color:" @ %sourceClient.nameColor @ ">" : "");
		%sourceClientName = %sourceClient.getPlayerName();
	}
	else
	{
		if (isObject(%sourceObject.sourceObject) && %sourceObject.sourceObject.getClassName() $= "AIPlayer")
		{
			%sourceClientName = %sourceObject.sourceObject.name;
		}
		else
		{
			%sourceClientName = "";
		}
	}
	%sourceClientName2 = %sourceColorStr @ %sourceClientName;

	%client.Player = 0;

	%lastAttackTime = getSimTime() - %player.lastDamageTime;
	if(%lastAttackTime < 7000)
	{
		%lastType = %player.lastAssistDamageType;
		%otherAttacker = %player.lastAssistAttacker;
		%suicideBitmap = $DamageType::SuicideBitmap[%lastType];

		if(%sourceClient == %client && isObject(%player.lastAttacker))
		{
			%sourceClient = %player.lastAttacker;
			if(%lastAttackTime < $Server::LastAttackTime && (%damageType == $DamageType::Suicide || %damageType == $DamageType::Lava || %damageType == $DamageType::Impact || %damageType == $DamageType::Fall || %damageType == $DamageType::AFK))
			{
				%lastAttackFinish = 1;
				if(%suicideBitmap $= "")
					%suicideBitmap = $DamageType::SuicideBitmap[$DamageType::Suicide];

				%message = strReplace(getTaggedString($Server::DeathFinishMsg[%damageType, getRandom(1, $Server::DeathFinishMsgCount[%damageType])]), "%3", "<bitmap:" @ %suicideBitmap @ ">");
			}

			%yourDeathMessage = %message;
			%sourceColorStr = (%sourceClient.nameColor !$= "" ? "<color:" @ %sourceClient.nameColor @ ">" : "");
			%sourceClientName = %sourceClient.getPlayerName();
			%sourceClientName2 = %sourceClientName;
		}
		else if(isObject(%sourceClient) && %sourceClient != %otherAttacker && %client != %sourceClient && isObject(%otherAttacker))
		{
			%isOtherAttackerDeath = 1;
			%otherAttacker.incScore(%client.miniGame.Points_KillPlayer);
			%otherSourceColorStr = (%otherAttacker.nameColor !$= "" ? "<color:" @ %otherAttacker.nameColor @ ">" : "");
			%otherSourceClientName = %otherAttacker.getPlayerName();
			%sourceClientName2 = "\c0" @ %sourceColorStr @ %sourceClientName @ " \c0+ " @ %otherSourceColorStr @ %otherSourceClientName;
			%yourDeathMessage = getTaggedString($DeathMessage_Murder[%damageType]);
			%message = strReplace(%message, "%2", "\c3%2 \c0+ \c3%3\c0");
		}
		else if(!isObject(%sourceClient) && isObject(%lastAttacker))
		{
			%sourceClient = %lastAttacker;
			%sourceColorStr = (%sourceClient.nameColor !$= "" ? "<color:" @ %sourceClient.nameColor @ ">" : "");
			%sourceClientName = %sourceClient.getPlayerName();
			%sourceClientName2 = %sourceClientName;
			%message = getTaggedString($DeathMessage_Murder[%damageType]);
			%yourDeathMessage = %message;
		}
		else if(!isObject(%sourceClient) && isObject(%otherAttacker))
		{
			%sourceClient = %otherAttacker;
			%sourceColorStr = (%sourceClient.nameColor !$= "" ? "<color:" @ %sourceClient.nameColor @ ">" : "");
			%sourceClientName = %sourceClient.getPlayerName();
			%sourceClientName2 = %sourceClientName;
			%message = getTaggedString($DeathMessage_Murder[%damageType]);
			%yourDeathMessage = %message;
		}
	}

	if(%sourceClient.team == %client.team && isObject(%client.team) && %sourceClient != %client)
		%teamKillStr = " \c0(\c3Teamkill\c0)";

	if(isObject(%mg = %client.miniGame))
	{
		if(isfunction(%mg.class, updateLives))
			%mg.updateLives();

		if(isObject(%mode = %mg.mode) && isObject(%sourceClient) && %sourceClient.getClassName() $= "GameConnection")
		{
			if(isFunction(%mode.class, "onDeath"))
				%mode.onDeath(%mg, %client, %sourceClient, %damageType, %position);
		}

		if(isObject(%special = %mg.special) && isObject(%sourceClient) && %sourceClient.getClassName() $= "GameConnection")
		{
			if(isFunction(%special.class, "onDeath"))
				%special.onDeath(%mg, %client, %sourceClient, %damageType, %position);
		}
	}

	%client.killStreak = 0;
	if(%sourceClient != %client)
	{
		%sourceClient.killStreak++;
		if(%sourceClient.killStreak >= 3)
		{
			%sourceClient.tempKillDeathStr = " \c0(\c3Killstreak\c0)";
		}
	}

	if(isObject(%team = %client.team))
	{
		if(%team.getLiving() <= 0)
			%teamKillStr = %teamKillStr @ " \c0(\c3Team eliminated\c0)";
	}

	%message = %message @ %sourceClient.tempKillDeathStr @ %sourceClient.tempDeathStr[%client] @ %teamKillStr;
	%yourDeathMessage = %yourDeathMessage @ %sourceClient.tempKillDeathStr @ %sourceClient.tempDeathStr[%client] @ %teamKillStr;

	%sourceClient.tempDeathStr[%client] = "";
	%sourceClient.tempKillDeathStr = "";

	if(!%isOtherAttackerDeath && $Server::DeathMsgCount[%damageType] > 0 && !%lastAttackFinish)
	{
		%message = getTaggedString($Server::DeathMsg[%damageType, getRandom(1, $Server::DeathMsgCount[%damageType])]) @ %teamKillStr;
		%yourDeathMessage = getTaggedString($Server::DeathMsg[%damageType, getRandom(1, $Server::DeathMsgCount[%damageType])]) @ %teamKillStr;
	}

	if (isObject(%mg))
	{
		if (%sourceClient == %client)
		{
			%client.incScore(%client.miniGame.Points_KillSelf);
		}
		else
		{
			if (%sourceClient == 0)
			{
				%client.incScore(%client.miniGame.Points_Die);
			}
			else
			{
				if (!%sourceClientIsBot)
				{
					%sourceClient.incScore(%client.miniGame.Points_KillPlayer);
				}
				%client.incScore(%client.miniGame.Points_Die);
			}
		}
	}

	if (isObject(%mg))
	{
		%mg.messageAllExcept(%client, 'MsgClientKilled', addTaggedString(%message), %colorStr @ %client.getPlayerName(), %sourceColorStr @ %sourceClientName, %otherSourceColorStr @ %otherSourceClientName);
		//%yourDeathMessage = strReplace(%yourDeathMessage, "%1", %colorStr @ %client.getPlayerName());
		//%yourDeathMessage = strReplace(%yourDeathMessage, "%2", %sourceClientName2);
		messageClient(%client, 'MsgYourDeath', addTaggedString(%yourDeathMessage), %colorStr @ %client.getPlayerName(), %sourceClientName2, (%mg.WaveSpawn == 1 ? -1 : %mg.RespawnTime));
		if (%mg.RespawnTime < 0)
		{
			commandToClient(%client, 'centerPrint', "", 1);
		}
		%mg.checkLastManStanding();
	}
	else
	{
		//%yourDeathMessage = strReplace(%yourDeathMessage, "%1", %colorStr @ %client.getPlayerName());
		//%yourDeathMessage = strReplace(%yourDeathMessage, "%2", %sourceClientName2);
		messageAllExcept(%client, -1, 'MsgClientKilled', addTaggedString(%message), %colorStr @ %client.getPlayerName(), %colorStr @ %sourceClientName);
		messageClient(%client, 'MsgYourDeath', addTaggedString(%yourDeathMessage), %colorStr @ %client.getPlayerName(), %sourceClientName2, $Game::MinRespawnTime);
	}

	if(isObject(%client.minigame))
		%client.nameColor = "eaeaea";

	if(%sourceClient != %client)
		%sourceClient.roundKills++;
}

function initRandomKillMessages()
{
	$Server::DeathMsg[$DamageType::Suicide, 1] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0ended their life';
	$Server::DeathMsg[$DamageType::Suicide, 2] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0wanted to die';
	$Server::DeathMsg[$DamageType::Suicide, 3] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0sacrificed themselves';
	$Server::DeathMsg[$DamageType::Suicide, 4] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0suicided';
	$Server::DeathMsg[$DamageType::Suicide, 5] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0was shreked';
	$Server::DeathMsgCount[$DamageType::Suicide] = 5;

	$Server::DeathFinishMsg[$DamageType::Suicide, 1] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0before ending life';
	$Server::DeathFinishMsg[$DamageType::Suicide, 2] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0before wanting to die';
	$Server::DeathFinishMsg[$DamageType::Suicide, 3] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0before suiciding';
	$Server::DeathFinishMsgCount[$DamageType::Suicide] = 3;

	$Server::DeathMsg[$DamageType::Lava, 1] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0couldn\'t swim';
	$Server::DeathMsg[$DamageType::Lava, 2] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0fed themselves to the sharks';
	$Server::DeathMsg[$DamageType::Lava, 3] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0drowned';
	$Server::DeathMsgCount[$DamageType::Lava] = 3;

	$Server::DeathFinishMsg[$DamageType::Lava, 1] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0into the liquid and couldn\'t swim';
	$Server::DeathFinishMsg[$DamageType::Lava, 2] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0into the liquid and fed them to the sharks';
	$Server::DeathFinishMsg[$DamageType::Lava, 3] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0into the liquid and drowned';
	$Server::DeathFinishMsgCount[$DamageType::Lava] = 3;

	$Server::DeathMsg[$DamageType::Impact, 1] = '<bitmap:base/client/ui/ci/splat> \c3%1 \c0hit something too hard';
	$Server::DeathMsg[$DamageType::Impact, 2] = '<bitmap:base/client/ui/ci/splat> \c3%1 \c0launched themselves';
	$Server::DeathMsg[$DamageType::Impact, 3] = '<bitmap:base/client/ui/ci/splat> \c3%1 \c0ejected';
	$Server::DeathMsgCount[$DamageType::Impact] = 3;

	$Server::DeathFinishMsg[$DamageType::Impact, 1] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0hit something too hard';
	$Server::DeathFinishMsg[$DamageType::Impact, 2] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0launched themselves';
	$Server::DeathFinishMsg[$DamageType::Impact, 3] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0ejected';
	$Server::DeathFinishMsgCount[$DamageType::Impact] = 3;

	$Server::DeathMsg[$DamageType::Fall, 1] = '<bitmap:base/client/ui/ci/crater> \c3%1 \c0couldn\'t find a parachute';
	$Server::DeathMsg[$DamageType::Fall, 2] = '<bitmap:base/client/ui/ci/crater> \c3%1 \c0hit the ground too hard';
	$Server::DeathMsg[$DamageType::Fall, 3] = '<bitmap:base/client/ui/ci/crater> \c3%1 \c0went too fast';
	$Server::DeathMsgCount[$DamageType::Fall] = 3;

	$Server::DeathFinishMsg[$DamageType::Fall, 1] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0and couldn\'t find a parachute';
	$Server::DeathFinishMsg[$DamageType::Fall, 2] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0before hitting the ground too hard';
	$Server::DeathFinishMsg[$DamageType::Fall, 3] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0before going too fast';
	$Server::DeathFinishMsgCount[$DamageType::Fall] = 3;

	$Server::DeathMsg[$DamageType::AFK, 1] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0was star-gazing';
	$Server::DeathMsg[$DamageType::AFK, 2] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0went idle';
	$Server::DeathMsg[$DamageType::AFK, 3] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0smashed their keyboard';
	$Server::DeathMsg[$DamageType::AFK, 4] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0had more important things to do';
	$Server::DeathMsg[$DamageType::AFK, 5] = '<bitmap:base/client/ui/ci/skull> \c3%1 \c0died from afk disease';
	$Server::DeathMsgCount[$DamageType::AFK] = 5;

	$Server::DeathFinishMsg[$DamageType::AFK, 1] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0while they were star-gazing';
	$Server::DeathFinishMsg[$DamageType::AFK, 2] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0when they went idle';
	$Server::DeathFinishMsg[$DamageType::AFK, 3] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0when smashing their keyboard';
	$Server::DeathFinishMsg[$DamageType::AFK, 4] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0while having more important things to do';
	$Server::DeathFinishMsg[$DamageType::AFK, 5] = '\c3%2 \c0finished \c3%1 \c0with \c0%3 \c0before dying from afk disease';
	$Server::DeathFinishMsgCount[$DamageType::AFK] = 5;
}
schedule(100, 0, initRandomKillMessages);

if(isPackage("CustomMinigameCore"))
	deactivatePackage("CustomMinigameCore");

package CustomMinigameCore
{
	function servercmdupdatebodycolors(%client, %headColor, %hatColor, %accentColor, %packColor, %secondPackColor, %chestColor, %hipColor, %LLegColor, %RLegColor, %LArmColor, %RArmColor, %LHandColor, %RHandColor, %decalName, %faceName)
	{
		Parent::servercmdupdatebodycolors(%client, %headColor, %hatColor, %accentColor, %packColor, %secondPackColor, %chestColor, %hipColor, %LLegColor, %RLegColor, %LArmColor, %RArmColor, %LHandColor, %RHandColor, %decalName, %faceName);
		%currTime = getSimTime();
		if(%currTime - %client.lastUpdateBodyColorsTime < 1000)
			return;

		if(isObject(%minigame = %client.minigame) && %minigame.isCustomMini)
		{
			//edit for all body stuff later
			if(isObject(%teams = %minigame.Teams) && isObject(%team = %client.team) && %minigame.lockedAvatarShirt)
			{
				%client.oldchestcolor = %client.chestColor;
				%client.chestColor = %team.color;
				%client.applyBodyColors();
			}
		}
	}

	function GameConnection::applyBodyParts(%client)
	{
		// if(isObject(%mini = %client.minigame) && isObject(%teams = %minigame.Teams) && isObject(%team = %client.getTeam()))
		// {
		//
		// }

		return Parent::applyBodyParts(%client);
	}

	function GameConnection::applyBodyColors(%client)
	{
		if(isObject(%minigame = %client.minigame) && isObject(%teams = %minigame.Teams) && isObject(%team = %client.team))
		{
			if(%minigame.lockedAvatarShirt)
			{
				if(%client.oldchestcolor $= "")
					%client.oldchestcolor = %client.chestColor;

				%client.chestColor = %team.color;		
			}
		}

		return Parent::applyBodyColors(%client);
	}

	function Observer::onTrigger(%this, %obj, %trigger, %state)
	{
		%client = %obj.getControllingClient();
		if(%trigger == 0.0 && isObject(%mini = %client.minigame) && %mini.WaveSpawn)
			return;

		Parent::onTrigger(%this, %obj, %trigger, %state);
	}

	function MiniGameSO::pickSpawnPoint(%mini, %client)
	{
		if(isObject(%mode = %mini.mode))
		{
			if(isFunction(%mode.class, "pickSpawnPoint"))
				%pos = %mode.pickSpawnPoint(%mini, %client);

			if(%pos !$= "" && %pos !$= "0 0 0")
				return %pos;
		}

		return Parent::pickSpawnPoint(%mini, %client);
	}

	function serverCmdJoinMiniGame(%client, %miniGameID)
	{
		if (%client.currentPhase < 3)
			return;

		if (!isObject(%miniGameID))
			return;

		if (%miniGameID.class !$= "MiniGameSO" && %minigameID.class !$= "CustomMinigameSO")
			return;

		if (%miniGameID.InviteOnly)
		{
			messageClient(%client, '', 'That mini-game is invite-only.');
			return;
		}

		if (%miniGameID.isMember(%client))
		{
			messageClient(%client, '', 'You\'re already in that mini-game.');
			return;
		}

		if (getSimTime() - %client.miniGameJoinTime < $Game::MiniGameJoinTime)
		{
			messageClient(%client, '', 'You must wait %1 seconds before joining another minigame.', mCeil($Game::MiniGameJoinTime / 1000 - (getSimTime() - %client.miniGameJoinTime) / 1000) + 1);
			return;
		}

		if (isObject(%client.miniGame) && nameToID(%client.miniGame) == nameToID($DefaultMiniGame))
		{
			if (!%client.isAdmin && !%client.leaveOverride)
			{
				commandToClient(%client, 'messageBoxOK', "Minigame", "You can\'t leave the default minigame");
				return;
			}
		}

		%client.miniGameJoinTime = getSimTime();
		if (isObject(%client.miniGame))
			%client.miniGame.removeMember(%client);

		%miniGameID.addMember(%client);
	}

	function GameConnection::onDeath(%this, %killerObj, %killerClient, %damageType, %position)
	{
		if(isObject(%minigame = %this.minigame) && !%minigame.noSlowmo && %minigame.lastResetTime != %minigame.timescaleResetTime && %minigame.respawnTime == -1)
		{
			if(%minigame.doTimeScale && %minigame.isCustomMini && !isEventPending($TimescaleSch))
				if((%minigame.getLiving() <= 1 || (isObject(%teams = %minigame.teams) && %minigame.teams.getCount() > 0 && %minigame.getTeamLiving() == 1)) && !%minigame.isInTimescale)
				{
					for(%i = 0; %i < %minigame.numMembers; %i++)
					{
						%cl = %minigame.member[%i];
						if(isObject(%cl) && %cl.getClassName() $= "GameConnection")
							if(!isObject(%pl = %cl.player) && isObject(%cam = %cl.camera))
								%cam.setMode("Corpse", %this.player);
							else if(isObject(%pl) && %pl.getState() !$= "dead")
								%winner = %pl;
					}

					%minigame.timescaleResetTime = %minigame.lastResetTime;
					%oldTimescale = getTimescale();
					setTimescale(0.2);
					%timescale = 1;
				}
		}

		Parent::onDeath(%this, %killerObj, %killerClient, %damageType, %position);

		if(isObject(%minigame))
			%minigame.updateLives();

		if(%timescale)
		{
			cancel($TimescaleSch);
			$TimescaleSch = %minigame.schedule(1500 * getTimescale(), setTimescale, %oldTimescale, %winner);
		}

		if(!isObject(%winner) && isObject(%cam = %this.camera) && isObject(%killerPlayer = %killerClient.player))
			%cam.schedule(500, setMode, "Corpse", %killerPlayer);
	}

	function serverCmdResetMiniGame(%client)
	{
		if(!isObject(%client))
			return;

		%mg = %client.miniGame;
		if (!isObject(%mg))
			return;

		if(!%mg.isCustomMini)
		{
			Parent::serverCmdResetMiniGame(%client);
			return;
		}

		if(($DefaultMinigame == %mg && %client.isAdmin) || %mg.owner == %client)
		{
			%mg.messageAll('', '\c3%1 \c6has reset the minigame.', %client.getPlayerName());
			%mg.Reset(%client);
		}
	}

	function GameConnection::SpawnPlayer(%client)
	{
		if(isObject(%mini = %client.minigame))
			%mini.onPreSpawn(%client);

		%r = Parent::SpawnPlayer(%client);
		%client.SpawnTime = getSimTime();

		if(isObject(%mini))
			%mini.onSpawn(%client);

		return %r;
	}

	function serverCmdTeamMessageSent(%client, %message)
	{
		serverCmdStopTalking(%client);

		if(!isObject(%team = %client.team))
		{
			%client.chatMessage("\c5Sorry, would make sense to chat like this if you were on a team.");
			return;
		}

		%message = stripMLControlChars(trim(%message));
		%length = strLen(%message);
		if(!%length)
			return;

		%time = getSimTime();

		if(!%client.isSpamming)
		{
			//did they repeat the same message recently?
			if(%message $= %client.lastMsg && %time - %client.lastMsgTime < $SPAM_PROTECTION_PERIOD)
			{
				messageClient(%client, '', "\c5Do not repeat yourself.");
				if(!%client.isAdmin)
				{

					%client.isSpamming = true;
					%client.spamProtectStart = %time;
					%client.schedule($SPAM_PENALTY_PERIOD, spamReset);
				}
			}

			//are they sending messages too quickly?
			if(!%client.isAdmin)
			{
				if(%client.spamMessageCount >= $SPAM_MESSAGE_THRESHOLD)
				{
					%client.isSpamming = true;
					%client.spamProtectStart = %time;
					%client.schedule($SPAM_PENALTY_PERIOD, spamReset);
				}
				else
				{
					%client.spamMessageCount ++;
					%client.schedule($SPAM_PROTECTION_PERIOD, spamMessageTimeout);
				}
			}
		}

		//tell them they're spamming and block the message
		if(%client.isSpamming)
		{
			spamAlert(%client);
			return;
		}

		//eTard Filter, which I hate, but have to include
		if($Pref::Server::ETardFilter)
		{
			if(!chatFilter(%client, %message, $Pref::Server::ETardList, '\c5This is a civilized game.  Please use full words.'))
				return;
		}

		//URLs
		// for(%i = getWordCount(%message) - 1; %i >= 0; %i --)
		// {
		// 	%word = getWord(%message, %i);
		// 	%pos = strPos(%word, "://") + 3;
		// 	%pro = getSubStr(%word, 0, %pos);
		// 	%url = getSubStr(%word, %pos, strLen(%word));

		// 	if((%pro $= "http://" || %pro $= "https://" || %pro $= "ftp://") &&
		// 		strPos(%url, ":") == -1)
		// 	{
		// 		%word = "<sPush><a:" @ %url @ ">" @ %url @ "</a><sPop>";
		// 		%message = setWord(%message, %i, %word);
		// 	}
		// }

		%mini = getMinigameFromObject(%client);
		%all  = '\c7[\c6%5%6\c7] \c7%1\c3%5%2\c7%3\c4: %4';

		%color = %client.team.colorHexStr;

		%name = %client.getPlayerName();
		%pre  = %client.clanPrefix;
		%suf  = %client.clanSuffix;

		for(%i = 0; %i < ClientGroup.getCount(); %i++)
		{
			%selectedClient = ClientGroup.getObject(%i);
			if(%client.team == %selectedClient.team)
				commandToClient(%selectedClient, 'chatMessage', %client, '', '', %all, %pre, %name, %suf, %message, %color, %team.name, "<color:ffffff>");
		}

		echo("(TEAM) " @ %client.getSimpleName() @ ": " @ %message);

		%client.lastMsg = %message;
		%client.lastMsgTime = %time;

		// if(isObject(%client.player))
		// {
		// 	%client.player.playThread(3, "talk");
		// 	%client.player.schedule(%length * 50, playThread, 3, "root");
		// }
	}

	function Armor::Damage(%data, %obj, %sourceObject, %position, %damage, %damageType)
	{
		%client = %obj.client;
		%sourceClient = %sourceObject.client;
		if(!isObject(%sourceClient))
			%sourceClient = %sourceObject.sourceClient;

		if(isObject(%sourceClient) && %sourceClient != %client)
		{
			if(isObject(%sourceClient.player.lastTF2Healer.client) && %sourceClient.player.beingHealed)
			{
				%obj.lastAssistAttacker = %sourceClient.player.lastTF2Healer.client;
				%obj.lastAttacker = %sourceClient;
			}
			else if(!isObject(%obj.lastAssistAttacker))
			{
				%obj.lastAssistAttacker = %sourceClient;
				%obj.lastAttacker = %sourceClient;
			}
			else if(%obj.lastAttacker != %sourceClient)
			{
				%obj.lastAssistAttacker = %obj.lastAttacker;
				%obj.lastAttacker = %sourceClient;
			}
			%obj.lastAssistDamageType = %damageType;
			%obj.lastAssistDamageTime = getSimTime();
		}
		
		%obj.lastDamageTime = getSimTime();

		Parent::Damage(%data, %obj, %sourceObject, %position, %damage, %damageType);
	}
};
activatePackage(CustomMinigameCore);

function SimObject::setScopeToAll(%this, %bool)
{
	if(!isFunction("NetObject", "setNetFlag"))
	{
		if(%this.getClassName() $= "Player")
		{
			if(isObject(%client = %this.client))
			{
				if(!%bool)
				{
					%this.setShapeNameDistance(0);
					%this.hideNode("ALL");
				}
				else
				{
					%this.unHideNode("headSkin");
					%client.applyBodyParts();
					%client.applyBodyColors();
					%this.setShapeNameDistance(100);
				}
			}
		}

		return;
	}

	%this.isInvisible = !%bool;
	%this.setNetFlag(6, !%bool);
	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		if(%client == %this.client)
		{
			if(%bool)
				%this.scopeToClient(%client);

			continue;
		}

		if(%bool)
			%this.scopeToClient(%client);
		else
		{
			if(!isObject(%client.team) || (isObject(%client.team) && %this.teamID != %client.teamID))
				%this.clearScopeToClient(%client);
		}
	}
}

function getWordIndexInString(%string, %needle)
{
	%count = getWordCount(%string);
	for(%i = 0; %i < %count; %i++)
	{
		%word = getWord(%string, %i);
		if(%word $= %needle)
			return %i;
	}

	return -1;
}

function shuffleMusicCache()
{
	%cache = MusicDataCache.getID();
	%count = %cache.itemCount;
	for(%i = 0; %i < %count; %i++)
	{
		%temp = %cache.item[%i];
		%r = getRandom(0,%count-1);
		%cache.item[%i] = %cache.item[%r];
		%cache.item[%r] = %temp;
	}
}

function findMusicByName(%name, %val)
{
	if(isObject(%name)) return %name.getName();
	if(!isObject(MusicDataCache))
		new ScriptObject(MusicDataCache)
		{
			itemCount = 0;
			lastDatablockCount = DatablockGroup.getCount();
		};

	//Should automatically create the lookup if you:
	// + Added new weapons
	// + Started the server
	if(MusicDataCache.itemCount <= 0 || MusicDataCache.lastDatablockCount != DatablockGroup.getCount() || %val) //We don't need to cause lag everytime we try to find an item
	{
		MusicDataCache.lastDatablockCount = DatablockGroup.getCount();
		MusicDataCache.itemCount = 0;
		for(%i=0;%i<DatablockGroup.getCount();%i++)
		{
			%obj = DatablockGroup.getObject(%i);
			if(%obj.getClassName() $= "AudioProfile" && strLen(%obj.uiName) > 0 && %obj.description $= "AudioMusicLooping3d")
			{
				MusicDataCache.item[MusicDataCache.itemCount] = %obj;
				MusicDataCache.itemCount++;
			}
		}

		echo("findMusicByName() - Created lookup database.");
	}

	//First let's see if we find something to be exact
	if(MusicDataCache.itemCount > 0)
	{
		%result["string"] = 0;
		%result["id"] = 0; //If this is found we are definitely giving it
		%result["string", "pos"] = 9999;
		for(%a = 0; %a < MusicDataCache.itemCount; %a++)
		{
			%objA = MusicDataCache.item[%a];
			if(%objA.getClassName() $= "AudioProfile")
				if(%objA.uiName $= %name || %objA.getName() $= %name)
				{
					%result["id"] = 1;
					%result["id", "item"] = %objA;
				}
				else
				{
					%pos = striPos(%objA.uiName, %name);
					if(striPos(%objA.uiName, %name) >= 0 && %pos < %result["string", "pos"])
					{
						%result["string"] = 1;
						%result["string", "item"] = %objA;
						%result["string", "pos"] = %pos;
					}						
				}
		}

		if(%result["id"] && isObject(%result["id", "item"])) //This should most likely say yes
			return %result["id", "item"].getName();

		if(%result["string"] && isObject(%result["string", "item"]))
			return %result["string", "item"].getName();
	}
	
	return -1;
}
schedule(100, 0, findMusicByName);

function fxDTSBrick::getTopPosition(%this)
{
	%pos = %this.getPosition();
	return getWord(%pos, 0) SPC getWord(%pos, 1) SPC getWord(%pos, 2) + 0.1 * %this.getdatablock().bricksizez;
}

function serverCmdListMusic(%client)
{
	%client.chatMessage("\c6List of songs:");
	for(%i = 0; %i < MusicDataCache.itemCount; %i++)
	{
		if(MusicDataCache.item[%i].uiName !$= "")
			%client.chatMessage(" \c6+ \c3" @ MusicDataCache.item[%i].uiName);
	}
}

function serverCmdSpawn(%client, %a1, %a2, %a3, %a4)
{
	if(!%client.isAdmin)
		return;

	for(%i = 1; %i <= 4; %i++)
	{
		if(%a[%i] !$= "")
		{
			if(%name $= "")
				%name = %a[%i];
			else
				%name = %name SPC %a[%i];
		}
	}

	if(!isObject(%target = findClientByName(%name)))
	{
		%client.chatMessage("Invalid target!");
		return;
	}

	if(!isObject(%targetplayer = %target.player))
	{
		%client.chatMessage("Did you mean /Respawn " @ %name @ "? (Player is dead)");
		return;
	}

	%targetplayer.setTransform($DefaultMinigame.pickSpawnPoint());

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);
		if(%cl.isAdmin)
			%cl.chatMessage("\c3" @ %client.getPlayerName() @ " \c6has put \c3" @ %target.getPlayerName() @ "\c6 into a spawn location.");
	}

	echo(" >>> " @ %client.getPlayerName() @ " has put " @ %target.getPlayerName() @ " into a spawn location.");
}

function serverCmdRespawn(%client, %a1, %a2, %a3, %a4)
{
	if(!%client.isAdmin)
		return;
	
	for(%i = 1; %i <= 4; %i++)
	{
		if(%a[%i] !$= "")
		{
			if(%name $= "")
				%name = %a[%i];
			else
				%name = %name SPC %a[%i];
		}
	}

	if(!isObject(%target = findClientByName(%name)))
	{
		%client.chatMessage("Invalid target!");
		return;
	}

	if(isObject(%target.player) && %target.player.isEnabled())
	{
		%client.chatMessage("Did you mean /Spawn " @ %name @ "? (Player is alive)");
		return;
	}

	%target.spawnPlayer();
	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);
		if(%cl.isAdmin)
			%cl.chatMessage("\c3" @ %client.getPlayerName() @ " \c6has respawned \c3" @ %target.getPlayerName() @ "\c6.");
	}

	echo(" >>> " @ %client.getPlayerName() @ " has respawned " @ %target.getPlayerName() @ ".");
}

function findclosestcolor(%x)
{
	%x = getColorF(%x);
	for(%a=0; %a<64; %a++)
	{
		%match = mabs(getword(getcoloridtable(%a),0) - getword(%x,0)) + 
			mabs(getword(getcoloridtable(%a),1) - getword(%x,1)) + mabs(getword(getcoloridtable(%a),2) - getword(%x,2))
			+ mabs(getword(getcoloridtable(%a),3) - getword(%x,3));

		if(%match < %bestmatch || %bestmatch $= "")
		{
			%bestmatch = %match;
			%bestid = %a;
		}
	}
	return %bestid;
}

//////////////////////////////////////////////////////////////////

if(!isPackage("CustomMinigameCore_LoadOnce"))
	exec("./mainLoadOnly.cs");

exec("./specials.cs");
exec("./games.cs");

exec("./minigame - custom.cs");
exec("./minigameTeams.cs");
exec("./minigame.cs");