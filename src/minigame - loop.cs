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

	%curTime = $Sim::Time;

	// some local vars
	%isReset = isEventPending(%mini.resetSchedule);
	%isCustom = %mini.isCustomMini;
	%font = $Pref::Server::DRFont;
	%deathrace_maxtime = %mini.deathRaceMaxTime;
	%members = %mini.numMembers;
	%deathrace_lastReset = %mini.lastDeathRaceReset;
	%deathrace_vehicleExplodeTime = $Pref::Server::VehicleAfkTimer;
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
			%hud = "";

			if(isObject(%player = %client.player) && %player.isEnabled() && !%isReset)
			{
				%OOVT = %player.OutOfVehicleTimer;
				%maxOOVT = %player.maxOutOfVehicleTimer;
				%vehicle = %player.getBaseMount();
				if(!%client.noHud)
				{
					%hud = %client.DR_hud;
				}

				if((%curTime - %player.lastTick) > 1)
				{
					//doesn't happen before start
					if(!%isStarting)
					{
						//player is in a vehicle
						if(isObject(%vehicle) && %vehicle.isEnabled() && %vehicle.getDatablock().rideAble)
						{
							if((%curTime - %vehicle.lastTick) > 1)
							{
								// afk vehicle check
								if(VectorLen(%vehicle.getVelocity()) < 4)
								{
									%vehicleTimeDeath = mFloor(%vehicle.VehicleAfkTimer);
									if(%vehicleTimeDeath < %deathrace_vehicleExplodeTime)
									{
										cancel(%vehicle.shapeNameSch);
										%vehicle.shapeNameSch = %vehicle.schedule(3000, setShapeName, "");
										%vehicle.setShapeName("'AFK', exploding in " @ (%deathrace_vehicleExplodeTime - %vehicleTimeDeath) @ "s");
										%vehicle.setShapeNameDistance(%shapeNameDistance);
										if(isObject(%vehicle.client))
										{
											%vehicle.client.bottomPrint("\c6'AFK', exploding in " @ (%deathrace_vehicleExplodeTime - %vehicleTimeDeath) @ "s",true,%loopTime * 2 / 1000);
										}
									}

									if(%vehicleTimeDeath > %deathrace_vehicleExplodeTime)
									{
										%vehicle.damage(%vehicle, %vehicle.getPosition(), 999999);
									}

									%vehicle.VehicleAfkTimer += 1;
								}
								else if(%vehicle.VehicleAfkTimer > 0)
								{
									%vehicle.schedule(1, setShapeName, "");
									%vehicle.VehicleAfkTimer = mClampF(%vehicle.VehicleAfkTimer - 0.15,0,100);
								}
								%vehicleTick = true;
							}
						}
						else
						{
							// out of vehicle timer
							if(%maxOOVT !$= "" && !%avoidCheck)
							{
								%player.OutOfVehicleTimer = mClampF(%OOVT + 1,0,%maxOOVT);
								if(%OOVT >= %maxOOVT)
								{
									%player.addHealth(-12);
								}
								
								%client.bottomPrint("<just:center><font:" @ %font @ ":22>\c6You need to get back in your vehicle!\n\c6Time: \c3" @ getTimeString(mCeil(%maxOOVT - %OOVT)), 2, 1);
							}
						}
					}

					// healing over time
					%lastDamageTime = getSimTime() - %player.lastDamageTime;
					if(%lastDamageTime > 7000 && %player.getHealth() < %player.getMaxHealth())
					{
						%heal = %lastDamageTime / 7000;
						%player.addHealth(0.058 * %heal);
					}

					if(isObject(%vehicle) && %vehicle.isEnabled() && %vehicle.getDatablock().rideAble)
					{
						//refill out of vehicle timer
						%player.OutOfVehicleTimer = mClampF(%OOVT - 0.01,0,%maxOOVT);

						if((%curTime - %vehicle.lastTick) > 1)
						{
							//return teamingHP to 0
							if(!isEventPending(%vehicle.teamCheckSch))
							{
								%vehicle.teamingHP = mClampF(%vehicle.teamingHP - 0.2, 0, 100);
							}
							%vehicleTick = true;
						}
					}

					if(%vehicleTick)
					{
						%vehicle.lastTick = %curTime;
					}
					%player.lastTick = %curTime;
				}
			}
			//get hud for observers
			else if(!%client.noHud && isObject(%spyClient = %client.spyObj.client) && isObject(%minigame = %spyClient.minigame) && !%isReset)
			{
				%hud = %spyClient.DR_hud;
			}

			if(%hud)
			{
				if(%isStarting)
				{
					%client.bottomPrint(%hud.get(), %loopTime * 2 / 1000, 1);
				}
				else
				{
					%client.centerPrint(%hud.get(), %loopTime * 2 / 1000);
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

function ShapeBase::SetHud(%obj,%slot,%s)
{
	%mounted = %obj.getMountedObjects();
	%count = getWordCount(%mounted) @ %obj;
	for(%i = 0; %i < %count; %i++)
	{
		%player = getWord(%mounted,%i);
		%client = %player.client;
		if(isObject(%client))
		{
			%client.DR_Hud.set(%slot,%s);
		}
	}
}

function WheeledVehicleData::onDamage(%this,%obj,%damage)
{
	ShapeBase::onDamage(%this,%obj,%damage);
}

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

	function NewStereo_Set(%mount,%musicData)
	{
		%song = "NONE";
		if(isObject(%musicData))
		{
			%song = %musicData.uiName;
		}
		%client = %mount.client;
		if(isObject(%client))
		{
			%client.DR_hud.set($Hud::VehicleSong,"<just:Right>\c6Vehicle song: \c4" @ %song @ "\n");
		}
		%mount.setHud($Hud::VehicleSong,"<just:Right>\c6Vehicle song: \c4" @ %song @ "\n");
		return parent::NewStereo_Set(%mount,%musicData);
	}

	function ShapeBase::onDamage(%db,%obj,%damage)
	{
		%damage = %obj.getDamageLevel();
		%MaxHp = %db.maxDamage;
		%client = %obj.client;
		if(isObject(%client))
		{
			%client.DR_hud.set($Hud::HP,"<just:left>\c6Health: \c3" @ mCeil((%MaxHp - %damage) / %MaxHp * 100) @ "\c6%");
		}
		%obj.setHud($Hud::VehicleHP,"<just:Left>\c6Vehicle: \c3" @ mCeil((%MaxHp - %damage) / %MaxHp * 100) @ "\c6%");
		return parent::onDamage(%db,%obj,%damage);
	}	

	function GameConnection::SpawnPlayer(%c)
	{
 		%r = parent::SpawnPlayer(%c);
		%c.DR_hud.set($Hud::HP,"<just:left>\c6Health: \c3" @ mCeil(%c.player.getHealth() / %c.player.getMaxHealth() * 100) @ "\c6%");
		%c.DR_hud.set($Hud::VehicleSong,"");
		%c.DR_hud.set($Hud::VehicleHP,"");
		return %r;
	}

	function GameConnection::SetScore(%c,%score)
	{
		%c.DR_hud.set($Hud::Score,"<just:left>\c6Score: \c3" @ %score);
		return parent::SetScore(%c,%score);
	}

	function Armor::onMount(%this, %obj, %vehicle, %node)
	{
		Parent::onMount(%this, %obj, %vehicle, %node);
		%team = %obj.client.team;
		%base = %obj.getBaseMount();
		if(isObject(%base) && %base.isEnabled() && %base.getDataBlock().rideAble && isObject(%team))
		{
			NewStereo_Set(%obj,"");
			if(!isObject(%base.client.team))
			{
				%base.setNodeColor("ALL", %team.color);
			}
			%base.DR_TeamCheck();

			%song = "NONE";
			if(isObject(%handler = %base.stereoHandler) && isObject(%audioEmitter = %handler.audioEmitter) 
			&& isObject(%song = %audioEmitter.profile))
			{

					%song = %song.uiName;

			}
			%damage = %base.getDamageLevel();
			%MaxHp = %base.getDatablock().maxDamage;
			%c = %obj.client;
			if(isObject(%c))
			{
				%c.DR_hud.set($Hud::VehicleSong,"<just:Right>\c6Vehicle song: \c4" @ %song @ "\n");
				%c.DR_hud.set($Hud::VehicleHP,"<just:Left>\c6Vehicle: \c3" @ mCeil((%MaxHp - %damage) / %MaxHp * 100) @ "\c6%");
			}
		}
	}

	function Armor::onUnMount(%this, %obj, %vehicle, %node)
	{
		if(isObject(%vehicle))
		{
			%base = %vehicle.getBaseMount();
		}
		
		Parent::onUnMount(%this, %obj, %vehicle, %node);

		%c = %obj.client;
		if(isObject(%c))
		{
			%c.DR_hud.set($Hud::VehicleSong,"");
			%c.DR_hud.set($Hud::VehicleHP,"");
		}

		if(isObject(%base) && %base.isEnabled())
		{
			%mounted = %base.getMountedObjects() SPC %base;
			%count = getWordCount(%mounted);
			for(%i = 0; %i < %count; %i++)
			{
				%team = getWord(%mounted,%i).client.team;
				if(isObject(%team))
				{
					cancel(%base.blinkColorSch);
					%base.setNodeColor("ALL", %team.color);
					return;
				}
			}
			cancel(%base.teamCheckSch);
		}
	}
};
activatePackage(DeathRace_MinigameLoop);

function ShapeBase::DR_TeamCheck(%vehicle)
{
	cancel(%vehicle.teamCheckSch);
	%mini = getMinigameFromObject(%vehicle);
	if(!isObject(%mini) || !%mini.isCustomMini)
		return;

	%vehicleData = %vehicle.getDatablock();
	%count = %vehicle.getMountedObjectCount();
	%mounted = %vehicle.getMountedObjects() SPC %vehicle;
	%count = getWordCount(%mounted);
	%playerCount = 0;
	for(%i = 0; %i < %count; %i++)
	{
		%player = getWord(%mounted,%i);
		if(isObject(%player.client))
		{
			%vehiclePassenger[%playerCount] = %player;
			%playerCount++;
		}
	}

	%isTeaming = 0;
	%currTeam = %vehiclePassenger[0].client.team;
	for(%j = 1; %j < %playerCount; %j++)
	{
		if(%currTeam != %vehiclePassenger[%j].client.team)
		{
			%isTeaming = 1;
			break;
		}
	}	
	
	//Blink car if teaming, then destroy it after a certain time
	if(%isTeaming)
	{
		%vehicle.teamingHP = mClampF(%vehicle.teamingHP + 0.3, 0, 100);

		%maxBlinkTime = 1200;
		%blinkTime = mClampF(%maxBlinkTime - %vehicle.teamingHP * 20, 0, %maxBlinkTime);

		if(%blinkTime == 0)
		{
			%vehicle.damage(%vehicle, %vehicle.getPosition(), 999999, $DamageType::Default);
		}
		else
		{
			//Blink the car
			%blinkColor = "1 1 1 1";
			if(!isEventPending(%vehicle.blinkColorSch) && getSimTime() - %vehicle.lastBlinkTime > %vehicle.blinkTime)
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