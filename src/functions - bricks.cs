// DeathRace file

package DeathRace_Bricks
{
	function fxDTSBrick::spawnVehicle(%this, %delay)
	{
		Parent::spawnVehicle(%this, %delay);
		%this.schedule((%delay | 0) + 100, "DR_CheckVehicle");
	}
};
activatePackage("DeathRace_Bricks");

function DR_GetRandomDeathName(%name)
{
	%names = "No double points for you" @ (strLen(%name) > 0 ? ", " @ %name : "") TAB
	"Dangerous brick" TAB
	strReplace($Pref::Player::NetName @ "'s", "s\'s", "s\'") @ " win brick" TAB
	"ALLAHU AKBAR" TAB
	"Mushroom" TAB
	"Button" TAB
	"Jeff";
	return getField(%names, getRandom(0, getFieldCount(%names)-1));
}

function DR_RandomName()
{
	%r = getRandom(0, 5);
	switch(%r)
	{
		case 1:
			%name = "Johnny";

		case 2:
			%name = "Blah";

		case 3:
			%name = "Banana";

		case 4:
			%name = "Tear ripper";

		case 5:
			%name = "Your nightmare";

		default:
			%name = "Jeff";
	}

	return "(AI) " @ %name;
}

function getNTBrickcount(%name, %brickgroup)
{
	%name = stripChars(%name, "!@#$%^&*(){}[]=;\':\"\\,./<>?`~");
	%name = strReplace(%name, "-", "DASH");
	%name = strReplace(%name, " ", "_");

	%bl_id = %brickgroup;
	if(isObject(%bl_idBrickgroup = "BrickGroup_" @ %bl_id))
		%brickgroup = %bl_idBrickgroup;

	if(isObject(%brickgroup))
		return %brickgroup.NTObjectCount_[%name];
	else
	{
		%count = 0;
		for(%i = 0; %i < MainBrickGroup.getCount(); %i++)
		{
			%brickgroup = MainBrickGroup.getObject(%i);
			%count += mFloor(%brickgroup.NTObjectCount_[%name]);
		}
	}

	return %count;
}


function getNTBrick(%name, %num, %brickgroup)
{
	%name = stripChars(%name, "!@#$%^&*(){}[]=;\':\"\\,./<>?`~");
	%name = strReplace(%name, "-", "DASH");
	%name = strReplace(%name, " ", "_");

	%num = mFloor(%num);

	%bl_id = %brickgroup;
	if(isObject(%bl_idBrickgroup = "BrickGroup_" @ %bl_id))
		%brickgroup = %bl_idBrickgroup;
	
	if(isObject(%brickgroup))
		return %brickgroup.NTObject_[%name, %num];
	else
	{
		for(%i = 0; %i < MainBrickGroup.getCount(); %i++)
		{
			%brickgroup = MainBrickGroup.getObject(%i);
			if(%brickgroup.NTObjectCount_[%name] > 0)
				return %brickgroup.NTObject_[%name, %num];
		}
	}
	return -1;
}

///////////////////////////////////////////////////////////////////////////////////////////

//registerOutputEvent(fxDTSBrick,"StartDeathRace","int 10 60 30",1);
registerOutputEvent(fxDTSBrick, "WinDeathRace", "", 1);

function fxDTSBrick::DR_CheckVehicle(%this)
{
	if(!isObject(%vehicle = %this.vehicle))
		return;

	if(!isObject(%mini = getMiniGameFromObject(%this)))
		return;

	if(%mini.isStartingDR)
		%this.schedule(50, setVehiclePowered, 0, %this.client);
	else
		%this.schedule(50, setVehiclePowered, 1, %this.client);
}

function SimGroup::DR_CloseDoors(%this)
{
	%count0 = %this.NTObjectCount["_door"];
	for(%a=0;%a<%count0;%a++)
	{
		%obj0 = %this.NTObject["_door",%a];
		%obj0.schedule(1 * %a,disappear,0);
	}

	%count2 = %this.NTObjectCount["_admindoor"];
	for(%j=0;%j<%count2;%j++)
	{
		%obj2 = %this.NTObject["_admindoor",%j];
		%obj2.schedule(1 * %j,disappear,0);
	}

	%count1 = %this.NTObjectCount["_Zone"];
	for(%i=0;%i<%count1;%i++)
	{
		%obj1 = %this.NTObject["_Zone",%i];
		%obj1.schedule(1 * %i,setEventEnabled,"0 1 2 3 4",1);
	}
}

