$Shop::Chars = "`~!@#^&*-=+{}\\|;:\'\",<>/?[].";

if(!isObject(DRShopGroup))
{
	new ScriptGroup(DRShopGroup)
	{
		class = DRShopSO;
		filePath = "config/server/Shop/";
	};
	DRShopGroup.schedule(1000, Load);
}
else
{
	DRShopGroup.schedule(1000, Load);
}

function GuiTextListCtrl::findTextIndexHack(%gui, %find)
{
	%c = %gui.rowCount();
	if(%c == 0)
		return -1;

	for(%i = 0; %i < %c; %i++)
	{
		if(%find $= getField(%gui.getRowText(%i), 0))
			return %i;
	}

	return -1;
}

function DRShopSO::getShopFieldList(%this, %ShopTree)
{
	for(%i=0;%i<%this.getCount();%i++)
	{
		%Shop = %this.getObject(%i);
		if(strReplace(%ShopTree, " ", "_") $= %Shop.getTreeParsed())
			%list = %list TAB %Shop.uiName;
	}
	%list = trim(%list);
	return %list;
}

function DRShopSO::findScriptInTree(%this, %tree, %ShopName)
{
	%list = %this.getShopFieldList(%tree);
	if(getFieldCount(%list) <= 0)
		return -1;

	for(%i=0;%i<getFieldCount(%list);%i++)
	{
		%obj = %this.findScript(getField(%list, %i));
		if(nameToID(%ShopName) == nameToID(%obj))
			return %obj;
		else if(%obj.uiName $= %ShopName)
			return %obj;
		else if(strPos(%obj.uiName, %ShopName) == 0)
			return %obj;
	}
	return -1;
}

function DRShopSO::findScript(%this, %ShopName)
{
	//Agh got to make everything complicated
	for(%i=0;%i<%this.getCount();%i++)
	{
		%obj = %this.getObject(%i);
		if(nameToID(%ShopName) == nameToID(%obj))
			return %obj;
	}

	for(%i=0;%i<%this.getCount();%i++)
	{
		%obj = %this.getObject(%i);
		if(%obj.uiName $= %ShopName)
			return %obj;
	}

	for(%i=0;%i<%this.getCount();%i++)
	{
		%obj = %this.getObject(%i);
		if(strPos(%obj.uiName, %ShopName) == 0)
			return %obj;
	}
	return -1;
}

