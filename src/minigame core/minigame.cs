// Edit what you want in here but just know that this is the main core, use your own functions for less clutter.
// ::onModeChange
// ::onMaxRoundLimit
// ::onPreRoundStart
// ::onRoundEnd
// ::onCleanUp
// ::onRoundStart
// ::onFinalRound


// REMAME all classes in this file from CustomMinigameSO to whatever you want for your preferred class name.

function createCustomMinigame(%name, %title, %class, %datablock)
{
	endAllMinigames();

	if(%name $= "")
		%name = "Mini";

	if(%title $= "")
		%title = "Default Custom Minigame";

	if(%class $= "")
		%class = "CustomMinigameSO";


	%colorIdx = 0;
	$MiniGameColorTaken[%colorIdx] = 1;

	%col0 = getColorIDTable(findclosestcolor("0 0 1 1")); // blue
	%colid0 = findclosestcolor("0 0 1 1");
	%col1 = getColorIDTable(findclosestcolor("1 0 0 1")); // red
	%colid1 = findclosestcolor("1 0 0 1");
	%col2 = getColorIDTable(findclosestcolor("1 1 0 1")); // yellow
	%colid2 = findclosestcolor("1 1 0 1");
	%col3 = getColorIDTable(findclosestcolor("0 0.78 0 1")); // green
	%colid3 = findclosestcolor("0 0.78 0 1");
	%col4 = getColorIDTable(findclosestcolor("0 0 0 1")); // black
	%colid4 = findclosestcolor("0 0 0 1");

	$DefaultMinigame = new ScriptObject(%name)
	{
		superclass = MiniGameSO;
		class = %class;
		owner = 0;
		title = %title;
		colorIdx = %colorIdx;
		numMembers = 0;
		InviteOnly = 0;
		UseAllPlayersBricks = 1;
		PlayersUseOwnBricks = 0;
		UseSpawnBricks = 1;
		Points_BreakBrick = 0;
		Points_PlantBrick = 0;
		Points_KillPlayer = 1;
		Points_KillBot = 0;
		Points_KillSelf = 0;
		Points_Die = 0;
		RespawnTime = -1;
		_RespawnTime = -1;
		VehicleRespawnTime = 5000;
		BrickRespawnTime = 10000;
		BotRespawnTime = 5000;
		FallingDamage = 1;
		WeaponDamage = 1;
		SelfDamage = 1;
		VehicleDamage = 1;
		BrickDamage = 0;
		BotDamage = 1;
		EnableWand = 0;
		EnableBuilding = 0;
		PlayerDataBlock = (isObject(%datablock) ? nameToID(%datablock) : nameToID("NoJetArmor"));
		StartEquip0 = 0;
		StartEquip1 = 0;
		StartEquip2 = 0;
		StartEquip3 = 0;
		StartEquip4 = 0;
		TimeLimit = (60 * 8);
		_TimeLimit = (60 * 8);

		// Main
		displayName = %title;

		overhealHealth = 0;
		overhealDecay = 0.75;

		allowSpecials = 1;
		specialChance = 6;

		//preloadMode = "TDM";
		lockedMode = "Team DeathMatch";

		_teams = 5;
		//
		_teamName[0] = "Blue Berries";
		_teamColorID[0] = %colid0;
		_teamColor[0] = %col0;
		//
		_teamName[1] = "Red Apples";
		_teamColorID[1] = %colid1;
		_teamColor[1] = %col1;
		//
		_teamName[2] = "Yellow Bananas";
		_teamColorID[2] = %colid2;
		_teamColor[2] = %col2;
		//
		_teamName[3] = "Green Beans";
		_teamColorID[3] = %colid3;
		_teamColor[3] = %col3;
		//
		_teamName[4] = "Admin Team";
		_teamColorID[4] = %colid4;
		_teamColor[4] = %col4;
		_teamNoAutoJoin[4] = true;
		//
		_teamIDFromColorID[%colid0] = 0;
		_teamIDFromColorID[%colid1] = 1;
		_teamIDFromColorID[%colid2] = 2;
		_teamIDFromColorID[%colid3] = 3;
		_teamIDFromColorID[%colid4] = 4;

		lockedAvatarShirt = 1;

		lives = 1;
		_lives = 1;

		WaveSpawn = 0;
		_WaveSpawn = 0;
		WaveSpawnTime = 25;
		_WaveSpawnTime = 25;
		LastWaveSpawn = 0;

		lockedMaxRound = 10;
		maxRounds = 10;
		_maxRounds = 10;
		round = 0;
		
		loadOnly = 1;
		cosmeticLiving = 1; // Basically just update live counts on prints in the server using M_PCount1 (right digit), M_PCount2 (left digit)

		playMusicData = 0;
		enableMusic = 0;

		// Custom to your liking
		isCustomMini = 1;
		doTimeScale = 1;
		DRBeginTime = 60;

		breakSpawnMessages = 1;
	};
	MiniGameGroup.add($DefaultMinigame);

	$DefaultMinigame.onCreate(); //Make sure all values are going good
	return $DefaultMinigame;
}

