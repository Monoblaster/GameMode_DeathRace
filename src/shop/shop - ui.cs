$DR::DefaultLoadout = "EMPTYSLOT PistolItem L4BCookingKnifeItem EMPTYSLOT EMPTYSLOT EMPTYSLOT EMPTYSLOT";
//inventory ui
datablock ItemData(DRMenuPrimary)
{
	uiname = "Primary";
	iconName = BattleRifleItem.iconName;
	doColorShift = true;
	colorShiftColor = "0 0 0 1";
};

datablock ItemData(DRMenuSecondary)
{
	uiname = "Secondary";
	iconName = PistolItem.iconName;
	doColorShift = true;
	colorShiftColor = "0 0 0 1";
};

datablock ItemData(DRMenuMelee)
{
	uiname = "Melee";
	iconName = L4BCookingKnifeItem.iconName;
	doColorShift = true;
	colorShiftColor = "0 0 0 1";
};

datablock ItemData(DRMenuSupport)
{
	uiname = "Support";
	iconName = StimpackItem.iconName;
	doColorShift = true;
	colorShiftColor = "0 0 0 1";
};

datablock ItemData(DRMenuSwitch)
{
	uiname = "Switch";
	doColorShift = true;
	colorShiftColor = "1 1 0 1";
};

datablock ItemData(DRMenuReady)
{
	uiname = "Done";
	doColorShift = true;
	colorShiftColor = "1 0 1 1";
};

datablock ItemData(DRMenuSlotOne)
{
	uiname = "1";
};

datablock ItemData(DRMenuSlotTwo)
{
	uiname = "2";
};

datablock ItemData(DRMenuSlotThree)
{
	uiname = "3";
};

datablock ItemData(DRMenuNext)
{
	uiname = ">";
};

datablock ItemData(DRMenuNone)
{
	uiname = "None";
};

datablock ItemData(DRMenuNewJeep)
{
	uiname = "New Jeep";
	iconName = $Deathrace::Icons @ "newjeep";
};

datablock ItemData(DRMenuYes)
{
	uiname = "Yes";
};

datablock ItemData(DRMenuCancel)
{
	uiname = "Cancel";
};

datablock ItemData(DRMenuNo)
{
	uiname = "No";
};

datablock ItemData(DRMenuConfirm)
{
	uiname = "Confirm";
};

datablock ItemData(DRMenuEquipmentEditor)
{
	doColorShift = true;
	colorShiftColor = "1 1 0 1";
	uiname = "Loadout";
};

datablock ItemData(DRMenuShop)
{
	doColorShift = true;
	colorShiftColor = "1 0 0 1";
	uiname = "Shop";
};

datablock ItemData(DRMenuVehicles)
{
	doColorShift = true;
	colorShiftColor = "1 0 0 1";
	uiname = "Vehicles";
};

datablock ItemData(DRMenuTrails)
{
	doColorShift = true;
	colorShiftColor = "1 0 0 1";
	uiname = "Player Trails";
};

datablock ItemData(DRMenuMinigameSpecials)
{
	doColorShift = true;
	colorShiftColor = "1 0 0 1";
	uiname = "Minigame Specials";
};

datablock ItemData(DRMenuSpecials)
{
	doColorShift = true;
	colorShiftColor = "1 0 0 1";
	uiname = "Specials";
};

function DRMenu_Owns(%client,%shopobj)
{
	%name = %shopObj.func_call;
	if(%name $= "")
	{
		%strName = stripChars(%shopObj.uiName, $Shop::Chars);
		%strName = strReplace(%strName, " ", "_");
	}
	else
	{
		%strName = getSafeVariableName("Trail - " @ %shopObj.func_call);
	}
	
	return %client.dataInstance($DR::SaveSlot).boughtItem[%strName] || %client.bypassShop;
}

function ToolCost(%client,%item,%notRoundStart)
{
	if(!isObject(%item))
	{
		return 0;
	}

	%data = %client.dataInstance($DR::SaveSlot);
	%shopName = stripChars(strReplace(%item.uiname, " ", "_"), $Shop::Chars);
	%script = %item.shopobj;
	if(%notRoundStart && !%script.buyOnce)
	{
		return 0;
	}
	if(!%data.boughtItem[%shopName])
	{
		return %script.cost;
	}
	return 0;
}

