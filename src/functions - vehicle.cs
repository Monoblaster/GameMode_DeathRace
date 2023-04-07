// DeathRace file

package DeathRace_Vehicle
{
	//vehicles should explode in water
	function VehicleData::onEnterLiquid(%data, %obj, %coverage, %type)
	{
		Parent::onEnterLiquid(%data, %obj, %coverage, %type);
		if(isObject(%driver = %obj.getMountNodeObject(0)))
			if(%driver.getClassName() !$= "AIPlayer")
				if(!isObject(%driver.client.minigame))
					return;

		%obj.damage(%obj, %obj.getPosition(), 10000, $DamageType::Lava);
		%obj.finalExplosion();
	}

	function Vehicle::applyImpulse(%vehicle,%position,%impulse)
	{
		if(!%vehicle.getWheelPowered(2)) %impulse = 0;
		return Parent::applyImpulse(%vehicle,%position,%impulse);
	}

	function vehicle::OnActivate(%vehicle, %activatingObj, %activatingClient, %pos, %vec)
	{
		if(!%vehicle.getWheelPowered(2)) return;
		return Parent::OnActivate(%vehicle, %activatingObj, %activatingClient, %pos, %vec);
	}

	function Vehicle::damage(%obj,%sourceObject,%position,%damage,%damageType,%damageLoc)
	{
		if(isObject(%mini = getMiniGameFromObject(%obj.spawnBrick)))
		{
			if(%mini.vehicleDamageMult > 0)
				%damage *= %mini.vehicleDamageMult;
			if(isObject(%driver = %obj.getMountNodeObject(0)))
				if(isObject(%cl = %driver.client))
				{
					if(%cl.DRData["vehicleSelfDamage_Mult"] > 0)
						%damage *= %cl.DRData["vehicleSelfDamage_Mult"];
				}

			if(!%obj.getWheelPowered(2)) %damage = 0;
		}

		return Parent::damage(%obj,%sourceObject,%position,%damage,%damageType,%damageLoc);
	}

	function WheeledVehicleData::onCollision(%this, %obj, %col, %velocity, %vectorLen)
	{
		if(isObject(%driver = %obj.getMountNodeObject(0)))
		{
			if(isObject(%minigame = getMiniGameFromObject(%driver)) && %minigame.DR_EOC && $Sim::Time - %minigame.DR_StartTime > 5 && %minigame.DR_StartTime > 0)
			{
				if($Sim::Time - %col.lastInvincibility > 0.5)
				{
					%alive_obj = %obj.getDamageLevel() < 1;
					%alive_col = %col.getDamageLevel() < 1;
					if(%alive_obj && %alive_col && %vectorLen >= 25)
					{
						%obj.lastInvincibility = $Sim::Time;
						if(isObject(TankShellProjectile))
							%col.spawnExplosion(TankShellProjectile, 2);
						else if(isObject(RocketLauncherProjectile))
							%col.spawnExplosion(RocketLauncherProjectile, 2);
					}
				}
			}
		}

		Parent::onCollision(%this, %obj, %col, %velocity, %vectorLen);
	}
};
activatePackage(DeathRace_Vehicle);

///////////////////////////////////////////////////////////

function Vehicle::DR_toHealthString(%vehicle)
{
	if(!isObject(%vehicle))
		return "";

	%vHP = %vehicle.getDatablock().maxDamage - %vehicle.getDamageLevel();
	%vMaxHP = %vehicle.getDatablock().maxDamage;
	%vPer = %vHP / %vMaxHP * 100;

	return "\c6Vehicle: \c3" @ mFloor(%vPer) @ "\c6%";
}

function Vehicle::DR_toSongString(%vehicle)
{
	if(!isObject(%vehicle))
		return "";

	if(isObject(%handler = %vehicle.stereoHandler))
		if(isObject(%audioEmitter = %handler.audioEmitter))
			%vehicle_Song[%i] = %audioEmitter.profile;

	if(isObject(%vehicle_Song[%i]))
		%vehicle_Song[%i] = %vehicle_Song[%i].uiName;
	else
		%vehicle_Song[%i] = "NONE";

	return "\c6Vehicle song: \c4" @ %vehicle_Song;
}

///////////////////////////////////////////////////////////

function setVehicleSpeed(%dataBlock,%speed)
{
	if(!isObject(%dataBlock)) return;
	if(!$DefaultDataSpeed_[%dataBlock.getName()]) $DefaultDataSpeed_[%dataBlock.getName()] = %dataBlock.maxWheelSpeed;
	if(%speed <= 0) %speed = $DefaultDataSpeed_[%dataBlock.getName()];
	%datablock.maxWheelSpeed = %speed;
	return %speed;
}

///////////////////////////////////////////////////////////

function serverCmdSetVehicleSpeed(%this,%speed)
{
	if(!%this.isSuperAdmin) return;
	if(!isObject(%pl = %this.player)) return;
	if(!isObject(%vehicle = %pl.getObjectMount())) return;
	%this.chatMessage("Vehicle speed set to: " @ setVehicleSpeed(%vehicle.getDatablock(),%speed));
}