///////////////////////////////////////////////////////////////////////////////////////////////////

function MinigameSO::updateLives(%mini)
{
	%lives = %mini.getLiving();
	if(%lives != %mini.lastLiveCheck)
	{
		if(strLen(%lives) == 1)
		{
			%plN1 = %lives;
			%plN2 = 0;
		}
		else
		{
			%plN1 = getSubStr(%lives, 1, 1);
			%plN2 = getSubStr(%lives, 0, 1);
		}

		%mini.lastLiveCheck = %lives;
		%name = "M_PCount1";
		for(%i = 0; %i < MainBrickGroup.getCount(); %i++)
		{
			%group = MainBrickGroup.getObject(%i);
			if((%nameCount = %group.NTObjectCount_[%name]) > 0)
			{
				for(%j = 0; %j < %nameCount; %j++)
				{
					%group.NTObject_[%name, %j].setPrint($printNameTable["Letters/" @ %plN1]);
				}
			}
		}

		%name = "M_PCount2";
		for(%i = 0; %i < MainBrickGroup.getCount(); %i++)
		{
			%group = MainBrickGroup.getObject(%i);
			if((%nameCount = %group.NTObjectCount_[%name]) > 0)
			{
				for(%j = 0; %j < %nameCount; %j++)
				{
					%group.NTObject_[%name, %j].setPrint($printNameTable["Letters/" @ %plN2]);
				}
			}
		}
	}
}

function MinigameSO::respawnDead(%mini)
{
	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%client = %mini.member[%i];
		if(!isObject(%player = %client.player) || %player.getDamagePercent() >= 1)
			%client.spawnPlayer();
	}
}

function MiniGameSO::addItem(%mini, %item)
{
	%item = findItemByName(%item);
	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%cl = %mini.member[%i];
		if(isObject(%pl = %cl.player) && %pl.getState() !$= "dead")
			%pl.addNewItem(%item);
	}
}

///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////

function ScriptGroup::find(%group, %find, %exempt)
{
	%partialFindPos = 9999;
	%c = %group.getCount();
	for(%i = 0; %i < %c; %i++)
	{
		%obj = %group.getObject(%i);
		if(%obj == %exempt)
			continue;

		if(%obj.uiName $= %find || %obj == nameToID(%find))
			%found = %obj;
		else if((%pos = striPos(%obj.uiName, %find)) >= 0 && %pos < %partialFindPos)
		{
			%partialFindPos = %pos;
			%partialFind = %obj;
		}
	}

	if(isObject(%found))
		return %found;

	if(isObject(%partialFind))
		return %partialFind;

	return 0;
}

///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////

function MinigameSO::getLiving(%mini)
{
	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%client = %mini.member[%i];
		if(isObject(%player = %client.player) && %player.getDamagePercent() < 1)
		{
			%livePlayerCount++;
		}
	}

	return mFloor(%livePlayerCount);
}

function MinigameSO::getTeamLiving(%mini)
{
	if(isObject(%teams = %mini.Teams) && (%teamCount = %teams.getCount()) > 0)
	{
		for(%i = 0; %i < %teamCount; %i++)
		{
			%team = %teams.getObject(%i);
			%living = %team.getLiving();
			if(%living > 0)
				%livingTeams++;
		}
	}

	return mFloor(%livingTeams);
}

function MinigameSO::setTimescale(%mini, %timescale, %viewObject)
{
	cancel($TimescaleSch);
	setTimescale(%timescale);

	if(!isObject(%mini))
		return;

	if(%mini.numMembers <= 0)
		return;

	if(!isObject(%viewObject))
		return;

	if(%viewObject.getClassName() !$= "Player")
		return;

	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%cl = %mini.member[%i];
		if(isObject(%cl) && %cl.getClassName() $= "GameConnection")
			if(!isObject(%cl.player) && isObject(%cam = %cl.camera))
				%cam.setMode("Corpse", %viewObject);
	}
}