function SimGroup::DR_OpenDoors(%this)
{
	%count0 = %this.NTObjectCount["_door"];
	for(%a=0;%a<%count0;%a++)
	{
		%obj0 = %this.NTObject["_door",%a];
		%obj0.schedule(1 * %a,disappear,-1);
	}

	%count2 = %this.NTObjectCount["_admindoor"];
	for(%j=0;%j<%count2;%j++)
	{
		%obj2 = %this.NTObject["_admindoor",%j];
		%obj2.schedule(1 * %j,disappear,-1);
	}

	%count1 = %this.NTObjectCount["_Zone"];
	for(%i=0;%i<%count1;%i++)
	{
		%obj1 = %this.NTObject["_Zone",%i];
		%obj1.schedule(1 * %i,setEventEnabled,"0 1 2 3 4",0);
	}
}

function SimGroup::DR_DisableVehicles(%this)
{
	%countVe = %this.NTObjectCount["_car"];
	for(%b=0;%b<%countVe;%b++)
	{
		%brick = %this.NTObject["_car",%b];
		%brick.schedule(50,setVehiclePowered,0,%this.client);
		%brick.setEventEnabled("0 1 2",1);
	}

	%countVeA = %this.NTObjectCount["_admincar"];
	for(%z=0;%z<%countVeA;%z++)
	{
		%brickV = %this.NTObject["_admincar",%z];
		%brickV.schedule(50,setVehiclePowered,0,%this.client);
	}
}

function SimGroup::DR_EnableVehicles(%this)
{
	%countVe = %this.NTObjectCount["_car"];
	for(%b=0;%b<%countVe;%b++)
	{
		%brick = %this.NTObject["_car",%b];
		%brick.schedule(50,setVehiclePowered,1,%this.client);
		%brick.setEventEnabled("0 1 2",1);
	}

	%countVeA = %this.NTObjectCount["_admincar"];
	for(%z=0;%z<%countVeA;%z++)
	{
		%brickV = %this.NTObject["_admincar",%z];
		%brickV.schedule(50,setVehiclePowered,1,%this.client);
	}
}

function fxDTSBrick::StartDeathRace(%this, %time, %client)
{
	setRandomSeed(getsimtime());
	if(!isObject(%mini = getMiniGameFromObject(%this)))
		return;

	if($Server::MapChanger::Changing)
		return;

	%mini.DR_Loop();
	%lastTime = %mini.lastDeathRaceReset;
	if(getSimTime() - %lastTime < 500) return;
	%time = mFloor(%time * 1);

	//Redlight brickname - StartLight3
	//Yellowlight brickname - StartLight2
	//Greenlight brickname - StartLight1
	%mini.lastDeathRaceReset = getSimTime();
	%mini.deathRaceMaxTime = %time;

	if(%mini.CurrentMap !$= $Server::MapChanger::CurrentMap)
		$CACHE::MapChanger::WonRounds = 0;
	%mini.CurrentMap = $Server::MapChanger::CurrentMap;
	
	%rounds      = $CACHE::MapChanger::WonRounds;
	%totalRounds = $Pref::Server::MapChanger::MaxWinsBeforeRoundChange;
	
	%mini.isStartingDR = 1;
	%mini.messageAll('MsgUploadStart',"<font:Arial Black:25>\c6New Deathrace round! Race will begin in \c3" @ %time @ " second" @ (%time == 1 ? "" : "s") @ "\c6.");

	%this.ResetDeathRace();

	%countBrick["Green"] = %group.NTObjectCount["_StartLight1"];
	for(%gg=0;%gg<%countVeA;%gg++)
	{
		%brick["Green"] = %group.NTObject["_StartLight1",%gg];
		%brick["Green"].setColorFX(0);
	}

	%countBrick["Yellow"] = %group.NTObjectCount["_StartLight2"];
	for(%yy=0;%yy<%countVeA;%yy++)
	{
		%brick["Yellow"] = %group.NTObject["_StartLight2",%yy];
		%brick["Yellow"].setColorFX(0);
	}

	%countBrick["Red"] = %group.NTObjectCount["_StartLight3"];
	for(%rr=0;%rr<%countVeA;%rr++)
	{
		%brick["Red"] = %group.NTObject["_StartLight3",%rr];
		%brick["Red"].setColorFX(3);
	}

	%mini.DR_time = %time;
	%mini.DR_sch = %this.schedule(1000,DeathRaceLoop,%client);
}