/// <summary>
/// Loads the Shop program.
/// </summary>
function DRShopSO::Load(%this)
{
	//--PARAMETERS - READ CAREFULLY--\\
	
	//Description: This will display the current Shop fields it can support.

	//--KEY--\\
	//	//_!	<- Required field, otherwise it will be confused
	//	//->	<- Normal
	//	//-?	<- Can depend on things
	//	-/- 	<- Comment, do not copy that part if you plan to

	

	//--YOU MAY USE FUNCTIONS, HERE IS HOW IT GOES--\\
	//	CMD function(args...);

	%this.deleteAll();
	if(!isObject(%this.shopCategories))
		%this.shopCategories = new GuiTextListCtrl();

	%this.shopCategories.clear();

	%path = %this.filePath @ "*";
	if(getFileCount(%path) <= 0)
	{
		echo("ERROR - No weapons exist in path -> " @ %path);
		announce("ERROR - No weapons exist in path -> " @ %path);
		return;
	}

	//announce("Loading saved files for mob classes. -> Path: " @ %path);
	%file = findFirstFile(%path);
	if(isFile(%file))
	{
		%fileExt = fileExt(%file);
		%name = fileBase(%file);
		if(%fileExt $= ".cs") //Just making sure
		{
			if(isObject(%obj = isRegisteredDRShopItem(fileBase(%path))))
				%obj.delete();

			exec(%file);
		}
	}
	else
		return;

	while(%file !$= "")
	{
		%file = findNextFile(%path);
		%fileExt = fileExt(%file);
		%name = fileBase(%file);
		if(%fileExt $= ".cs") //Just making sure
		{
			if(isObject(%obj = isRegisteredDRShopItem(fileBase(%path))))
				%obj.delete();

			exec(%file);
		}
	}

	if($Trails::TrailCount > 0)
	{
		// registerDRShopItem("Trail - None", 
		// 		"shopClass Player Trails" TAB
		// 		"cannotModify 1" TAB
		// 		"buyOnce 1" TAB
		// 		"cost 0" TAB
		// 		"description Use '/Trail " @ %name @ "' to set your player trail. This takes off your trail." TAB
		// 		"func Shop_TrailUpgrades" TAB
		// 		"func_call " @ %name);

		for(%i = 0; %i < $Trails::TrailCount; %i++)
		{
			%name = $Trails::TrailDisplayName[%i];
			registerDRShopItem("Trail - " @ %name, 
				"shopClass Player Trails" TAB
				"cannotModify 1" TAB
				"buyOnce 1" TAB
				"cost 60" TAB
				"description Use '/Trail " @ %name @ "' to set your player trail. Equip again to remove trail or /trail None. Permanent perchase." TAB
				"func Shop_TrailUpgrades" TAB
				"func_call " @ %name);
		}
	}

	registerDRShopItem("Round - Crazy Speed", 
		"shopClass Minigame Specials" TAB
		"cannotModify 1" TAB
		"cost 6" TAB
		"description The next round will have faster jeeps. 1 round use." TAB
		"func Shop_MiniSpecial" TAB
		"func_call CrazySpeed");

	registerDRShopItem("Round - Bigger Vehicles", 
		"shopClass Minigame Specials" TAB
		"cannotModify 1" TAB
		"cost 5" TAB
		"description The next round will have bigger jeeps. 1 round use." TAB
		"func Shop_MiniSpecial" TAB
		"func_call VehicleScale");

	registerDRShopItem("Round - Horse", 
		"shopClass Minigame Specials" TAB
		"cannotModify 1" TAB
		"cost 7" TAB
		"description The next round will turn everyone into a faster horse. 1 round use." TAB
		"func Shop_MiniSpecial" TAB
		"func_call Horse");

	registerDRShopItem("Post-Apoc Fordor", 
		"shopClass Vehicles" TAB
		"cannotModify 1" TAB
		"cost 22" TAB
		"description Sets the vehicle to Post-Apoc Fordor." TAB
		"func Shop_VehicleUpgrades" TAB
		"func_call Post-Apoc Fordor");

	registerDRShopItem("Post-Apoc Pickup", 
		"shopClass Vehicles" TAB
		"cannotModify 1" TAB
		"cost 18" TAB
		"description Sets the vehicle to Post-Apoc Pickup." TAB
		"func Shop_VehicleUpgrades" TAB
		"func_call Post-Apoc Pickup");

	registerDRShopItem("Skateboard", 
		"shopClass Vehicles" TAB
		"cannotModify 1" TAB
		"cost 1" TAB
		"adminLevel 1" TAB
		"description Want to ride a skateboard? Go ahead." TAB
		"func Shop_VehicleUpgrades" TAB
		"func_call Skateboard");
       
        registerDRShopItem("Euro Turbo", 
		"shopClass Vehicles" TAB
		"cannotModify 1" TAB
		"cost 3" TAB
		"adminLevel 1" TAB
		"description Sets the car to Euro Turbo. Nothing really special about this car, it just looks cool." TAB
		"func Shop_VehicleUpgrades" TAB
		"func_call Euro Turbo");

	registerDRShopItem("Jeep", 
		"shopClass Vehicles" TAB
		"cannotModify 1" TAB
		"cost 1" TAB
		"description Are you used to the Jeep? Go ahead and try me." TAB
		"func Shop_VehicleUpgrades" TAB
		"func_call Jeep");

	registerDRShopItem("Phantom", 
		"shopClass Vehicles" TAB
		"cannotModify 1" TAB
		"cost 2" TAB
		"description Ghost!! Haha kidding, just another cool vehicle." TAB
		"func Shop_VehicleUpgrades" TAB
		"func_call Phantom");

	registerDRShopItem("Oldtimer", 
		"shopClass Vehicles" TAB
		"cannotModify 1" TAB
		"cost 5" TAB
		"description Want to grow old, pick the Oldtimer." TAB
		"func Shop_VehicleUpgrades" TAB
		"func_call Oldtimer");

	registerDRShopItem("Tank", 
		"shopClass Vehicles" TAB
		"cannotModify 1" TAB
		"cost 35" TAB
		"description Tanks can be very overpowered, even includes a turret. This is not the default tank." TAB
		"func Shop_VehicleUpgrades" TAB
		"func_call Tank");

	registerDRShopItem("Invisible vehicle", 
		"shopClass Specials" TAB
		"cannotModify 1" TAB
		"cost 5" TAB
		"description Not much of a benefit, just something interesting." TAB
		"func Shop_VehicleUpgrades" TAB
		"func_call Invisible");

	registerDRShopItem("Max Health", 
		"shopClass Player Upgrades" TAB
		"cannotModify 1" TAB
		"cost 2" TAB
		"description Add player max health by 10." TAB
		"func Shop_PlayerUpgrades" TAB
		"func_call Health");

	registerDRShopItem("Driving bot", 
		"shopClass Specials" TAB
		"cannotModify 1" TAB
		"cost 0" TAB
		"description Let a bot drive you!" TAB
		"func Shop_Bots" TAB
		"func_call Driver" TAB
		"adminLevel 2");
}

