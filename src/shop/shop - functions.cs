function findVehicleByName(%vehicle, %val)
{
	if(isObject(%vehicle))
		return %vehicle.getName();

	if(!isObject(VehicleCache)) new ScriptObject(VehicleCache);
	if(VehicleCache.itemCount <= 0 || %val) //We don't need to cause lag everytime we try to find an Music
	{
		VehicleCache.itemCount = 0;
		for(%i = 0; %i < DatablockGroup.getCount(); %i++)
		{
			%obj = DatablockGroup.getObject(%i);
			if(%obj.getClassName() $= "WheeledVehicleData" || %obj.getClassName() $= "FlyingVehicleData")
				if(strLen(%obj.uiName) > 0)
				{
					VehicleCache.item[VehicleCache.itemCount] = %obj;
					VehicleCache.itemCount++;
				}
		}
	}

	//First let's see if we find something to be exact
	if(VehicleCache.itemCount > 0)
	{
		for(%a = 0; %a < VehicleCache.itemCount; %a++)
		{
			%objA = VehicleCache.item[%a];
			if(%objA.getClassName() $= "WheeledVehicleData" || %objA.getClassName() $= "FlyingVehicleData")
				if(%objA.uiName $= %vehicle)
					return %objA.getName();
		}

		for(%a = 0; %a < VehicleCache.itemCount; %a++)
		{
			%objA = VehicleCache.item[%a];
			if(%objA.getClassName() $= "WheeledVehicleData" || %objA.getClassName() $= "FlyingVehicleData")
				if(strPos(%objA.uiName, %vehicle) >= 0)
					return %objA.getName();
		}
	}

	return -1;
}

/////////////////////////////////////////////////////////////////////////////////////////

// registerOutputEvent(GameConnection, "Shop_Save");
// registerOutputEvent(GameConnection, "Shop_Load");
// registerOutputEvent(GameConnection, "Shop_AutoLoad");

// registerOutputEvent(GameConnection,"RequestItem", "datablock itemData", "int 1 10 3");
// registerOutputEvent(GameConnection,"DisplayItem", "datablock itemData" TAB "int 1 10 3");

function GameConnection::Shop_AutoLoad(%this)
{
	%this.autoLoadMode = !%this.autoLoadMode;
	%this.chatMessage("\c6Autoload is now " @ (%this.autoLoadMode ? "\c2ON\c6. You will now auto load weapons when you spawn next time and beyond." 
		: "\c0OFF\c6. You will no longer auto load weapons when you spawn and beyond."));

	if(%this.Shop_Client)
		commandToClient(%this, 'DRShop', "SET_AUTOLOADMODE", %this.autoLoadMode);
}

function GameConnection::Shop_Save(%this)
{
	if(!isObject(%pl = %this.player)) return;
	commandToClient(%this,'MessageBoxYesNo',"Save your current items?",
		"Are you sure you want to save your current loadout?",
		'Shop_ConfirmSave');
}

function GameConnection::Shop_Load(%this)
{
	if(!isObject(%pl = %this.player)) return;
	commandToClient(%this,'MessageBoxYesNo',"Load your saved items?",
		"Are you sure you want to load your saved items?",
		'Shop_ConfirmLoad');
}

function GameConnection::Shop_Check(%this)
{
	if(!%this.Shop_Client && !%this.Shop_OutOfDate)
	{
		echo(%this.getPlayerName() @ " does not have the shop client.");
		%this.chatMessage("\c6You currently do not have the GUI for this server. It is optional, but it would make things easier for you\c6.");
		%this.chatMessage("\c6You can download it <a:" @ $Shop::Link @ ">here</a> if you are interested\c6.");
	}
	else
		echo(%this.getPlayerName() @ " has the shop client, and updated.");
}

////////////////////////////////////////////////////////////////////////////////////////////

function GameConnection::Shop_Spawn(%this)
{
	cancel(%this.shopSch);
	if(!isObject(%player = %this.player))
		return;

	if(%player.getState() $= "dead")
		return;

	if(%this.minigame.noitems)
	{
		%player.clearTools();
		return;
	}
	
	%this.DRInventoryUI_push("Spawn");
	%player.Shop_LoadList(%this.dataInstasnce($DR::SaveSlot).LastLoadOut);
}

function GameConnection::Shop_SetData(%this)
{
	%this.Shop_Password = getRandom(1000, 1000000); //Who cares, just make it complicated.
	commandToClient(%this, 'DRShop', "Handshake", %this.Shop_Password);
}