function formatItem(%item,%c,%desc)
{
	%data = %c.dataInstance($DR::SaveSlot);
	%script = %item.shopobj;
	%name = %script.func_call;
	if(%name $= "")
	{
		%shopName = stripChars(%script.uiName, $Shop::Chars);
		%shopName = strReplace(%shopName, " ", "_");
	}
	else
	{
		%shopName = getSafeVariableName("Trail - " @ %script.func_call);
	}
	%append = %item.uiname;
	
	if(!DRMenu_Owns(%c,%script))
	{
		%cost = %script.cost + 0;
		%append = "\c7" @ %append SPC "-" SPC %cost SPC "points";
	}
	else
	{
		%append = "\c6" @ %append;
	}

	if(%desc)
	{
		%append = %append NL "\c6" @  %script.description;
	}

	return %append;
}

function formatItems(%list,%c,%desc,%purchaseInfo)
{
	%count = getWordCount(%list);
	if(%count == 0)
	{
		return "\c6Empty";
	}
	
	%data = %c.dataInstance($DR::SaveSlot);
	%totalCost = 0;
	%s = "";
	for(%i = 0; %i < %count; %i++)
	{
		%item = getWord(%list,%i);
		if(!isObject(%item))
		{
			continue;
		}
		%shopName = stripChars(strReplace(%item.uiname, " ", "_"), $Shop::Chars);
		if(!%data.boughtItem[%shopName])
		{
			%totalCost += item.shopobj.cost;
		}
		
		%s = %s NL formatItem(%item,%c,%desc,%purchaseInfo);
	}

	if(%totalCost > 0)
	{
		%s = %s NL "\c5Costs" SPC %totalCost SPC "points every round";
	}
	return trim(%s);
}


function DRMenu_PayPreset(%client)
{
	%player = %client.player;
	if(!isObject(%player))
	{
		return;
	}

	%totalcost = 0;
	%count = %client.getMaxTools();
	for(%i = 0; %i < %count; %i++)
	{
		%item = %player.tool[%i];
		if(!isobject(%item))
		{
			continue;
		}

		%cost = ToolCost(%client,%item);
		if(%cost > 0)
		{
			%itemcostlist = %itemcostlist TAB %item.uiname;
			%totalCost += %cost;
		}
	}
	%itemcoststring = stringList(ltrim(%itemcostlist),"\t","\c6,\c3","\c6and\c3");
	%client.setscore(%client.score-%totalcost);

	if(%totalcost == 1)
	{
		%client.chatMessage("\c6You payed a single point to use the\c3" SPC %itemcoststring @ "\c6.");
	}
	else if(%totalcost > 1)
	{
		%client.chatMessage("\c6You payed"SPC %totalcost SPC"points to use the\c3" SPC %itemcoststring @ "\c6.");
	}
}

function DRMenu_EquipPreset(%client)
{
	%player = %client.player;
	if(!isObject(%player))
	{
		return;
	}

	%data = %client.dataInstance($DR::SaveSlot);
	%equippresetindex = %data.EquipPresetIndex + 0;
	%preset = %client.EquipPreset[%equippresetindex + 0];

	%count = %client.getMaxTools();
	for(%i = 0; %i < %count; %i++)
	{
		%player.tool[%i] = "";
	}
	
	%equipcount = 0;
	%totalcost  = 0;
	%score = %client.score;
	%count = %preset.Count();
	for(%i = 0; %i < %count; %i++)
	{
		%item = %preset.get(%i);
		if(!isObject(%item))
		{
			continue;
		}

		%cost = ToolCost(%client,%item);
		%score -= %cost;
		if(%score < 0 && %cost > 0)
		{
			%client.chatMessage("You cannot afford to use the\c5" SPC %item.uiname SPC "\c0anymore.");
			continue;
		}

		if(%cost > 0)
		{
			%totalcost += %cost;
			%itemcostlist = %itemcostlist TAB %item.uiname;
		}

		%player.tool[%equipcount] = %item.getid();
		%equipcount++;
	}
	%itemcoststring = stringList(ltrim(%itemcostlist),"\t","\c6,\c3","\c6and\c3");

	if(%totalcost == 1)
	{
		%client.chatMessage("\c6You will be paying a single point \c0every round\c6 to use the\c3" SPC %itemcoststring @ "\c6.");
	}
	else if(%totalcost > 1)
	{
		%client.chatMessage("\c6You will be paying"SPC %totalcost SPC"points \c0every round\c6 to use the\c3" SPC %itemcoststring @ "\c6.");
	}
}

