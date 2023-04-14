// Deathrace - Minigame functions

package DeathRace_Minigame
{
	function MapChanger_onMapChanged()
	{
		Parent::MapChanger_onMapChanged();
		$CACHE::MapChanger::WonRounds = 0;
		$DefaultMinigame.round = 0;
	}

	function GameConnection::AutoAdminCheck(%client)
	{
		%r = Parent::AutoAdminCheck(%client);

		%client.DeathRace_Load();

		return %r;
	}

	function GameConnection::onClientLeaveGame(%client)
	{
		return Parent::onClientLeaveGame(%client);
	}
	
	function MiniGameSO::addMember(%this, %member)
	{
		if(%member.unlockTitle("V8 Beta") == 1)
		{
			%member.unlockTitle("V8 Beta"); // remove after beta
			%member.chatMessage("\c6Welcome! Thank you for being a beta test for Deathrace. We really appreciate that you are helping out and finding bugs/issues we could not find ourselves.");
		}

		Parent::addMember(%this, %member);
		%member.Deathrace_Load();
		%member.setMaxTools(7);
		
		%member.TotalPlayTime = $Sim::time;
	}
	
	function MiniGameSO::removeMember(%this, %member)
	{
		%member.DeathRace_Save();
		%member.clearMaxTools();
		%member.DRInventoryUI_clear();
		%member.TotalPlayTime = $Sim::Time;
		Parent::removeMember(%this, %member);
	}

	function GameConnection::onDeath(%this, %killerObj, %killerClient, %damageType, %position)
	{
		Parent::onDeath(%this, %killerObj, %killerClient, %damageType, %position);

		if(isObject(%killerClient) && %killerClient.getClassName() $= "GameConnection" && %this != %killerClient)
		{
			%killerClient.dataInstance($DR::SaveSlot).DR_totalKills++;
			%killerClient.unlockAchievement("Unstoppable");
			%killerClient.unlockAchievement("On the Kill");

			%killerClient.addDRKill(1);
		}

		if(%damageType != $DamageType::AFK)
			%this.dataInstance($DR::SaveSlot).DR_totalDeaths++;

		%this.DRInventoryUI_clear();
	}

	function SimObject::setNTObjectName(%obj, %name)
	{
		Parent::setNTObjectName(%obj, %name);
		%newName = %obj.getName();
		if(striPos(%newName, "_DRItem_") == 0)
		{
			%item = getSubStr(%newName, 8, strLen(%newName)-1);
			if(isObject(%item))
			{
				ItemNameBrickGroup.add(%obj);
				%obj.setItem(%item);
				%obj.itemDataName = %item;
			}
		}
	}
};
activatePackage(DeathRace_Minigame);

// -------------------------- Regular functions -------------------------- \\

if(!isObject(ItemNameBrickGroup))
	new SimSet(ItemNameBrickGroup);

function MinigameSO::spawnItems(%mini)
{
	%items = 0;
	if((%c = ItemNameBrickGroup.getCount()) > 0)
	{
		for(%i = 0; %i < %c; %i++)
		{
			%br = ItemNameBrickGroup.getObject(%i);
			if(%br.itemDataName !$= "")
			{
				%items++;
				%br.setItem(%br.itemDataName);
			}
		}
	}

	return %items;
}

function MinigameSO::respawnItems(%mini)
{
	%items = 0;
	if((%c = ItemBrickGroup.getCount()) > 0)
	{
		for(%i = 0; %i < %c; %i++)
		{
			%br = ItemBrickGroup.getObject(%i);
			if(%br.itemDataName !$= "")
			{
				%items++;
				%br.setItem(%br.itemDataName);
			}
		}
	}

	return %items;
}

function MinigameSO::deleteItems(%mini)
{
	%items = 0;
	%c = MissionCleanUp.getCount();
	for(%i = 0; %i < %c; %i++)
	{
		%obj = MissionCleanUp.getObject(%i);
		if(%obj.getClassName() $= "Item" && isObject(%spawnBr = %obj.spawnBrick) && getMinigameFromObject(%spawnBr) == %mini)
		{
			%spawnBr.itemDataName = %obj.getDatablock().getName();
			if(!ItemBrickGroup.isMember(%spawnBr))
				ItemBrickGroup.add(%spawnBr);

			%obj.schedule(0, "delete");
			%items++;
		}
	}

	return %items;
}

function MinigameSO::addNewItemToAll(%this,%item)
{
	for(%i=0;%i<%this.numMembers;%i++)
		if(isObject(%pl = %this.member[%i].player))
			if(%pl.getClassName() $= "Player")
				%pl.addNewItem(%item);
}

function MinigameSO::DR_ScrambleVehicleScaleLoop(%this, %group)
{
	cancel(%this.DR_RandomVehicleScaleLoopSch);
	if(!isObject(%group))
		%group = nameToID("Brickgroup_888888");

	if(!isObject(%group))
		return;

	if(!%this.DR_RandomVehicleScaleLoop)
		return;

	%countVe = %group.NTObjectCount["_car"];
	for(%b=0;%b<%countVe;%b++)
	{
		%brick = %group.NTObject["_car",%b];
		if(isObject(%vehicle = %brick.vehicle))
		{
			if(%this.DR_RandomVehicleScale)
				%vehicle.schedule(100, setScale, getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
			else
				%vehicle.schedule(100, setScale, vectorScale("1 1 1", %this.vehicleScale));
		}
	}

	%countVeA = %group.NTObjectCount["_admincar"];
	for(%z=0;%z<%countVeA;%z++)
	{
		%brickV = %group.NTObject["_admincar",%z];
		if(isObject(%vehicle = %brick.vehicle))
		{
			if(%this.DR_RandomVehicleScale)
				%vehicle.schedule(100, setScale, getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
			else
				%vehicle.schedule(100, setScale, vectorScale("1 1 1", %this.vehicleScale));
		}
	}

	%this.DR_RandomVehicleScaleLoopSch = %this.schedule(10000, "DR_ScrambleVehicleScaleLoop");
}

///////////////////////////////////////////////////

function DR_MinigameTeamSO::incScoreAll(%this,%mode,%score,%client)
{
	for(%i=0; %i < %this.numMembers; %i++)
	{
			%member = %this.member[%i];
			if((%mode == 0 || %mode $= "dead") && !isObject(%member.player) && %member != %client)
				%member.incScore(%score);
			else if((%mode == 1 || %mode $= "alive") && isObject(%member.player) && %member != %client)
				%member.incScore(%score);
	}
}