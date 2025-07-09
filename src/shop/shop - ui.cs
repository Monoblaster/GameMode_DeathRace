$DR::DefaultLoadout = "EMPTYSLOT PistolItem L4BCookingKnifeItem EMPTYSLOT EMPTYSLOT EMPTYSLOT EMPTYSLOT";
//inventory ui
datablock ItemData(DRInventoryUIPrimary)
{
	uiname = "Primary";
	iconName = BattleRifleItem.iconName;
	doColorShift = true;
	colorShiftColor = "0 0 0 1";
};

datablock ItemData(DRInventoryUISecondary)
{
	uiname = "Secondary";
	iconName = PistolItem.iconName;
	doColorShift = true;
	colorShiftColor = "0 0 0 1";
};

datablock ItemData(DRInventoryUIMelee)
{
	uiname = "Melee";
	iconName = L4BCookingKnifeItem.iconName;
	doColorShift = true;
	colorShiftColor = "0 0 0 1";
};

datablock ItemData(DRInventoryUISupport)
{
	uiname = "Support";
	iconName = StimpackItem.iconName;
	doColorShift = true;
	colorShiftColor = "0 0 0 1";
};

datablock ItemData(DRInventoryUILoad)
{
	uiname = "Load";
	doColorShift = true;
	colorShiftColor = "1 1 0 1";
};

datablock ItemData(DRInventoryUISave)
{
	uiname = "Save";
	doColorShift = true;
	colorShiftColor = "0 1 0 1";
};

datablock ItemData(DRInventoryUIReady)
{
	uiname = "Done";
	doColorShift = true;
	colorShiftColor = "1 0 1 1";
};

datablock ItemData(DRInventoryUISlotOne)
{
	uiname = "1";
};

datablock ItemData(DRInventoryUISlotTwo)
{
	uiname = "2";
};

datablock ItemData(DRInventoryUISlotThree)
{
	uiname = "3";
};

datablock ItemData(DRInventoryUINext)
{
	uiname = "V";
};

datablock ItemData(DRInventoryUINone)
{
	uiname = "None";
};

datablock ItemData(DRInventoryUIYes)
{
	uiname = "Yes";
};

datablock ItemData(DRInventoryUICancel)
{
	uiname = "Cancel";
};

datablock ItemData(DRInventoryUINo)
{
	uiname = "No";
};

datablock ItemData(DRInventoryUIConfirm)
{
	uiname = "Confirm";
};

datablock ItemData(DRInventoryUIEquipmentEditor)
{
	uiname = "Loadout";
};



function DRInventoryManager::add(%obj, %inv, %name)
{
	if(%obj.UI[%name] !$= "")
	{
		return;
	}

	%obj.UI[%name] = %inv;
	%obj.list = trim(%obj.list SPC %inv);
}

function DRInventoryManager::get(%obj, %name)
{
	return %obj.UI[%name];
}

function DRInventoryManager::onRemove(%obj)
{
	%s = %obj.list;
	%count = getWordCount(%s);
	for(%i = 0; %i < %count; %i++)
	{
		getWord(%s,%i).delete();
	}
}