function DRMenu_SetPreset(%client,%slot,%item)
{
	%data = %client.dataInstance($DR::SaveSlot);
	%equippresetindex = %data.EquipPresetIndex + 0;
	%preset = %client.EquipPreset[%equippresetindex + 0];

	%preset.set(%slot,%item);
	%s = "";
	%count = %client.getMaxTools();
	for(%i = 0; %i < %count; %i++)
	{
		%tool = %preset.get(%i);
		if(isObject(%tool))
		{
			%shopObj = getDRShopGroup().findScript(%tool.uiName);
			%shopObj.canSave = 1;
			%s = %s SPC %tool.getName();
		}
		else
		{
			%s = %s SPC "EMPTYSLOT";
		}
	}
	%data.savedLoadout[%equippresetindex] = trim(%s);
}

function DRMenu_EquipmentDisplay(%stack,%slot)
{
	%client = %stack.client;
	%data = %client.dataInstance($DR::SaveSlot);
	%equippresetindex = %data.EquipPresetIndex + 0;
	%stack.peek(0).display(%stack.client,true);
	%preset = %client.EquipPreset[%equippresetindex];
	if(!isObject(%preset))
	{
		%loadout = %client.dataInstance($DR::SaveSlot).savedLoadout[%equippresetindex];
		%count = getWordCount(%loadout);
		%preset = Inventory_Create();
		%client.EquipPreset[%equippresetindex] = %preset;
		for(%i = 0; %i < %count; %i++)
		{
			%preset.set(%i,getWord(%loadout,%i));
		}
	}
	%preset.display(%client);
}

function DRMenu_EquipmentSelect(%stack,%slot)
{
	%client = %stack.client;
	%c = -1;
	%s = "";
	switch(%slot)
	{
	case %c++:
		%s = "Switch your primary weapon";
	case %c++:
		%s = "Switch your secondary weapon";
	case %c++:
		%s = "Switch your melee weapon";
	case %c++:
		%s = "Switch your support item";
	case %c++:
		%s = "Switch your current loadout";
	}
	%s = "<font:palatino linotype:24>\c3" @ %s;
	
	%preset = %client.EquipPreset[%client.dataInstance($DR::SaveSlot).EquipPresetIndex + 0];
	%item = %preset.get(%slot);
	%itemname = "";
	%description = "";
	if(isObject(%item))
	{
		%itemname = %item.uiname;
		%description = %item.shopobj.description;
	}

	return "<font:impact:30>\c6" @ %itemname NL "<font:palatino linotype:22>\c7" @ %description NL "" NL  %s;
}

function DRMenu_EquipmentUse(%stack,%slot)
{
	%client = %stack.client;
	%selected = %stack.peek(0).tool[%slot];
	%shopclass = %selected.uiname;
	if(%selected $= "DRMenuSwitch")
	{
		%stack.push("Switch");
	}
	else
	{
		DRMenu_ShopListOpen(%stack,%slot,%shopclass,DRMenuNone,"Empties this slot","Buy and equip","Borrow and pay at round start","Equip");
	}
}

function DRMenu_EquipmentPop(%stack,%slot)
{
	%client = %stack.client;
	DRMenu_EquipPreset(%client);
}

