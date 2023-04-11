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
	%deathrace_lastReset = %mini.lastDeathRaceReset;
	%deathrace_vehicleExplodeTime = $Pref::Server::VehicleAfkTimer;
	%deathrace_time = mCeil(%mini.deathRaceMaxTime - ((getSimTime() - %mini.lastDeathRaceReset) / 1000));
	%avoidCheck = %mini.avoidVehicleDeathCheck;
	%isStarting = %mini.isStartingDR;

	%usingTimer = false || %isStarting;
	// loop through everyone
	%count = %mini.numMembers;
	for(%i = 0; %i < %count; %i++)
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
				if(!%client.dataInstance($DR::SaveSlot).DR_NoHud)
				{
					%hud = %client.DR_hudObject;
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
										%usingTimer = true;
										%vehicle.setHud($Hud::Time,"<Just:Right>Exploding in " @ getTimeString(mCeil(%deathrace_vehicleExplodeTime - %vehicleTimeDeath)) @ "\n");
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
									%player.addHealth(-24);
								}
								%usingTimer = true;
								%client.DR_hudObject.set($Hud::Time, "<Just:Right>Dying in " @ getTimeString(mCeil(%maxOOVT - %OOVT)) @ "\n");
								//%client.bottomPrint("<just:center><font:" @ %font @ ":22>\c6You need to get back in your vehicle!\n\c6Time: \c3" @ , 2, 1);
							}
						}
					}

					if(!%usingTimer)
					{
						%client.DR_hudObject.set($Hud::Time, "\n");
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
			else if(!%client.dataInstance($DR::SaveSlot).DR_NoHud && isObject(%spyClient = %client.spyObj.client) && isObject(%minigame = %spyClient.minigame) && !%isReset)
			{
				%hud = %spyClient.DR_hudObject;
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
	%count = getWordCount(%mounted);
	for(%i = 0; %i < %count; %i++)
	{
		%player = getWord(%mounted,%i);
		%client = %player.client;
		if(isObject(%client))
		{
			%client.DR_hudObject.set(%slot,%s);
		}
	}
}

function Armor::onDamage(%this,%obj,%delta)
{
	if (%delta > 0 && %obj.getState () !$= "Dead")
	{
		%flash = %obj.getDamageFlash () + ((%delta / %this.maxDamage) * 2);
		if (%flash > 0.75)
		{
			%flash = 0.75;
		}
		%obj.setDamageFlash (%flash);
		%painThreshold = 7;
		if (%this.painThreshold !$= "")
		{
			%painThreshold = %this.painThreshold;
		}
		if (%delta > %painThreshold)
		{
			%obj.playPain ();
		}
	}
	ShapeBase::onDamage(%this,%obj,%damage);
}

package DeathRace_MinigameLoop
{
	function WheeledVehicleData::onDamage(%this,%obj,%damage)
	{
		parent::onDamage(%this,%obj,%damage);
		ShapeBase::onDamage(%this,%obj,%damage);
	}

	function Armor::onDamage(%this,%obj,%damage)
	{
		parent::onDamage(%this,%obj,%damage);
	}

	function GameConnection::onClientEnterGame(%c)
	{
		%c.DR_hudObject = Print_Create();
		parent::onClientEnterGame(%c);
	}

	function GameConnection::onClientLeaveGame(%c)
	{
		%c.DR_hudObject.delete();
		parent::onClientLeaveGame(%c);
	}

	function NewStereo_Set(%mount,%musicData)
	{
		%song = "NONE";
		if(%musicData.uiName !$= "")
		{
			%song = %musicData.uiName;
		}
		%client = %mount.client;
		if(isObject(%client))
		{
			%client.DR_hudObject.set($Hud::VehicleSong,"<just:Right>\c6Vehicle song: \c4" @ %song @ "\n ");
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
			%client.DR_hudObject.set($Hud::HP,"<just:left>\c6Health: \c3" @ mCeil((%MaxHp - %damage) / %MaxHp * 100) @ "\c6%");
		}
		%obj.setHud($Hud::VehicleHP,"<just:Left>\c6Vehicle: \c3" @ mCeil((%MaxHp - %damage) / %MaxHp * 100) @ "\c6%");
		//return parent::onDamage(%db,%obj,%damage);
	}	

	function GameConnection::SpawnPlayer(%c)
	{
 		%r = parent::SpawnPlayer(%c);
		%c.DR_hudObject.set($Hud::HP,"<just:left>\c6Health: \c3" @ mCeil(%c.player.getHealth() / %c.player.getMaxHealth() * 100) @ "\c6%");
		%c.DR_hudObject.set($Hud::VehicleSong,"");
		%c.DR_hudObject.set($Hud::VehicleHP,"");
		return %r;
	}

	function GameConnection::SetScore(%c,%score)
	{
		%c.DR_hudObject.set($Hud::Score,"<just:left>\c6Score: \c3" @ %score);
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

			cancel(%base.teamCheckSch);
			%base.DR_TeamCheck();

			%song = "NONE";
			if(%base.stereoHandler.audioEmitter.profile.uiName !$= "")
			{
				%song = %base.stereoHandler.audioEmitter.profile.uiName;
			}
			%damage = %base.getDamageLevel();
			%MaxHp = %base.getDatablock().maxDamage;
			%c = %obj.client;
			if(isObject(%c))
			{
				%c.DR_hudObject.set($Hud::VehicleSong,"<just:Right>\c6Vehicle song: \c4" @ %song @ "\n");
				%c.DR_hudObject.set($Hud::VehicleHP,"<just:Left>\c6Vehicle: \c3" @ mCeil((%MaxHp - %damage) / %MaxHp * 100) @ "\c6%");
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
			%c.DR_hudObject.set($Hud::VehicleSong,"");
			%c.DR_hudObject.set($Hud::VehicleHP,"");
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
	%mini = getMinigameFromObject(%vehicle);
	if(!isObject(%mini) || !%mini.isCustomMini)
		return;

	%vehicleData = %vehicle.getDatablock();
	%mounted = %vehicle.getMountedObjects() SPC %vehicle;
	%count = getWordCount(%mounted);
	%lastTeam = "";
	for(%i = 0; %i < %count; %i++)
	{
		%currTeam = getWord(%mounted,%i).client.team;
		if(isObject(%currTeam))
		{
			if(%lastTeam)
			{
				if(%lastTeam != %currTeam)
				{
					%isTeaming = true;
					break;
				}
			}
			%lastTeam = %currTeam;
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