function purchaseTool(%client,%item,%notRoundStart)
{
	%this = %client;
	%obj = findItemByName(%item);
	%cost = ToolCost(%client,%obj,%notRoundStart);
	if(%cost > %client.score)
	{
		%this.chatMessage("Cannot afford");
		return false;
	}

	%groupObj = getDRShopGroup().findScript(%item);
	if(!isObject(%groupObj))
	{
		%this.chatMessage("\c6Invalid shop item (\"\c3" @ %item @ "\c6\"). Please contact your super administrator.");
		return false;
	}

	%strName = stripChars(%groupObj.uiName, $Shop::Chars);
	%strName = strReplace(%strName, " ", "_");

	if(!%groupObj.buyOnce)
	{
		%this.dataInstance($DR::SaveSlot).boughtItem[%strName] = 0;
		commandToClient(%this, 'DRShop', "SET_BOUGHT", %groupObj.uiName, %this.dataInstance($DR::SaveSlot).boughtItem[%strName]);
	}

	if(!%this.dataInstance($DR::SaveSlot).boughtItem[%strName] && !%this.bypassShop)
	{
		%this.incScore(-%cost);
	}

	if(%groupObj.buyOnce && !%this.dataInstance($DR::SaveSlot).boughtItem[%strName])
	{
		%this.dataInstance($DR::SaveSlot).boughtItem[%strName] = 1;
		if(%cost > 0) %this.chatMessage("\c6You have bought this item: \c4" @ %groupObj.uiName @ "\c6, you now equip it! You do not ever have to buy this weapon again, yay!");
	}

	commandToClient(%this, 'DRShop', "SET_BOUGHT", %groupObj.uiName, %this.dataInstance($DR::SaveSlot).boughtItem[%strName]);
	return true;
}

function purchaseLoadout(%client,%notRoundStart)
{
	%this = %client;
	%player = %client.player;
	if(!isObject(%player))
	{
		return false;
	}

	%cost = LoadoutCost(%client,%notRoundStart);
	if(%cost > %client.score)
	{
		return false;
	}

	%count = %client.getMaxTools();
	for(%i = 0; %i < %count; %i++)
	{
		%item = %player.tool[%i];
		if(!isObject(%item))
		{
			continue;
		}

		%item = %item.uiName;

		if(!purchaseTool(%client,%item,%notRoundStart))
		{
			return false;
		}
	}

	return true;
}

function ToolCost(%client,%item,%notRoundStart)
{
	if(!isObject(%item))
	{
		return 0;
	}

	%data = %client.dataInstance($DR::SaveSlot);
	%shopName = stripChars(strReplace(%item.uiname, " ", "_"), $Shop::Chars);
	%script = getDRShopGroup().findScript(%item.uiname);
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

function LoadoutCost(%client,%notRoundStart)
{
	%player = %client.player;
	if(!isObject(%player))
	{
		return;
	}

	%data = %client.dataInstance($DR::SaveSlot);
	%count = %client.getMaxTools();
	%total = 0;
	for(%i = 0; %i < %count; %i++)
	{
		%item = %player.tool[%i];
		%total += ToolCost(%client,%item,%notRoundStart);
	}

	return %total;
}

function DRInventoryUI_SpawnPrint(%client,%inv,%slot)
{
	%c = -1;
	%s = "";
	switch(%slot)
	{
	case 6:
		%s = "Open the loadout editor";
	}
	%s = "\c3" @ %s;
	return %s;
}

function DRInventoryUI_SpawnNext(%client,%inv,%slot)
{
	%c = -1;
	%s = "";
	switch(%slot)
	{
	case 6:
		%s = "EquipmentEditor";
	}
	return %s;
}

function DRInventoryUI_EquipmentEditorPrint(%client,%inv,%slot)
{
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
		%s = "Save your current loadout";
	case %c++:
		%s = "Load a different loadout";
	case %c++:
		%s = "Confirm your loadout";
	}
	%s = "\c3" @ %s;
	%cost = LoadoutCost(%client);
	if(%cost > 0)
	{
		%s = "\c5Current loadout costs" SPC %cost SPC "points every round" NL %s;
	}
	
	%player = %client.player;
	%item = %player.tool[%slot];
	if(isObject(%item))
	{
		%s = formatItem(%item,%client,true) NL %s;
	}

	return %s;
}