function DRMenu_ShopSelect(%stack,%slot)
{
	%client = %stack.client;
	switch$(%stack.peek(0).get(%slot))
	{
	case "DRMenuVehicles":
		%s = "Change what vehicle you are driving";
	case "DRMenuTrails":
		%s = "Buy and equip trails";
	case "DRMenuMinigameSpecials":
		%s = "Buy special rounds";
	}

	return "\c3" @ %s;
}

function DRMenu_ShopUse(%stack,%slot)
{
	%client = %stack.client;
	%selected = %stack.peek(0).get(%slot);
	%shopclass = %selected.uiname;

	switch$(%stack.peek(0).get(%slot))
	{
	case "DRMenuVehicles":
		%icon = DRMenuNewJeep;
		%desc = "Returns to the normal jeep";
		%borrowblurb = "Pay and switch";
	case "DRMenuTrails":
		%icon = DRMenuNone;
		%desc = "Select no trail";
		%buyBlurb = "Buy and equip";
		%equipBlurb = "Equip";
	case "DRMenuMinigameSpecials":
	}
	DRMenu_ShopListOpen(%stack,%slot,%shopclass,%icon,%desc,%buyBlurb,%borrowBlurb,%equipBlurb);
}

function DRMenu_ShopListOpen(%stack,%slot,%class,%defaultOption,%defaultDescription,%buyBlurb,%borrowBlurb,%equipBlurb)
{
	%client = %stack.client;
	%client.DRMenu_ShopListClass = %class;
	%client.DRMenu_ShopListSlot = %slot;
	%client.DRMenu_ShopListDefault = %defaultOption;
	%client.DRMenu_ShopListDefaultDesc = %defaultDescription;
	%client.DRMenu_ShopListBuyBlurb = %buyBlurb;
	%client.DRMenu_ShopListBorrowBlurb = %borrowBlurb;
	%client.DRMenu_ShopListEquipBlurb = %equipBlurb;
	%stack.push("ShopList");
}

function DRMenu_ShopListDisplay(%stack,%slot)
{
	%client = %stack.client;
	%adminLevel = 0 + %client.isSuperAdmin + %client.isAdmin;
	%client.DRMenu_ShopListOffset = 0;

	%inv = Inventory_Create();
	if(isObject(%client.DRMenu_ShopList))
	{
		%client.DRMenu_ShopList.delete();
	}
	%client.DRMenu_ShopList = %inv;
	%inv.set(0,%client.DRMenu_ShopListDefault);

	%shopclass = %client.DRMenu_ShopListClass;
	%list = getDRShopGroup().priceSorted[%shopclass];
	%count = getWordCount(%list);
	for(%i = 0; %i < %count; %i++)
	{
		%shopObj = getWord(%list,%i);
		if(%adminLevel < %shopObj.adminLevel)
		{
			continue;
		}
		%inv.set(%inv.Count(),%shopobj.item);
	}
	%inv.display(%client,true,-1);
	%stack.peek(0).display(%client,false);
}

function DRMenu_ShopListSelect(%stack,%slot)
{
	%client = %stack.client;
	%shopSlot = %slot + %client.DRMenu_ShopListOffset - 1;
	%item = %client.DRMenu_ShopList.get(%shopSlot);
	if(%slot == 0)
	{
		return "\c3Next Page";
	}

	if(isObject(%item))
	{
		if(isObject(%client.DRMenu_ShopListDefault) && %shopSlot == 0)
		{
			return "\c3" @ %client.DRMenu_ShopListDefaultDesc;
		}
		%shopObj = %item.shopobj;
		%itemname = %item.uiname;
		
		if(DRMenu_Owns(%client,%shopObj))
		{
			%namecolor = "\c2";
			%action = %client.DRMenu_ShopListEquipBlurb;
		}
		else
		{
			if(%shopobj.cost > %client.score)
			{
				%namecolor = "\c0";
				%action = "Cannot afford";
				if(%shopObj.buyOnce)
				{
					%price = "\c7-\c3" SPC %shopObj.cost;
				}
				else
				{
					%price = "\c7-\c3" SPC %shopObj.cost SPC "\c0per round";
				}
			}
			else
			{
				%namecolor = "\c6";
				if(%shopObj.buyOnce)
				{
					%price = "\c7-\c3" SPC %shopObj.cost;
					%action = %client.DRMenu_ShopListBuyBlurb;
				}
				else
				{
					%price = "\c7-\c3" SPC %shopObj.cost SPC "\c0per round";
					%action = %client.DRMenu_ShopListBorrowBlurb;
				}
			}
		}
		%description = %shopObj.description;

		return "<font:impact:30>" @ %namecolor @ %itemname SPC %price NL "<font:palatino linotype:22>\c7" @ %description NL "" NL "<font:palatino linotype:24>\c3" @ %action;
	}
	return "";
}