function fxDTSBrick::WinDeathRace(%this, %client)
{
	if(isObject(%client) && !%client.player.hasWonDeathRace && isObject(%mini = %client.minigame) && %mini.isCustomMini && isObject(%teams = %mini.teams) && !%mini.resetting)
	{
		%client.player.hasWonDeathRace = 1;
		%scoreX = 1;
		%numMembers = %mini.numMembers - 5;
		%score = 6 + mFloor(%numMembers / 3);

		if(%numMembers > 0)
		{
			%scoreX = 1;
		 	%teamScore = 3 + mFloor(%numMembers / 3);
		}
		else
		{
			%no = 1;
			%winMsg = "Sorry, there are not enough players for full winning points.";
		}

		if(isObject(%team = %client.team) && !%no)
		{
			if(%team.getLiving() == 1)
			{
				%scoreX = 2;
				%winMsg = "! The winner gets double points (" @ (%score * %scoreX) @ ") for being the only survivor on the team!";
			}
			else 
			{
				%winMsg = ", and their team wins (" @ %teamScore @ " points)!";
			}
		}

		%mapName    = $Temp::MapSys_CurrentMapName;
		%mapVarName = "totalButtonWinsOn" @ getSafeVariableName(%mapName);

		// if(%score)
		// {
		%client.incScore(%score * %scoreX);
		if(isObject(%team) && %team.getLiving() > 1)
		{
			for(%i = 0; %i < %team.numMembers; %i++)
			{
				%teamMember = %team.member[%i];
				if(%teamMember != %client && isObject(%teamMember.player) && %teamMember.player.getState() !$= "dead")
				{
					%teamMember.incScore(%teamScore);

					%teamMember.dataInstance($DR::SaveSlot).DR_FirstWin = 1;
					%teamMember.unlockAchievement("I Win!");
					
					%teamMember.dataInstance($DR::SaveSlot).DR_totalWins++;
					%teamMember.dataInstance($DR::SaveSlot).DR_totalWinsByButton++;

					%teamMember.unlockAchievement("I am a winner!"); 
					%teamMember.unlockAchievement("Veteran Driver");
					%teamMember.unlockAchievement("Demolition Uber");

					if(%mapName !$= "")
					{
						%teamMember.dataInstance($DR::SaveSlot).DeathRaceData[%mapVarName]++;
						if(%teamMember.dataInstance($DR::SaveSlot).DeathRaceData[%mapVarName] >= 10)
						{
							%teamMember.unlockAchievement(%mapName @ " Expert");
						}
						
					}
				}
			}
		}
		// }

		%time = (getSimTime() - %client.lastSpawnTime) / 1000;
		%timeString = getTimeString(mFloor(%time));
		%mini.messageAll('MsgUploadEnd',"<font:arial bold:22>" @ %client.team.colorHexStr @ %client.getPlayerName() @ " (" @ %score @ " points)" @ %winMsg);
		echo("DRMini - " @ %client.getPlayerName() @ " (" @ %score @ " points)" @ %winMsg);
		if(%mini.DR_crazyspeed)
			%mini.messageAll('',"<font:arial bold:20>  \c4-> \c6Crazy race was completed in \c3" @ %timeString @ "\c6.");
		else
			%mini.messageAll('',"<font:arial bold:20>  \c4-> \c6Regular race was completed in \c3" @ %timeString @ "\c6.");

		%mini.schedulereset();
		%mini.resetting = 1;

		%client.dataInstance($DR::SaveSlot).DR_FirstWin = 1;
		%client.unlockAchievement("I Win!");
		
		%client.dataInstance($DR::SaveSlot).DR_totalWins++;
		%client.dataInstance($DR::SaveSlot).DR_totalWinsByButton++;

		%client.unlockAchievement("I am a winner!"); 
		%client.unlockAchievement("Veteran Driver");
		%client.unlockAchievement("Demolition Uber");

		if(%mapName !$= "")
		{
			%client.dataInstance($DR::SaveSlot).DeathRaceData[%mapVarName]++;
			if(%client.dataInstance($DR::SaveSlot).DeathRaceData[%mapVarName] >= 10)
			{
				%client.unlockAchievement(%mapName @ " Expert");
			}
}
	}
	else if(isObject(%mini = %client.minigame) && isFunction("fxDTSBrick", "FFExplode"))
	{
		%str = DR_GetRandomDeathName(%client.getPlayerName());
		if(%str $= "Jeff" && isObject(MyNameIsJeffSound))
			serverPlay2D(MyNameIsJeffSound);

		if(isFunction(fxDTSBrick, FFExplode))
			%this.FFExplode(TankShellProjectile, 5, %str);
	}
	else if(!isObject(%mini))
		%client.chatMessage("\c5You win Deathrace! Oh wait.. you have to be in the minigame XD");
}

