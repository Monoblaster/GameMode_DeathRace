function MinigameSO::DR_Loop(%mini)
{
	cancel(%mini.DRSch);
	if(%mini.numMembers <= 0)
	{
		%mini.DRSch = %mini.schedule($Server::DeathRace_Loop, "DR_Loop");
		return;
	}

	%loopTime = $Server::DeathRace_Loop;
	%shapeNameDistance = $Server::DeathRace_ShapeDist;
	%showScore = !%mini.NoDRDataHUD_Score;
	%showHealth = !%mini.NoDRDataHUD_Health;

	%vehicleHealth = !%mini.NoDRDataHUD_VehicleHealth;
	%vehicleSong = !%mini.NoDRDataHUD_VehicleSong;

	%curTime = $Sim::Time;

	if(%mini.time > 0)
	{
		%time = (%mini.time * 60000 - (getSimTime() - %mini.lastResetTime)) / 1000;

		%timeLeft = mCeil(%time);
		if(%timeLeft > 0)
		{
			%timeString = getTimeString(%timeLeft);

			if(%timeLeft <= 60)
			{
				if(%timeLeft == 60)
				{
					%mini.blinkTime = 1;
					%mini.blinkTimeCol = "\c3";
					%mini.blinkTimeSec = 1;
				}
				else if(%timeLeft % 2 == 0 && %timeLeft > 10 && %timeLeft <= 30)
				{
					%mini.blinkTime = 1;
					%mini.blinkTimeCol = "\c0";
					%mini.blinkTimeSec = 1;
				}
				else if(%timeLeft == 10)
				{
					%mini.blinkTime = 10;
					%mini.blinkTimeCol = "\c0";
					%mini.blinkTimeSec = 1;
				}
			}

			if(%timeLeft == 120 && %curTime - %mini.lastPlayTimeout > 3)
			{
				%mini.lastPlayTimeout = %curTime;
				if(isFunction("MinigameSO", "playSound"))
					%mini.playSound(TimeRunningOutSound);
				else if(isFunction("MinigameSO", "play2D"))
					%mini.play2D(TimeRunningOutSound);
			}
			
			if(%timeLeft % 60 == 0 && %timeLeft > 60)
			{
				%mini.blinkTime = 1;
				%mini.blinkTimeCol = "\c4";
				%mini.blinkTimeSec = 0.5;
			}

			%timeStr = "Time";
			if(%mini.blinkTime > 0 && %curTime - %mini.lastBlinkTime <= %mini.blinkTimeSec)
				%timeString = %mini.blinkTimeCol @ %timeString;
			else if(%mini.blinkTime > 0 && %curTime - %mini.lastBlinkTime > %mini.blinkTimeSec)
			{
				%mini.blinkTime--;
				%mini.lastBlinkTime = %curTime;
				%mini.blinkTimeSecCool = %curTime;
				if(%mini.blinkTime > 0)
					%timeString = %mini.blinkTimeCol @ %timeString;
			}
		}
	}

	%isReset = isEventPending(%mini.resetSchedule);
	%isCustom = %mini.isCustomMini;
	%font = $Pref::Server::DRFont;
	%deathrace_maxtime = %mini.deathRaceMaxTime;
	%members = %mini.numMembers;
	%deathrace_lastReset = %mini.lastDeathRaceReset;
	%deathrace_vehicleExplodeTime = $Pref::Server::VehicleLimitTimeDeath;
	%deathrace_time = %mini.deathRaceDatatime;
	%avoidCheck = %mini.avoidVehicleDeathCheck;

	if(%deathrace_maxtime > 0 && getSimTime() - %deathrace_lastReset < %deathrace_maxtime * 1000)
	{
		%isStarting = 1;
		%timeStr = "Starting in";
		%timeString = getTimeString(mCeil(%deathrace_maxtime - ((getSimTime() - %deathrace_lastReset) / 1000)));
	}

	for(%i = 0; %i < %members; %i++)
	{
		%client = %mini.member[%i];

		if(isObject(%client))
		{
			if(isObject(%player = %client.player) && %player.getState() !$= "dead" && !%isReset)
			{
				%lastDamageTime = getSimTime() - %player.lastDamageTime;

				if(%lastDamageTime > 7000 && %curTime - %player.lastMainTick_Health > 0.25 && %player.getHealth() < %player.getMaxHealth())
				{
					%player.lastMainTick_Health = %curTime;

					%heal = %lastDamageTime / 7000;
					%player.addHealth(0.058 * %heal);
				}

				if(isObject(%vehicle = %player.getObjectMount()) && (%vehicleDmgLvl = %vehicle.getDamageLevel()) < (%vehicleMaxHp = %vehicle.getDatablock().maxDamage))
				{
					if(!isEventPending(%vehicle.teamCheckSch) && %vehicle.teamingHP > 0)
						%vehicle.teamingHP = mClampF(%vehicle.teamingHP - 0.2, 0, 100);

					%vehicleData = %vehicle.getDatablock();

					if(%player.maxVehicleLimit > 0 && %player.vehicleLimitTime < %player.maxVehicleLimit)
						%player.vehicleLimitTime += 0.01;

					if(isObject(%handler = %vehicle.stereoHandler))
					{
						if(isObject(%audioEmitter = %handler.audioEmitter) && isObject(%vehicleSong = %audioEmitter.profile))
						{
							%vehicleSong = %vehicleSong.uiName;
						}
						else
						{
							%vehicleSong = "NONE";
						}
					}
					else
						%vehicleSong = "NONE";

					%vehiclePrintSong = "\c6Vehicle song: \c4" @ %vehicleSong;
					%vehiclePrintHealth = "\c6Vehicle: \c3" @ mCeil((%vehicleMaxHp - %vehicleDmgLvl) / %vehicleMaxHp * 100) @ "\c6%";

					%mountCount = 0;
					%mountPoints = %vehicleData.numMountPoints;
					for(%j = 0; %j < %mountPoints; %j++)
					{
						if(isObject(%vehiclePassenger[%i, %j] = %vehicle.getMountNodeObject(%j)))
						{
							%mountCount++;
						}
					}

					if(%curTime - %vehicle.lastDeathTick >= 1 && (isObject(%victim = %vehiclePassenger[%i, 0]) || isObject(%victim = %vehicle.getMountedObject(0))))
					{
						%vehicle.lastDeathTick = %curTime;
						%victim_vehicleTimeDeath = mFloor(%victim.vehicleLimitTimeDeath[%vehicle]);
						%victim_lastMainTick = %victim.lastMainTick_Vehicle;

						if(VectorLen(%vehicle.getVelocity()) < 4 && %mountCount > 0 && %deathrace_time <= 0 && !%client.ignoreVehicleAFK)
						{
							if(%deathrace_vehicleExplodeTime - %victim_vehicleTimeDeath > 0)
							{
								cancel(%vehicle.shapeNameSch);
								%vehicle.shapeNameSch = %vehicle.schedule(3000, setShapeName, "");
								%vehicle.setShapeName("'AFK', exploding in " @ (%deathrace_vehicleExplodeTime - %victim_vehicleTimeDeath) @ "s");
								%vehicle.setShapeNameDistance(%shapeNameDistance);
							}

							if(%victim_vehicleTimeDeath > %deathrace_vehicleExplodeTime)
							{
								%vehicle.damage(%vehicle, %vehicle.getPosition(), %vehicleMaxHp);
							}

							if(%curTime - %victim_lastMainTick >= 1)
							{
								%victim.vehicleLimitTimeDeath[%vehicle] += 1;
							}
						}
						else if(%victim.vehicleLimitTimeDeath[%vehicle] > 0 && %mountCount > 0 && %deathrace_time <= 0)
						{
							%vehicle.schedule(1, setShapeName, "");
							if(%curTime - %victim_lastMainTick >= 1)
							{
								%victim.vehicleLimitTimeDeath[%vehicle] -= 0.15;
							}
						}
					}
				}
				else if(!%avoidCheck)
				{
					if(%player.maxVehicleLimit > 0 && %curTime - %player.lastMainTick_Vehicle > 0.5 && %deathrace_time <= 0)
					{
						if(%curTime - %player.lastMainTick_Vehicle > 1)
						{
							%player.lastMainTick_Vehicle = %curTime;
							%player.vehicleLimitTime -= 1;
						}

						if(%player.vehicleLimitTime < 0)
						{
							%player.addHealth(-12);
						}
					}

					%vehicleLimitTime = %player.vehicleLimitTime;
					if(%vehicleLimitTime < 15 && %vehicleLimitTime > 0)
						%msg1 = "<just:center><font:" @ %font @ ":22>\c6You need to get back in your vehicle!\n\c6Time: \c3" @ getTimeString(mCeil(%vehicleLimitTime)) @ "";
					else if(%vehicleLimitTime <= 0)
						%msg1 = "<just:center><font:" @ %font @ ":22>\c6You need to get back in your vehicle!";
				}

				%hpStr = "\c6Health: \c3" @ mCeil(%player.getHealth() / %player.getMaxHealth() * 100) @ "\c6%";
				if(%timeString !$= "")
				{
					%mPrint = "<just:left>" @ %hpStr @ "<just:right>\c6" @ %timeStr @ ": " @ %timeString @ "  ";
				}
				else
				{
					%mPrint = "<just:left>" @ %hpStr;
				}

				if(%isStarting)
				{
					if(isObject(%vehicle))
					{
						%msg1 = "<font:" @ %font @ ":20>" @ %mPrint @ "\n<just:left>" @ %vehiclePrintHealth @ "<just:right>" @ %vehiclePrintSong @ "\n<just:left>\c6Score: \c3" @ %client.score;
					}
					else
					{
						%msg1 = "<font:" @ %font @ ":20>" @ %mPrint @ "\n<just:left>\c6Score: \c3" @ %client.score;
					}
				}
				else
				{
					if(isObject(%vehicle))
					{
						%msg0 = "<font:" @ %font @ ":20>" @ %mPrint @ "\n<just:left>" @ %vehiclePrintHealth @ "<just:right>" @ %vehiclePrintSong @ "\n<just:left>\c6Score: \c3" @ %client.score;
					}
					else
					{
						%msg0 = "<font:" @ %font @ ":20>" @ %mPrint @ "\n<just:left>\c6Score: \c3" @ %client.score;
					}
				}
			}
			else if(isObject(%player = %client.spyObj) && isObject(%spyClient = %player.client) && isObject(%minigame = %spyClient.minigame) && !%isReset) // Must be spying on someone
			{
				if(isObject(%vehicle = %player.getObjectMount()) && (%vehicleDmgLvl = %vehicle.getDamageLevel()) < (%vehicleMaxHp = %vehicle.getDatablock().maxDamage))
				{
					%vehicleData = %vehicle.getDatablock();

					if(isObject(%handler = %vehicle.stereoHandler))
					{
						if(isObject(%audioEmitter = %handler.audioEmitter) && isObject(%vehicleSong = %audioEmitter.profile))
						{
							%vehicleSong = %vehicleSong.uiName;
						}
						else
						{
							%vehicleSong = "NONE";
						}
					}
					else
						%vehicleSong = "NONE";

					%vehiclePrintSong = "\c6Vehicle song: \c4" @ %vehicleSong;
					%vehiclePrintHealth = "\c6Vehicle: \c3" @ mCeil((%vehicleMaxHp - %vehicleDmgLvl) / %vehicleMaxHp * 100) @ "\c6%";
				}

				%hpStr = "\c6Health: \c3" @ mCeil(%player.getHealth() / %player.getMaxHealth() * 100) @ "\c6%";
				if(%timeString !$= "")
				{
					%mPrint = "<just:left>" @ %hpStr @ "<just:right>\c6" @ %timeStr @ ": " @ %timeString @ "  ";
				}
				else
				{
					%mPrint = "<just:left>" @ %hpStr;
				}

				if(%isStarting)
				{
					if(isObject(%vehicle))
					{
						%msg1 = "<font:" @ %font @ ":20>" @ %mPrint @ "\n<just:left>" @ %vehiclePrintHealth @ "<just:right>" @ %vehiclePrintSong @ "\n<just:left>\c6Score: \c3" @ %spyClient.score;
					}
					else
					{
						%msg1 = "<font:" @ %font @ ":20>" @ %mPrint @ "\n<just:left>\c6Score: \c3" @ %spyClient.score;
					}
				}
				else
				{
					if(isObject(%vehicle))
					{
						%msg0 = "<font:" @ %font @ ":20>" @ %mPrint @ "\n<just:left>" @ %vehiclePrintHealth @ "<just:right>" @ %vehiclePrintSong @ "\n<just:left>\c6Score: \c3" @ %spyClient.score;
					}
					else
					{
						%msg0 = "<font:" @ %font @ ":20>" @ %mPrint @ "\n<just:left>\c6Score: \c3" @ %spyClient.score;
					}
				}
			}

			// if(%client.Shop_Client && %client.ShopPref_HUD == 1)
			// {
			// 	%client.DR_SendHUD(%isDead, %player.getHealth() SPC %player.getMaxHealth(), %vehicleDmgLvl SPC %vehicleMaxHp, %vehicleSong, %timeStr, %timeString, %client.score);
			// }
			// else
			if(%client.DeathRaceDataHUD)
			{
				if(%client.lastDRPrint !$= %msg0 || %curTime - %client.lastDRPrint > 2)
				{
					%client.lastDRPrintTime = %curTime;
					if(%client.lastDRPrint !$= "" || %msg0 !$= "")
						%client.centerPrint(%msg0, %loopTime / 100 + 2);

					%client.lastDRPrint = %msg0;
				}

				if(%client.lastDRBottomPrint !$= %msg1 || %curTime - %client.lastDRBottomPrintTime > 2)
				{
					%client.lastDRBottomPrintTime = %curTime;
					if(%client.lastDRBottomPrint !$= "" || %msg1 !$= "")
						%client.bottomPrint(%msg1, %loopTime / 100 + 2, 1);

					%client.lastDRBottomPrint = %msg1;
				}
			}

			%msg0 = "";
			%msg1 = "";
			%mPrint = "";
			%vehiclePrintSong = "";
			%vehiclePrintHealth = "";
		}
	}

	cancel(%mini.DRSch);
	%mini.DRSch = %mini.schedule(%loopTime, "DR_Loop");
}