function DRMenu_ShopListUse(%stack,%slot)
{
	%client = %stack.client;
	if(%slot == 0)
	{
		%client.DRMenu_ShopListOffset += %client.getMaxTools() - 1;
		if(!isObject(%client.DRMenu_ShopList.get(%client.DRMenu_ShopListOffset + 1)))
		{
			%client.DRMenu_ShopListOffset = 0;
		}
		%client.DRMenu_ShopList.display(%client,true,%client.DRMenu_ShopListOffset - 1);
		%stack.peek(0).display(%stack.client);
		return;
	}

	%shopSlot = %slot + %client.DRMenu_ShopListOffset -1;
	%item = %client.DRMenu_ShopList.get(%shopslot);
	%class = %client.DRMenu_ShopListClass;
	if(isObject(%item))
	{
		%item = %item.getid();
		%shopobj = %item.shopobj;
		if(isObject(%shopObj) && !DRMenu_Owns(%client,%shopObj))
		{
			%func = "DRMenu_ShopBuy";
			%client.DRMenu_OnYes = %func;
			%client.DRMenu_ShopObj = %shopObj;
			%client.DRMenu_Message = "\c6Do you want to purchase this?" NL formatItem(%item,%client,true,true);
			%stack.push("Confirm");
			return;
		}

		DRMenu_ShopEquip(%client,%class,%shopObj);
		return;
	}
}

function DRMenu_ShopBuy(%client,%class,%shopObj)
{
	if(!isObject(%shopObj))
	{
		%client.chatMessage("\c6Invalid shop item (\"\c3" @ %item @ "\c6\"). Please contact your super administrator.");
		return false;
	}

	%cost = %shopobj.cost;
	if(%cost > %client.score)
	{
		%client.chatMessage("Cannot afford");
		return false;
	}	

	switch$(%class)
	{
	case "Primary" or "Secondary" or "Melee" or "Support":
		if(%shopobj.buyOnce)
		{
			%client.incScore(-%cost);
		}
		%client.BuyItem(%shopObj.item);
	case "Vehicles":
		if(isObject(%shopObj))
		{
			if(%client.Shop_VehicleUpgrades(%shopObj.func_call))
			{
				%client.incScore(-%cost);
			}
		}
		else
		{
			%client.Shop_VehicleUpgrades("NewJeep");
		}
		
	case "Player Trails":
		if(isObject(%shopObj))
		{
			%client.incScore(-%cost);
			%client.Shop_TrailUpgrades(%shopObj.func_call);
		}
	case "Minigame Specials":
		if(isObject(%shopObj))
		{
			if(%client.Shop_MiniSpecial(%shopObj.func_call))
			{
				%client.incScore(-%cost);
			}
		}
	}

	DRMenu_ShopEquip(%client,%class,%shopObj);
	return true;
}

function DRMenu_ShopEquip(%client,%class,%shopObj)
{
	switch$(%class)
	{
	case "Primary" or "Secondary" or "Melee" or "Support":
		DRMenu_SetPreset(%client,%client.DRMenu_ShopListSlot,%shopObj.item);
	case "Vehicles":
	case "Player Trails":
		if(isObject(%shopObj))
		{
			serverCmdTrail(%client,%shopObj.func_call);
		}
		else
		{
			serverCmdTrail(%client,"None");
		}
	case "Minigame Specials":
	}
	%client.InventoryStack.pop();
	return true;
}