function DRInventoryUI_Ready(%client,%save,%notRoundStart)
{	
	%player = %client.player;
	if(isObject(%player) && purchaseLoadout(%client,%notRoundStart))
	{
		if(%save && !%notRoundStart)
		{
			%s = "";
			%count = %client.getMaxTools();
			for(%i = 0; %i < %count; %i++)
			{
				%tool = %player.tool[%i];
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
			%client.dataInstance($DR::SaveSlot).LastLoadOut = trim(%s);
		}
		
		//remove gaps
		for(%i = 0; %i < %count; %i++)
		{
			%tool = %player.tool[%i];
			if(!isObject(%tool))
			{
				for(%j = (%i + 1); %j < %count; %j++)
				{
					%tool = %player.tool[%j];
					if(isObject(%tool))
					{
						%player.tool[%i] = %player.tool[%j];
						%player.tool[%j] = "";
						break;
					}
				}
			}
		}
		Inventory::Display(%player,%client,true);

		return true;
	}

	%client.chatMessage("\c6You do not have enough score to use this loadout.");
	return false;
}

function DRInventoryUI_EquipmentEditorNext(%client,%inv,%slot)
{
	%c = -1;
	%s = "";
	switch(%slot)
	{
	case %c++:
		%client.DRInventoryUI_ShopReplaceSlot = %slot;
		%client.DRInventoryUI_Shop = $DRInventoryUI_ShopPrimary;
		%s = "ShopOverlay";
	case %c++:
		%client.DRInventoryUI_ShopReplaceSlot = %slot;
		%client.DRInventoryUI_Shop = $DRInventoryUI_ShopSecondary;
		%s = "ShopOverlay";
	case %c++:
		%client.DRInventoryUI_ShopReplaceSlot = %slot;
		%client.DRInventoryUI_Shop = $DRInventoryUI_ShopMelee;
		%s = "ShopOverlay";
	case %c++:
		%client.DRInventoryUI_ShopReplaceSlot = %slot;
		%client.DRInventoryUI_Shop = $DRInventoryUI_ShopSupport;
		%s = "ShopOverlay";
	case %c++:
		%s = "Save";
	case %c++:
		%s = "Load";
	case %c++:
		if( DRInventoryUI_Ready(%client,true,true))
		{
			%s = "POP";
		}
	}
	return %s;
}

function formatItem(%item,%c,%desc,%purchaseinfo)
{
	%data = %c.dataInstance($DR::SaveSlot);
	%shopName = stripChars(strReplace(%item.uiname, " ", "_"), $Shop::Chars);
	%append = %item.uiname;
	%script = getDRShopGroup().findScript(%item.uiname);
	if(!%data.boughtItem[%shopName])
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

	if(%purchaseInfo)
	{
		if(%script.buyOnce)
		{
			%append = %append NL "\c5Single-time pruchase";
		}
		else
		{
			%append = %append NL "\c5MUST BE REBOUGHT";
		}

		if(!%script.buyOnce)
		{
			%append = %append NL "\c3Equip and purchase at roundstart";
		}
		else if(%data.boughtItem[%shopName])
		{
			%append = %append NL "\c3Equip";
		}	
		else if(%script.cost + 0 > %c.score)
		{
			%append = %append NL "\c0Not enough points";
		}
		else
		{
			%append = %append NL "\c3Purchase";
		}
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
			%totalCost += getDRShopGroup().findScript(%item.uiname).cost;
		}
		
		%s = %s NL formatItem(%item,%c,%desc,%purchaseInfo);
	}

	if(%totalCost > 0)
	{
		%s = %s NL "\c5Costs" SPC %totalCost SPC "points every round";
	}
	return trim(%s);
}

function DRInventoryUI_SavePrint(%client,%inv,%slot)
{
	%save = %client.dataInstance($DR::SaveSlot).savedLoadout[%slot];
	return formatItems(%save,%client) NL "\c3Save to slot " SPC (%slot + 1);
}

function DRInventoryUI_Save(%client,%inv,%slot)
{
	%player = %client.player;
	if(isObject(%player))
	{
		%player.Shop_Save(%slot);
		%client.DRInventoryUI_pop();
	}
}