package DeathRace_MinigameLoop
{
	function Armor::onMount(%this, %obj, %vehicle, %node)
	{
		Parent::onMount(%this, %obj, %vehicle, %node);
		if(isObject(%client = %obj.client) && isObject(%mini = %client.minigame) && %mini.isCustomMini && !isEventPending(%vehicle.teamCheckSch) && isObject(%spawnBrick = %vehicle.spawnBrick))
		{
			%vehicle.setNodeColor("ALL", %client.chestColor);
			%vehicle.DR_TeamCheck();
		}
	}

	function Armor::onUnMount(%this, %obj, %vehicle, %node)
	{
		Parent::onUnMount(%this, %obj, %vehicle, %node);
		if(isEventPending(%vehicle.teamCheckSch))
		{
			cancel(%vehicle.teamCheckSch);
			if(isObject(%spawnBrick = %vehicle.spawnBrick))
				%vehicle.setColor(%spawnBrick.getColorID());
		}
	}
};
activatePackage(DeathRace_MinigameLoop);

function WheeledVehicle::DR_TeamCheck(%vehicle)
{
	cancel(%vehicle.teamCheckSch);
	%mini = getMinigameFromObject(%vehicle);
	if(!isObject(%mini) || !%mini.isCustomMini)
		return;

	%vehicleData = %vehicle.getDatablock();
	%spawnBrick = %vehicle.spawnBrick;

	%mountCount = 0;
	%mountPoints = %vehicleData.numMountPoints;
	for(%j = 0; %j < %mountPoints; %j++)
	{
		if(isObject(%vehiclePassenger[%j] = %vehicle.getMountNodeObject(%j)))
		{
			%mountCount++;
		}
	}

	if(%mountCount <= 1 || !isObject(%client = %vehiclePassenger0.client))
	{
		return;
	}

	%isTeaming = 0;
	%teamingCount = 0;
	if(isObject(%mini.Teams) && %mini.Teams.getCount() > 0 && isObject(%curTeam = %client.team))
	{
		for(%j = 0; %j < %mountCount; %j++)
		{
			if(isObject(%vehiclePassenger[%j]))
				if(isObject(%vehicleObjClient = %vehiclePassenger[%j].client) && isObject(%vehicleObjMini = %vehicleObjClient.minigame))
					if(isObject(%vehicleObjTeam = %vehicleObjClient.team))
						if(%curTeam != %vehicleObjTeam && %vehiclePassenger[%j] != %vehiclePassenger0)
						{
							%isTeaming = 1;
							%teamingObjTeam[%teamingCount] = %vehicleObjClient.team;
							%teamingObj[%teamingCount] = %vehicleObjClient;
							%teamingCount++;
						}
		}
	}
	else
	{
		for(%j = 0; %j < %mountCount; %j++)
		{
			if(isObject(%vehiclePassenger[%j]))
				if(isObject(%vehicleObjClient = %vehiclePassenger[%j].client) && isObject(%vehicleObjMini = %vehicleObjClient.minigame))
					if(minigameCanDamage(%player, %vehicleObjClient) && %vehiclePassenger[%j] != %player)
					{
						%isTeaming = 1;
						%teamingObj[%teamingCount] = %vehicleObjClient;
						%teamingCount++;
					}
		}
	}

	//Blink car if teaming, then destroy it after a certain time
	if(%isTeaming && %mini.deathRaceData["time"] <= 0)
	{
		//Blink the car
		%vehicle.teamingHP = mClampF(%vehicle.teamingHP + 0.3, 0, 100);

		%maxBlinkTime = 1200;
		%blinkTime = mClampF(%maxBlinkTime - %vehicle.teamingHP * 20, 0, %maxBlinkTime);

		if(%blinkTime == 0)
		{

			%vehicle.damage(%vehicle, %vehicle.getPosition(), %vehicleData.maxDamage * getWord(%vehicle.getScale(), 2), $DamageType::Default);
		}
		else
		{
			%oldColor = %spawnBrick.getColorID();
			%blinkColor = "1 1 1 1";
			// if(isObject(%curTeam))
			// {
			// 	%blinkColor = %curTeam.colorID;
			// }
			// else
			// {
			// 	%blinkColor = findClosestColor(%client.chestColor);
			// }

			if(!isEventPending(%vehicle.blinkColorSch) && getSimTime() - %vehicle.lastBlinkTime > %vehicle.blinkTime && isObject(%spawnBrick) && %blinkColor != %oldColor)
			{
				%vehicle.lastBlinkTime = getSimTime();
				%vehicle.blinkTime = %blinkTime * 1.3;
				%vehicle.setNodeColor("ALL", %blinkColor);
				%vehicle.blinkColorSch = %spawnBrick.schedule(%blinkTime, "setColor", %spawnBrick.getColorID());
			}
		}
	}

	%vehicle.teamCheckSch = %vehicle.schedule(100, DR_TeamCheck);
}