function DRMenu_ConfirmSelect(%stack,%slot)
{
	%client = %stack.client;
	return %client.DRMenu_Message;
}

function DRMenu_ConfirmUse(%stack,%slot)
{
	%client = %stack.client;
	%func = %client.DRMenu_OnYes;
	if(isFunction(%func))
	{
		call(%func,%client,%client.DRMenu_ShopListClass,%client.DRMenu_ShopObj);
	}
	%stack.pop();
}

function DRMenu_SwitchSelect(%stack,%slot)
{
	%client = %stack.client;
	%save = %client.dataInstance($DR::SaveSlot).savedLoadout[%slot];
	return formatItems(%save,%client) NL "\Switch to slot " SPC (%slot + 1);
}

function DRMenu_SwitchUse(%stack,%slot)
{
	%client = %stack.client;
	%client.dataInstance($DR::SaveSlot).EquipPresetIndex = %slot;
	%stack.pop();
}

function DRMenu_SpawnDisplay(%stack,%slot)
{
	%client = %stack.client;
	Inventory::display(%client.player,%client,true);
	%stack.peek(0).display(%client,false);
}

function DRMenu_SpawnSelect(%stack,%slot)
{
	%item = %stack.peek(0).get(%slot);
	if(%item $= "DRMenuEquipmentEditor")
	{
		%s = "Open the loadout editor";
	}
	else if(%item $= "DRMenuShop")
	{
		%s = "Open the shop";
	}
	return "\c3" @ %s;
}

function DRMenu_SpawnUse(%stack,%slot)
{
	%client = %stack.client;
	%item = %stack.peek(0).get(%slot);
	if(%item $= "DRMenuEquipmentEditor")
	{
		%stack.push("Equipment");
	}
	else if(%item $= "DRMenuShop")
	{
		%stack.push("Shop");
	}
}

function DRMenu_Init()
{
	%inv = Inventory_Create("ShopList");
	%inv.set(0,DRMenuNext);
	%inv.display = "DRMenu_ShopListDisplay";
	%inv.select = "DRMenu_ShopListSelect";
	%inv.use = "DRMenu_ShopListUse";

	%inv = Inventory_Create("Confirm");
	%inv.set(0,DRMenuConfirm);
	%inv.select = "DRMenu_ConfirmSelect";
	%inv.use = "DRMenu_ConfirmUse";
	
	%inv = Inventory_Create("Shop");
	%c = -1;
	%inv.set(%c++,DRMenuVehicles);
	%inv.set(%c++,DRMenuTrails);
	%inv.set(%c++,DRMenuMinigameSpecials);
	%inv.select = "DRMenu_ShopSelect";
	%inv.use = "DRMenu_SHopUse";

	%inv = Inventory_Create("Equipment");
	%c = -1;
	%inv.set(%c++,DRMenuPrimary);
	%inv.set(%c++,DRMenuSecondary);
	%inv.set(%c++,DRMenuMelee);
	%inv.set(%c++,DRMenuSupport);
	%inv.set(%c++,DRMenuSwitch);
	%inv.display = "DRMenu_EquipmentDisplay";
	%inv.select = "DRMenu_EquipmentSelect";
	%inv.use = "DRMenu_EquipmentUse";
	%inv.pop = "DRMenu_EquipmentPop";

	%inv = Inventory_Create("Switch");
	%c = -1;
	%inv.set(%c++,DRMenuSlotOne);
	%inv.set(%c++,DRMenuSlotTwo);
	%inv.set(%c++,DRMenuSlotThree);
	%inv.select = "DRMenu_SwitchSelect";
	%inv.use = "DRMenu_SwitchUse";

	%inv = Inventory_Create("Spawn");
	%inv.canUseTools = true;
	%inv.dontOverwrite = true;
	%inv.cantClose = true;
	%c = 4;
	%inv.set(%c++,DRMenuShop);
	%inv.set(%c++,DRMenuEquipmentEditor);
	%inv.display = "DRMenu_SpawnDisplay";
	%inv.select = "DRMenu_SpawnSelect";
	%inv.use = "DRMenu_SpawnUse";
}
DRMenu_Init();