/// <summary>
/// When the Shop is created, we need to format the variables, so we put it into a command, see -> registerShop
/// Do not use this function.
/// </summary>
/// <param name="this">Name of the created mob.</param>
/// <param name="com">Parameters, each variable must be in a different field.</param>
function DRShopItem::onAdd(%this)
{
	DRShopGroup.add(%this);
	%categoryList = DRShopGroup.shopCategories;
	if(isObject(%categoryList))
	{
		%idx = %categoryList.findTextIndexHack(%this.shopClass);
		if(%idx != -1) // index? add it
			%categoryList.setRowById(%idx, %this.shopClass TAB getField(%categoryList.getRowText(%id), 1)+1);
		else
			%categoryList.addRow(%categoryList.rowCount(), %this.shopClass TAB 1, %categoryList.rowCount());

		cancel(%categoryList.sortSch);
		%categoryList.sortSch = %categoryList.schedule(100, 0, sort, 0);
		%categoryList.sort(0);
	}

	%this.parseCommand(%this.command);
	cancel($SendShopDataSch);
	$SendShopDataSch = schedule(1000, 0, Shop_SendDataToAllClients, 1);
}

function DRShopItem::getTreeParsed(%this)
{
	return strReplace(%this.tree, " ", "_");
}

/// <summary>
/// Parses Shop objects' commands, see -> Shop::onAdd
/// Do not use this function.
/// </summary>
/// <param name="this">Name of the created mob.</param>
/// <param name="com">Parameters, each variable must be in a different field.</param>
function DRShopItem::parseCommand(%this, %com)
{
	if(%com $= "")
		return;

	//echo("       -> Parasing Shop command line: " @ %this.uiName);
	//echo("       -> CommandLine: " @ %com);
	for(%i=0;%i<getFieldCount(%com);%i++)
	{
		%field = getField(%com, %i);
		%name = getWord(%field, 0);
		%value = collapseEscape(getWords(%field, 1, getWordCount(%field)-1));

		if(%name $= "shopClass")
		{
			for(%a=0;%a<getFieldCount($ShopTrees);%a++)
				if(getField($ShopTrees, %a) $= %value)
					%remove = 1;

			if(!%remove)
				$ShopTrees = %value TAB $ShopTrees;
		}

		//echo("         PARAMETER FOUND: " @ %cmd);
		//echo("             VALUE: " @ %value);
		%cmd = %this @ "." @ %name @ " = \"" @ %value @ "\";";
		//echo("             PARSED: " @ %cmd);

		//This may look like an exploit but there's no other way of doing this.
		//Either way this shouldn't hurt because it's all server-side.
		eval(%cmd);
	}

	%this.command = "";

	%datablock = findItemByName(%this.datablockName);
	//We will force stuff.
	if(isObject(%datablock) && %datablock.getClassName() $= "ItemData")
	{
		if(isObject(%proj = %datablock.image.projectile))
			%intDamage = %proj.directDamage SPC %proj.explosion.radiusDamage;

		%this.damage = mFloor(getWord(%intDamage, 0));
		%this.radiusDamage = mFloor(getWord(%intDamage, 1));
		%this.image = %datablock.iconName;
		%this.imageColor = %datablock.colorShiftColor;
		%this.save(%this.getGroup().filePath @ stripChars(%this.uiName, $Shop::Chars) @ ".cs");
	}
}

