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

	// if(%mini.time > 0)
	// {
	// 	%time = (%mini.time * 60000 - (getSimTime() - %mini.lastResetTime)) / 1000;

	// 	%timeLeft = mCeil(%time);
	// 	if(%timeLeft > 0)
	// 	{
	// 		%timeString = getTimeString(%timeLeft);

	// 		if(%timeLeft <= 60)
	// 		{
	// 			if(%timeLeft == 60)
	// 			{
	// 				%mini.blinkTime = 1;
	// 				%mini.blinkTimeCol = "\c3";
	// 				%mini.blinkTimeSec = 1;
	// 			}
	// 			else if(%timeLeft % 2 == 0 && %timeLeft > 10 && %timeLeft <= 30)
	// 			{
	// 				%mini.blinkTime = 1;
	// 				%mini.blinkTimeCol = "\c0";
	// 				%mini.blinkTimeSec = 1;
	// 			}
	// 			else if(%timeLeft == 10)
	// 			{
	// 				%mini.blinkTime = 10;
	// 				%mini.blinkTimeCol = "\c0";
	// 				%mini.blinkTimeSec = 1;
	// 			}
	// 		}

	// 		if(%timeLeft == 120 && %curTime - %mini.lastPlayTimeout > 3)
	// 		{
	// 			%mini.lastPlayTimeout = %curTime;
	// 			if(isFunction("MinigameSO", "playSound"))
	// 				%mini.playSound(TimeRunningOutSound);
	// 			else if(isFunction("MinigameSO", "play2D"))
	// 				%mini.play2D(TimeRunningOutSound);
	// 		}
			
	// 		if(%timeLeft % 60 == 0 && %timeLeft > 60)
	// 		{
	// 			%mini.blinkTime = 1;
	// 			%mini.blinkTimeCol = "\c4";
	// 			%mini.blinkTimeSec = 0.5;
	// 		}

	// 		%timeStr = "Time";
	// 		if(%mini.blinkTime > 0 && %curTime - %mini.lastBlinkTime <= %mini.blinkTimeSec)
	// 			%timeString = %mini.blinkTimeCol @ %timeString;
	// 		else if(%mini.blinkTime > 0 && %curTime - %mini.lastBlinkTime > %mini.blinkTimeSec)
	// 		{
	// 			%mini.blinkTime--;
	// 			%mini.lastBlinkTime = %curTime;
	// 			%mini.blinkTimeSecCool = %curTime;
	// 			if(%mini.blinkTime > 0)
	// 				%timeString = %mini.blinkTimeCol @ %timeString;
	// 		}
	// 	}
	// }

	// some local vars
	%isReset = isEventPending(%mini.resetSchedule);
	%isCustom = %mini.isCustomMini;
	%font = $Pref::Server::DRFont;
	%deathrace_maxtime = %mini.deathRaceMaxTime;
	%members = %mini.numMembers;
	%deathrace_lastReset = %mini.lastDeathRaceReset;
	%deathrace_vehicleExplodeTime = $Pref::Server::VehicleLimitTimeDeath;
	%deathrace_time = mCeil(%mini.deathRaceMaxTime - ((getSimTime() - %mini.lastDeathRaceReset) / 1000));
	%avoidCheck = %mini.avoidVehicleDeathCheck;

	// starting time
	if(%deathrace_time >= 0)
	{
		%isStarting = 1;
	}

	// loop through everyone
	for(%i = 0; %i < %members; %i++)
	{
		%client = %mini.member[%i];
		if(isObject(%client))
		{
			if(isObject(%player = %client.player) && %player.getState() !$= "dead" && !%isReset)
			{
				%hud = "";
				if(!%client.noHud)
				{
					%hud = %client.DR_hud;
				}

				// healing over time
				%lastDamageTime = getSimTime() - %player.lastDamageTime;

				if(%lastDamageTime > 7000 && %curTime - %player.lastMainTick_Health > 0.25 && %player.getHealth() < %player.getMaxHealth())
				{
					%player.lastMainTick_Health = %curTime;

					%heal = %lastDamageTime / 7000;
					%player.addHealth(0.058 * %heal);
				}

				//mounted vehicle and vehicle is not destroyed
				if(isObject(%vehicle = %player.getBaseMount()) && (%vehicleDmgLvl = %vehicle.getDamageLevel()) < (%vehicleMaxHp = %vehicle.getDatablock().maxDamage))
				{
					//return teamingHP to 0
					if(!isEventPending(%vehicle.teamCheckSch) && %vehicle.teamingHP > 0)
						%vehicle.teamingHP = mClampF(%vehicle.teamingHP - 0.2, 0, 100);

					%vehicleData = %vehicle.getDatablock();

					//return out of vehicle to max
					if(%player.maxVehicleLimit > 0 && %player.vehicleLimitTime < %player.maxVehicleLimit)
						%player.vehicleLimitTime += 0.01;
					
					//get the song
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

					%client.DR_hud.set($Hud::VehicleSong,"<just:Right>\c6Vehicle song: \c4" @ %vehicleSong @ "\n");
					%client.DR_hud.set($Hud::VehicleHP,"<just:Left>\c6Vehicle: \c3" @ mCeil((%vehicleMaxHp - %vehicleDmgLvl) / %vehicleMaxHp * 100) @ "\c6%");

					%mountCount = %vehicle.getMountedObjectCount();
					for(%j = 0; %j < %mountCount; %j++)
					{
						%vehiclePassenger[%i, %j] = %vehicle.getMountedObject(%j);
					}

					//tick limitted and is the driver or next person
					if(%curTime - %vehicle.lastDeathTick >= 1 && (isObject(%victim = %vehiclePassenger[%i, 0]) || isObject(%victim = %vehicle.getMountedObject(0))))
					{
						%vehicle.lastDeathTick = %curTime;
						%victim_vehicleTimeDeath = mFloor(%victim.vehicleLimitTimeDeath[%vehicle]);
						%victim_lastMainTick = %victim.lastMainTick_Vehicle;

						// afk vehicle check
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
						// refresh afk timer and remove the shapename
						else if(%victim.vehicleLimitTimeDeath[%vehicle] > 0 && %mountCount > 0 && %deathrace_time < 0)
						{
							%vehicle.schedule(1, setShapeName, "");
							if(%curTime - %victim_lastMainTick >= 1)
							{
								%victim.vehicleLimitTimeDeath[%vehicle] -= 0.15;
							}
						}
					}
				}
				//check if out of vehicle timer is disabled
				else if(!%avoidCheck)
				{
					//if the player has a max limit and tick limited and round not started
					if(%player.maxVehicleLimit > 0 && %curTime - %player.lastMainTick_Vehicle > 0.5 && %deathrace_time <= 0)
					{
						//return the timer to max
						if(%curTime - %player.lastMainTick_Vehicle > 1)
						{
							%player.lastMainTick_Vehicle = %curTime;
							%player.vehicleLimitTime -= 1;
						}

						//timer is below 0 kill player
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
					
					%client.bottomPrint(%msg1, 2, 1);

					%client.DR_hud.set($Hud::VehicleSong,"");
					%client.DR_hud.set($Hud::VehicleHP,"");
				}

				%client.DR_hud.set($Hud::HP,"<just:left>\c6Health: \c3" @ mCeil(%player.getHealth() / %player.getMaxHealth() * 100) @ "\c6%");
				%client.DR_hud.set($Hud::Score,"<just:left>\c6Score: \c3" @ %client.score);
			}
			//get hud for observers
			else if(!%client.noHud && isObject(%player = %client.spyObj) && isObject(%spyClient = %player.client) && isObject(%minigame = %spyClient.minigame) && !%isReset) // Must be spying on someone
			{
				%hud = %spyClient.DR_hud;
			}
			//print hud
			if(%hud)
			{
				if(%isStarting)
				{
					%client.bottomPrint(%hud.get(), 2, 1);
				}
				else
				{
					%client.centerPrint(%hud.get(), 2);
				}
			}
		}
	}

	cancel(%mini.DRSch);
	%mini.DRSch = %mini.schedule(%loopTime, "DR_Loop");
}