function GameConnection::Shop_SendSingleData(%this, %obj)
{
	if(!isObject(%group = getDRShopGroup()))
		return;

	if(!%group.isMember(%obj))
		return;
					
	if(%obj.uiName !$= "")
	{
		%strName = getSafeVariableName(%obj.uiName);

		%adminLevel = 0 + %this.isSuperAdmin + %this.isAdmin;
		if(%adminLevel >= %obj.adminLevel)
		{
			//Tricky part
			commandToClient(%this,'DRShop', "ADD", 
				%obj.uiName TAB %obj.shopClass,
				%obj.cost,
				%obj.description,
				mFloor(%obj.damage) TAB mFloor(%obj.radiusDamage),
				%obj.image TAB %obj.imageColor,
				mFloor(%this.dataInstance($DR::SaveSlot).boughtItem[%strName]) TAB mFloor(%obj.buyOnce),
				mFloor(%obj.adminLevel) TAB mFloor(%obj.canSave) TAB mFloor(%obj.cannotModify) TAB %obj.shopClass,
				(%obj.datablockName $= "" ? nameToID(%obj) : nameToID(findItemByName(%obj.datablockName))));
		}
	}
}

function GameConnection::Shop_SendData(%this, %avoidReset)
{
	if(!isObject(%group = getDRShopGroup()))
		return;

	if(%group.getCount() <= 0)
		return;

	if(!%avoidReset)
		commandToClient(%this, 'DRShop', "CleanSlate");

	%this.sendMapData();
	%this.sendAchievements(1);
	%this.sendTitles(1);
	%this.loadTitle();
	%this.sendLeaderboard();

	commandToClient(%this, 'DRShop', "SetProfile", "Rank", %this.getLeaderboardRank());
	commandToClient(%this, 'DRShop', "SetProfile", "Leaderboard", %this.getLeaderboardNumber());

	commandToClient(%this, 'DRShop', "SetSetting", "HUD", %this.DR_HUD);
	commandToClient(%this, 'DRShop', "SetSetting", "GUIHUD", %this.DR_GUIHUD);
	commandToClient(%this, 'DRShop', "SetSetting", "GUIHUDPassenger", %this.DR_HUDPassenger);
	commandToClient(%this, 'DRShop', "SetSetting", "GUIRound", %this.dataInstance($DR::SaveSlot).DR_GUIPerRound);
	commandToClient(%this, 'DRShop', "SetSetting", "MapVoteGUI", %this.dataInstance($DR::SaveSlot).DR_MapGUI);
	
	%boughtItems = 0;					
	for(%i = 0; %i < %group.getCount(); %i++)
	{
		%obj = %group.getObject(%i);

		if(%obj.uiName !$= "")
		{
			%strName = getSafeVariableName(%obj.uiName);
			%bought = mFloor(%this.dataInstance($DR::SaveSlot).boughtItem[%strName]);
			if(%bought)
				%boughtItems++;

			%adminLevel = 0 + %this.isSuperAdmin + %this.isAdmin;
			if(%adminLevel >= %obj.adminLevel)
			{
				//Tricky part
				commandToClient(%this,'DRShop', "ADD", 
					%obj.uiName TAB %obj.shopClass,
					%obj.cost,
					%obj.description,
					mFloor(%obj.damage) TAB mFloor(%obj.radiusDamage),
					%obj.image TAB %obj.imageColor,
					%bought TAB mFloor(%obj.buyOnce),
					mFloor(%obj.adminLevel) TAB mFloor(%obj.canSave) TAB mFloor(%obj.cannotModify) TAB %obj.shopClass,
					(%obj.datablockName $= "" ? nameToID(%obj) : nameToID(findItemByName(%obj.datablockName))));
			}
		}
	}

	//if(%this.dataInstance($DR::SaveSlot).DR_totalItemsBought $= "")
		%this.dataInstance($DR::SaveSlot).DR_totalItemsBought = %boughtItems;
}

function GameConnection::PingCheck(%this)
{
	if(%this.isPinging)
		return;

	%this.pingTries = 0;
	%this.isPinging = 1;
	%this.schedule(100, PingCheck2);
}

function GameConnection::PingCheck2(%this)
{
	%ping = %this.getPing();
	if(%ping > 2500)
	{
		%somethingsWrong = 1;
		%reason = "Ping too high";
	}

	if(%ping <= 0)
	{
		%somethingsWrong = 1;
		%reason = "Ping too low";
	}
	if(%somethingsWrong)
		%this.schedule(500, onPingCheckFailed, %reason);
	else
		%this.schedule(500, onPingCheckSuccessful);
}

function GameConnection::onPingCheckFailed(%this, %reason)
{
	%this.isPinging = 0;
	if(trim(%reason) !$= "")
		%this.chatMessage("Error: Ping could not be checked. (" @ %reason @ ") - Retrying...");

	%this.pingTries++;
	%this.schedule(1000, PingCheck2);
}

function GameConnection::onPingCheckSuccessful(%this)
{
	%this.isPinging = 0;
	%this.pingTries = 0;
	%ping = %this.getPing();
	%color = 1 / (500 * %ping);
	%this.chatMessage("\c6Your ping is <color:" @ rgbToHex(redToGreen(%color)) @ ">" @ %ping @ "\c6ms");
	if(!%this.hasspawnedonce)
		%this.schedule(1000, Shop_Check);
}