function fxDTSBrick::DeathRaceLoop(%this,%client)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)))
		return;
	cancel(%mini.DR_sch);
	%time = %mini.DR_time--;
	%group = %this.getGroup();

	if(%time == %time/4 && %time > 0) %mini.messageAll('MsgUploadEnd',"<font:Arial Bold_22:20>\c6The race begins in \c4" @ %time @ " seconds(s) \c6!");
	if(%time == 3)
	{
		if($Pref::Server::PlayMarioKartSound && isFunction(minigameso, playsound))
			%mini.playSound(StartRaceSound);
	}
	if(%time == 2)
	{

		%tag = 'MsgUploadEnd';
		if($Pref::Server::PlayMarioKartSound && isFunction(minigameso, playsound))
			%tag = '';

		%red = 3;
		%yellow = 0;
		%green = 0;
		%mini.messageAll(%tag,"<font:Arial Black:22>\c2Get ready!");
	}
	if(%time == 1)
	{
		%tag = 'MsgUploadEnd';
		if($Pref::Server::PlayMarioKartSound && isFunction(minigameso, playsound))
			%tag = '';

		%red = 0;
		%yellow = 3;
		%green = 0;
		%mini.messageAll(%tag,"<font:Arial Black:22>\c3Get set..");
	}
	if(%time <= 0)
	{
		%tag = 'MsgProcessComplete';
		if($Pref::Server::PlayMarioKartSound && isFunction(minigameso, playsound))
			%tag = '';

		%red = 0;
		%yellow = 0;
		%green = 3;

		//%mini.checkLastManStanding();
		%mini.messageAll(%tag,"<font:Arial Black:22>\c5The race has started!");
		%mini.DR_StartTime = $Sim::Time;
		%mini.isStartingDR = 0;

		%c = MissionCleanUp.getCount();
		for(%i = 0; %i < %c; %i++)
		{
			%obj = MissionCleanUp.getObject(%i);
			if(%obj.getClassName() $= "Item" && isObject(%spawnBr = %obj.spawnBrick) && %obj.getDataBlock().getName() !$= "BigStaticItem")
			{
				%spawnBr.itemDataName = %obj.getDatablock().getName();
				if(!ItemBrickGroup.isMember(%spawnBr) && getMinigameFromObject(%spawnBr) == %mini)
					ItemBrickGroup.add(%spawnBr);

				%obj.schedule(0, "delete");
			}
		}

		%group.DR_OpenDoors();

		%group.DR_EnableVehicles();

		%done = 1;

		%count = %mini.numMembers;
		for(%i = 0; %i < %count; %i++)
		{
			%currClient = %mini.member[%i];
			if(isObject(%currClient))
			{
				%currClient.DR_hudObject.set($Hud::Time,"<just:right>\n");
				%player = %currClient.player;
				if(isObject(%player))
				{
					if(vectorDist(%player.getPosition(), %player.DR_SpawnPosition) < 5)
					{
						%player.kill();
						continue;
					}

					%currClient.dataInstance($DR::SaveSlot).DR_totalRounds++;
					if(!DRInventoryUI_Ready(%currClient))
					{
						%currClient.chatMessage("\c6Loading default loadout for the race");
						%player.Shop_LoadList($DR::DefaultLoadout);
						DRInventoryUI_Ready(%currClient);
					}
				}
				%currClient.DRInventoryUI_clear();
			}
		}
	}
	else
	{
		%string = getTimeString(mCeil(%mini.dr_time));
		%count = %mini.numMembers;
		for(%i = 0; %i < %count; %i++)
		{
			%currClient = %mini.member[%i];
			if(isObject(%currClient))
			{
				%currClient.DR_hudObject.set($Hud::Time,"<just:right>\c6Starting in " @ %string  @ "\n");
			}
		}
	}
	

	%countBrick["Green"] = %group.NTObjectCount["_StartLight1"];
	for(%gg=0;%gg<%countBrick["Green"];%gg++)
	{
		%brick["Green"] = %group.NTObject["_StartLight1", %gg];
		%brick["Green"].setColorFX(%green);
	}

	%countBrick["Yellow"] = %group.NTObjectCount["_StartLight2"];
	for(%yy=0;%yy<%countBrick["Yellow"];%yy++)
	{
		%brick["Yellow"] = %group.NTObject["_StartLight2", %yy];
		%brick["Yellow"].setColorFX(%yellow);
	}

	%countBrick["Red"] = %group.NTObjectCount["_StartLight3"];
	for(%rr=0;%rr<%countBrick["Red"];%rr++)
	{
		%brick["Red"] = %group.NTObject["_StartLight3", %rr];
		%brick["Red"].setColorFX(%red);
	}

	if(!%done)
		%mini.DR_sch = %this.schedule(1000, DeathRaceLoop, %client);
}