function DRInventoryUI_LoadPrint(%client,%inv,%slot)
{
	%save = %client.dataInstance($DR::SaveSlot).savedLoadout[%slot];
	return formatItems(%save,%client) NL "\c3Load from slot " SPC (%slot + 1);
}

function DRInventoryUI_Load(%client,%inv,%slot)
{
	%player = %client.player;
	if(isObject(%player))
	{
		%player.Shop_LoadList(%client.dataInstance($DR::SaveSlot).savedLoadout[%slot]);
		%client.DRInventoryUI_pop();
	}
}

function DRInventoryUI_FillItems(%o,%catagory)
{
	%group = getDRShopGroup();
	%count = %group.getCount();
	%list = "";
	for(%i = 0; %i < %count; %i++)
	{
		%curr = %group.getObject(%i);
		if(%curr.shopClass $= %catagory && %curr.adminLevel <= 0)
		{
			%list = %list SPC findItemByName(%curr.datablockName);
		}
	}

	%list = trim(sortWords(%list,"getDRShopGroup().findScript(%v1.uiname).cost < getDRShopGroup().findScript(%v2.uiname).cost"));
	%count = getWordCount(%list);
	%o.set(0,DRInventoryUINone);
	%itemIndex = 1;
	for(%i = 0; %i < %count; %i++)
	{
		%o.set(%itemIndex,getWord(%list,%i));
		%itemIndex++;
	}
}

function DRInventoryUI_ShopPrint(%client,%inv,%slot)
{
	%shopSlot = %slot + %client.DRInventoryUI_ShopOffset;
	%item = %client.DRInventoryUI_Shop.get(%shopSlot);
	if(%slot == 0)
	{
		return "\c3Next Page";
	}

	if(isObject(%item))
	{
		if(%item.getid() == DRInventoryUINone.getid())
		{
			return "\c3Empties this slot";
		}
		return formatItem(%item,%client,true,true);
	}
	return "";
}

function DRInventoryUI_ShopPush(%client,%inv,%slot)
{
	%client.DRInventoryUI_ShopOffset = -1;
}

function DRInventoryUI_ShopDisplay(%client,%inv,%slot)
{
	%client.DRInventoryUI_Shop.display(%client,true,%client.DRInventoryUI_ShopOffset);
}

function DRInventoryUI_Shop(%client,%inv,%slot)
{
	%shopSlot = %slot + %client.DRInventoryUI_ShopOffset;
	%item = %client.DRInventoryUI_Shop.get(%slot + %client.DRInventoryUI_ShopOffset);
	if(%slot == 0)
	{
		%client.DRInventoryUI_ShopOffset += %client.getMaxTools() - 1;
		if(!isObject(%client.DRInventoryUI_Shop.get(%client.DRInventoryUI_ShopOffset + 1)))
		{
			%client.DRInventoryUI_ShopOffset = -1;
		}
		%client.DRInventoryUI_Shop.display(%client,true,%client.DRInventoryUI_ShopOffset);
		%client.DRInventoryUI_display();
		return;
	}

	%player = %client.player;
	if(isObject(%item) && isObject(%player))
	{
		%item = %item.getid();
		%replaceSlot = %client.DRInventoryUI_ShopReplaceSlot;
		if(%item == DRInventoryUINone.getid())
		{
			%player.tool[%replaceSlot] = "";
			messageClient(%client,'MsgItemPickup','',%replaceSlot,"",true);
			%client.DRInventoryUI_pop();
			return;
		}

		if(%client.DataInstance($DR::SaveSlot).boughtItem[stripChars(strReplace(%item.uiname, " ", "_"), $Shop::Chars)])
		{
			%player.tool[%replaceSlot] = %item;
			messageClient(%client,'MsgItemPickup','',%replaceSlot,%item,true);
			%client.DRInventoryUI_pop();
			return;
		}	

		%client.DRInventoryUI_OnYes = "ShopPurchaseItem";
		%client.DRInventoryUI_Item = %item;
		%client.DRInventoryUI_Message = "\c6Are you sure you want to equip this item?" NL formatItem(%item,%client,true,true);
		%client.DRInventoryUI_push("Confirm");
	}
}