$Hud::HP = 0;
$Hud::Time = 1;
$Hud::VehicleHP = 2;
$Hud::VehicleSong = 3;
$Hud::Score = 4;

package DeathRace_MinigameLoop
{
	function GameConnection::onClientEnterGame(%c)
	{
		%c.DR_hud = Print_Create();
		parent::onClientEnterGame(%c);
	}

	function GameConnection::onClientLeaveGame(%c)
	{
		%c.DR_hud.delete();
		parent::onClientLeaveGame(%c);
	}

	function Armor::onMount(%this, %obj, %vehicle, %node)
	{
		Parent::onMount(%this, %obj, %vehicle, %node);
		%team = %obj.client.team;
		if(isObject(%team) && isObject(%base) && !isObject(%base.client)  && %base.isEnabled())
		{
			%base = %obj.getBaseMount();
			%base.setNodeColor("ALL", %team.color);
			%base.DR_TeamCheck();
		}
	}

	function Armor::onUnMount(%this, %obj, %vehicle, %node)
	{
		Parent::onUnMount(%this, %obj, %vehicle, %node);
		%base = %obj.getBaseMount();
		if(isObject(%base) && %base.isEnabled() && !isObject(%base.client))
		{
			cancel(%base.blinkColorSch);
			%count = %base.getMountedObjectCount();
			for(%j = 0; %j < %count; %j++)
			{
				%mount = %base.getMountedObject(%j).getTopMount();
				%team = %mount.team;
				if(isObject(%team))
				{
					break;
				}
			}

			if(isObject(%team))
			{
				%base.setNodeColor("ALL", %team.color);
			}
			else
			{
				cancel(%base.teamCheckSch);
			}
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

	%count = %vehicle.getMountedObjectCount();
	for(%j = 0; %j < %count; %j++)
	{
		%mount = %vehicle.getMountedObject(%j).getTopMount();
		if(%mount.client.team)
		{
			%vehiclePassenger[%j] = %mount;
		}
	}

	if(%vehicle.client.team)
	{
		%vehiclePassenger[%j] = %vehicle;
	}
	%count++;

	%isTeaming = 0;
	%currTeam = %vehiclePassenger[0].client.team;
	for(%j = 1; %j < %count; %j++)
	{
		if(%currTeam != %vehiclePassenger[%j].client.team)
		{
			%isTeaming = 1;
		}
	}	

	//Blink car if teaming, then destroy it after a certain time
	if(%isTeaming && %mini.DR_time <= 0)
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
			%blinkColor = "1 1 1 1";
			if(!isEventPending(%vehicle.blinkColorSch) && getSimTime() - %vehicle.lastBlinkTime > %vehicle.blinkTime && isObject(%spawnBrick) && %blinkColor != %oldColor)
			{
				%vehicle.lastBlinkTime = getSimTime();
				%vehicle.blinkTime = %blinkTime * 1.3;
				%vehicle.setNodeColor("ALL", %blinkColor);
				%vehicle.blinkColorSch = %vehicle.schedule(%blinkTime, "setNodeColor", "ALL", %currTeam.color);
			}
		}
	}

	%vehicle.teamCheckSch = %vehicle.schedule(100, DR_TeamCheck);
}