function fxDTSBrick::ResetDeathRace(%this,%client)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)))
		return;
	cancel(%mini.DR_sch);
	setVehicleSpeed($Pref::Server::DeathRace_Vehicle, 0);

	//Minigame specials
	//serverCmdTimeScale(publicClient,1);
	cancel(%mini.DR_RandomVehicleScaleLoopSch);
	setTimescale(1);
	%mini.avoidVehicleDeathCheck = 0;
	%mini.DR_EOC = 0;
	%mini.DR_doubleHealth = 0;
	%mini.DR_crazyspeed = 0;
	%mini.DR_timescale2 = 0;
	%mini.DR_StartTime = 0;
	%mini.DR_RandomVehicleScale = 0;
	%mini.DR_RandomVehicleScaleLoop = 0;
	%mini.noitems = 0;
	%mini.tempPlayerData = 0;

	if(%mini.DR_SpecialBought != 0)
		%mini.DR_SpecialBought = %mini.DR_SpecialBought;
	else
		%mini.DR_SpecialBought = 0;

	%mini.vehicleDamageMult = 0;
	%mini.vehicleScale = 1;
	%mini.playerScale = 1;
	

	%mini.DR_Vehicle = $Pref::Server::DeathRace_Vehicle;
	if(!isObject(%mini.DR_Vehicle))
		%mini.DR_Vehicle = "JeepVehicle";

	%mini.DR_time = 0;
	%mini.cannotSuicide = false;

	%group = %this.getGroup();
	// if((%c = ItemBrickGroup.getCount()) > 0)
	// {
	// 	for(%i = 0; %i < %c; %i++)
	// 	{
	// 		%br = ItemBrickGroup.getObject(%i);
	// 		if(%br.itemDataName !$= "")
	// 			%br.setItem(%br.itemDataName);
	// 	}
	// }

	LoadEnvironmentFromFile(filePath($Server::MapSys_Path) @ "/" @ $Temp::MapSys_CurrentMapName @ ".txt", 1);
	%r = getRandom(1, mClampF($Server::DeathRace_Luck, 1, 10));
	if(%mini.DR_SpecialBought != 0)
		%r = 1;

	if(%r == 1)
	{
		if(%mini.DR_SpecialBought != 0)
		{
			%mini.schedule(100, DR_SetSpecial, %mini.DR_SpecialBought, %group);
			%mini.DR_SpecialBought = 0;
		}
		else
			%mini.schedule(100, DR_SetSpecial, "random", %group);
	}
	else
	{
		%countVe = %group.NTObjectCount["_car"];
		for(%b=0;%b<%countVe;%b++)
		{
			%brick = %group.NTObject["_car",%b];
			%brick.disappear(-1);
			%brick.reColorVehicle = 1;
			if(!strLen(%brick.DR_BrickColor))
				%brick.DR_BrickColor = %brick.getColorID();

			%brick.schedule(0, setColor, %brick.DR_BrickColor);
			%brick.setVehicle(nameToID(%mini.DR_Vehicle));
			if(isObject(%vehicle = %brick.vehicle) && %mini.vehicleScale !$= "")
				%vehicle.schedule(100, setScale, vectorScale("1 1 1", %mini.vehicleScale));
		}

		%countVeA = %group.NTObjectCount["_admincar"];
		for(%z=0;%z<%countVeA;%z++)
		{
			%brickV = %group.NTObject["_admincar",%z];
			%brick.disappear(-1);
			%brick.reColorVehicle = 1;

			if(!strLen(%brick.DR_BrickColor))
				%brick.DR_BrickColor = %brick.getColorID();

			%brick.schedule(0, setColor, %brick.DR_BrickColor);
			%brick.setVehicle(nameToID(%mini.DR_Vehicle));
			if(isObject(%vehicle = %brick.vehicle) && %mini.vehicleScale !$= "")
				%vehicle.schedule(100, setScale, vectorScale("1 1 1", %mini.vehicleScale));
		}
	}

	%group.DR_CloseDoors();

	%group.DR_DisableVehicles();

	%mini.winBrick = %group.NTObject["_win", 0];
}