function ShopPurchaseItem(%client)
{
	%client.DRInventoryUI_pop();
	%player = %client.player;
	%item = %client.DRInventoryUI_Item;
	if(!isObject(%item) || !isObject(%player))
	{
		return;
	}

	if(purchaseTool(%client,%item.uiName,true))
	{
		%replaceSlot = %client.DRInventoryUI_ShopReplaceSlot;
		%player.tool[%replaceSlot] = %item;
		messageClient(%client,'MsgItemPickup','',%replaceSlot,%item,true);
	}
	
}

function DRInventoryUI_ConfirmPrint(%client,%inv,%slot)
{
	%shopSlot = %slot + %client.DRInventoryUI_ShopOffset;
	%item = %client.DRInventoryUI_Shop.get(%shopSlot);
	return %client.DRInventoryUI_Message;
}

function DRInventoryUI_Confirm(%client,%inv,%slot)
{
	call(%client.DRInventoryUI_OnYes,%client);
	%client.DRInventoryUI_pop();
}

function ShopEquioment_Select(%stack,%slot)
{

}

function ShopEquioment_Select(%stack,%slot)
{

}

function DRInventoryUI_Init()
{
	%inv = Inventory_Create("Equipment");
	%inv.cantClose = true;
	%c = -1;
	%inv.set(%c++,DRInventoryUIPrimary);
	%inv.set(%c++,DRInventoryUISecondary);
	%inv.set(%c++,DRInventoryUIMelee);
	%inv.set(%c++,DRInventoryUISupport);
	%inv.set(%c++,DRInventoryUISave);
	%inv.set(%c++,DRInventoryUILoad);
	%o.select = "ShopEquipment_Select";
	%o.use = "ShopEquipment_Use";
	%o.display = "ShopEquioment_Display";
	
	// %o = Inventory_Create();
	// %o.set(6,DRInventoryUIEquipmentEditor);
	// %o.print = "DRInventoryUI_SpawnPrint";
	// %o.next = "DRInventoryUI_SpawnNext";
	// %o.cantClose = true;
	// %o.displayTools = true;
	// %o.canUseItems = true;
	// $DRInventoryUI.add(%o,"Spawn");

	// %o = Inventory_Create();
	// %c = -1;
	// %o.set(%c++,DRInventoryUIPrimary);
	// %o.set(%c++,DRInventoryUISecondary);
	// %o.set(%c++,DRInventoryUIMelee);
	// %o.set(%c++,DRInventoryUISupport);
	// %o.set(%c++,DRInventoryUISave);
	// %o.set(%c++,DRInventoryUILoad);
	// %o.print = "DRInventoryUI_EquipmentEditorPrint";
	// %o.next = "DRInventoryUI_EquipmentEditorNext";
	// %o.displayTools = true;
	// $DRInventoryUI.add(%o,"EquipmentEditor");

	// %c = -1;
	// %o = Inventory_Create();
	// %o.set(%c++,DRInventoryUISlotOne);
	// %o.set(%c++,DRInventoryUISlotTwo);
	// %o.set(%c++,DRInventoryUISlotThree);
	// %o.print = "DRInventoryUI_SavePrint";
	// %o.use = "DRInventoryUI_Save";
	// $DRInventoryUI.add(%o,"Save");

	// %c = -1;
	// %o = Inventory_Create();
	// %o.set(%c++,DRInventoryUISlotOne);
	// %o.set(%c++,DRInventoryUISlotTwo);
	// %o.set(%c++,DRInventoryUISlotThree);
	// %o.print = "DRInventoryUI_LoadPrint";
	// %o.use = "DRInventoryUI_Load";
	// $DRInventoryUI.add(%o,"Load");

	// %c = -1;
	// %o = Inventory_Create();
	// %o.set(0,DRInventoryUINext);
	// %o.display = "DRInventoryUI_ShopDisplay";
	// %o.print = "DRInventoryUI_ShopPrint";
	// %o.push = "DRInventoryUI_ShopPush";
	// %o.use = "DRInventoryUI_Shop";
	// %o.overlay = true;
	// $DRInventoryUI.add(%o,"ShopOverlay");

	// %c = -1;
	// %o = Inventory_Create();
	// %o.set(0,DRInventoryUIConfirm);
	// %o.print = "DRInventoryUI_ConfirmPrint";
	// %o.use = "DRInventoryUI_Confirm";
	// $DRInventoryUI.add(%o,"Confirm");
}
DRInventoryUI_Init();