/// <summary>
/// Registers a Shop into the DRShopSO program. Easy to see
/// </summary>
/// <param name="name">Name of the created mob.</param>
/// <param name="parm">Parameters, each variable must be in a different field.</param>
function registerDRShopItem(%name, %parm)
{
	%strName = stripChars(%name, $Shop::Chars);
	%strName = strReplace(%strName, " ", "_");
	%objName = "DRShop_" @ %strName;
	//echo("Registering a Shop.. - " @ %name @ " (" @ %objName @ ")");
	for(%i=0;%i<getFieldCount(%parm);%i++)
	{
		%field = getField(%parm,%i);
		%var = getWord(%field,0);
		%value = getWords(%field, 1, getWordCount(%field)-1);
		//echo("   PARAMETER FOUND: " @ %var);
		//echo("     VALUE: " @ %value);

		//for(%a=0;%a<getWordCount($City::DRShopSO_RequiredFields);%a++)
		//{
		//	%requirement = getWord($City::DRShopSO_RequiredFields,%a);
		//	if(%var $= %requirement && !%met_[%requirement])
		//	{
		//		%met_[%requirement] = 1;
		//		%metCount++;
		//	}
		//}
	}

	//if(%metCount < getWordCount($City::DRShopSO_RequiredFields))
	//{
	//	warn(" - Unable to add the Shop. Make sure you have made the parameters correctly.");
	//	warn(" - Requirement amount: " @ mFloor(%metCount) @ "/" @ getWordCount($City::DRShopSO_RequiredFields));
	//	return;
	//}

	if(isObject(%objName))
	{
		warn("Warning: Shop data \"" @ %objName @ "\" already exists. Overwriting.");
		%categoryList = DRShopGroup.shopCategories;
		if(isObject(%categoryList))
		{
			%idx = %categoryList.findTextIndexHack(%objName.shopClass);
			%count = getField(%categoryList.getRowText(%id), 1)-1;
			if(%idx != -1) // index? remove it
				%categoryList.setRowById(%idx, %objName.shopClass TAB %count);

			if(%count <= 0)
				%categoryList.removeRow(%idx);

			%categoryList.sort(0);
		}

		%objName.delete();
	}

	%obj = new ScriptObject(%objName)
	{
		class = "DRShopItem";
		uiName = %name;
		command = collapseEscape(%parm);
	};

	return %obj;
}

/// <summary>
/// Returns whether the Shop exists or not.
/// </summary>
/// <param name="Shop">Shop object or name.</param>
function isRegisteredDRShopItem(%Shop)
{
	return DRShopGroup.findScript(%Shop);
}

function Shop_SendSingleDataToAllClients(%obj)
{
	if(!isObject(%obj))
		return;
	
	if(!DRShopGroup.isMember(%obj))
		return;

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		%client.Shop_SendSingleData(%obj);
	}
}

function Shop_SendDataToAllClients()
{
	cancel($SendShopDataSch);

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		%client.Shop_SendData();
	}
}

function getDRShopGroup()
{
	return nameToID(DRShopGroup);
}

function Shop_LoadOldData()
{
	%filePath = "config/server/VShop/Clients/*.cs";

	if(getFileCount(%filePath) <= 0)
	{
		announce("No such file directory - " @ %filePath);
		return;
	}

	talk("Loading " @ getFileCount(%filePath) @ " old weapon data...");
	for(%file = findFirstFile(%filePath); %file !$= ""; %file = findNextFile(%filePath))
	{
		%fileExt = fileExt(%file);
		if(%fileExt !$= ".cs")
			continue;

		exec(%file);
	}
}

if(!$Server::Shop::OldData)
{
	$Server::Shop::OldData = 1;
	schedule(0, 0, Shop_LoadOldData);
}

if(isFile("config/server/ShopBuyPrefs.cs"))
	exec("config/server/ShopBuyPrefs.cs");