function DRInventoryUI_ShopInit()
{
	if(isObject($DRInventoryUI_ShopPrimary))
	{
		$DRInventoryUI_ShopPrimary.delete();
		$DRInventoryUI_ShopSecondary.delete();
		$DRInventoryUI_ShopMelee.delete();
		$DRInventoryUI_ShopSupport.delete();
	}

	%o = Inventory_Create();
	DRInventoryUI_FillItems(%o,"Primary");
	$DRInventoryUI_ShopPrimary = %o;
	
	%o = Inventory_Create();
	DRInventoryUI_FillItems(%o,"Secondary");
	$DRInventoryUI_ShopSecondary = %o;

	%o = Inventory_Create();
	DRInventoryUI_FillItems(%o,"Melee");
	$DRInventoryUI_ShopMelee = %o;

	%o = Inventory_Create();
	DRInventoryUI_FillItems(%o,"Support");
	$DRInventoryUI_ShopSupport = %o;
}
schedule(1000,0,"DRInventoryUI_ShopInit");

package DRInventoryUI
{
	function serverCmdUnUseTool(%client)
	{
		%curr = %client.DRInventoryUI_top();
		if(%curr !$= "")
		{
			%client.currUi = "";
			if(!%curr.cantClose)
			{
				%client.DRInventoryUI_pop();
			}
		}
		
		return Parent::serverCmdUnUseTool(%client);
	}

	function serverCmdUseTool(%client,%slot)
	{
		%curr = %client.DRInventoryUI_top();
		
		if(%curr !$= "")
		{
			%client.currUi = "";
			if(%slot != -1 && isObject(%curr.tool[%slot]) || %curr.overlay)
			{
				%client.currUi = %slot;
			}
			%uislot = %client.currUi;

			if(%uislot !$= "")
			{
				%slot = -1;
				%controls = "\c4Click to use";
				if(!%curr.cantClose)
				{
					%controls = %controls NL "Close to cancel";
				}
			}
			%client.centerPrint(trim(call(%curr.print,%client,%curr,%uislot) NL %controls));

			if(!%curr.canUseItems)
			{
				return;
			}

			%player = %client.player;
			if(%slot == -1 && isobject(%player))
			{
				%player.unmountImage(0);
				fixArmReady(%player);
			}
		}

		return parent::serverCmdUseTool(%client,%slot);
	}

	function Player::ActivateStuff(%player)
	{
		%client = %player.client;
		if(isObject(%client))
		{
			%slot = %client.currUi;
			if(%slot !$= "")
			{
				%curr = %client.DRInventoryUI_top();
				%next = call(%curr.next,%client,%curr,%slot);
				if(%next !$= "")
				{
					if(%next $= "pop")
					{
						%client.DRInventoryUI_pop();
					}
					else
					{
						%client.DRInventoryUI_push(%next);
					}
				}
				call(%curr.use,%client,%curr,%slot);
				if(!%curr.canUseItems || !isObject(%player.tool[%slot]))
				{
					return;
				}
			}
		}
		return parent::ActivateStuff(%player);
	}
};
activatepackage("DRInventoryUI");