
//inventory ui
datablock ItemData(DRInventoryUIPrimary)
{
	uiname = "Primary";
	doColorShift = true;
	colorShiftColor = "0.6 0 0 1";
};

datablock ItemData(DRInventoryUISecondary)
{
	uiname = "Secondary";
	doColorShift = true;
	colorShiftColor = "0 0.6 0 1";
};

datablock ItemData(DRInventoryUIMelee)
{
	uiname = "Melee";
	doColorShift = true;
	colorShiftColor = "0.5 0.5 0.5 1";
};

datablock ItemData(DRInventoryUISupport)
{
	uiname = "Support";
	doColorShift = true;
	colorShiftColor = "0 0 0.6 1";
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
	uiname = "Ready";
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

function DRInventoryUI_SpawnPrint(%client,%inv,%slot)
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
		%s = "Load a different loadout";
	case %c++:
		%s = "Save your current loadout";
	case %c++:
		%s = "Close";
	}
	return "\c6" @ %s;
}

function DRInventoryUI_SpawnNext(%client,%inv,%slot)
{
	%c = -1;
	%s = "";
	switch(%slot)
	{
	case %c++:
		%s = "Primary";
	case %c++:
		%s = "Secondary";
	case %c++:
		%s = "Melee";
	case %c++:
		%s = "Support";
	case %c++:
		%s = "Load";
	case %c++:
		%s = "Save";
	case %c++:
		%s = "POP";
	}
	return %s;
}

function formatItem(%item,%c,%desc)
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
		if(%script.buyOnce)
		{
			%append = %append NL "\c5Single-time pruchase";
		}
		else
		{
			%append = %append NL "\c5MUST BE REBOUGHT";
		}
	}

	return %append;
}

function formatItems(%list,%c,%desc)
{
	%count = getWordCount(%list);
	if(%count == 0)
	{
		return "Empty";
	}
	
	%totalCost = 0;
	%s = "";
	for(%i = 0; %i < %count; %i++)
	{
		%item = getWord(%list,%i);
		%totalCost += getDRShopGroup().findScript(%item.uiname).cost;

		%s = %s NL formatItem(%item,%c,%desc);
	}

	if(%totalCost > 0)
	{
		%s = %s NL "\c5Costs" SPC %totalCost SPC "points to use";
	}
	return trim(%s);
}

function DRInventoryUI_SavePrint(%client,%inv,%slot)
{
	%save = %client.dataInstance($DR::SaveSlot).savedLoadout[%slot];
	return formatItems(%save,%client);
}

function DRInventoryUI_Save(%client,%inv,%slot)
{
	%player = %client.player;
	if(isObject(%player))
	{
		%player.Shop_Save(%slot);
		%client.DRInventoryUI_pop();
		%client.centerPrint("\c6Saved succesfuly");
	}
}

function DRInventoryUI_LoadPrint(%client,%inv,%slot)
{
	%save = %client.dataInstance($DR::SaveSlot).savedLoadout[%slot];
	return formatItems(%save,%client);
}

function DRInventoryUI_Load(%client,%inv,%slot)
{
	%player = %client.player;
	if(isObject(%player))
	{
		%player.Shop_Load(%slot);
		%client.DRInventoryUI_pop();
		%client.centerPrint("\c6Loaded succesfuly");
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
	%itemIndex = 0;
	for(%i = 0; %i < %count; %i++)
	{
		//TODO:
		//this probably shouldn't be hard coded like this
		//i would like to look into better item slot count management code
		//datablock based sucks
		if(%itemIndex % 8 == 0)
		{
			%o.set(%itemIndex,DRInventoryUINext);
			%itemIndex++;
		}

		%o.set(%itemIndex,getWord(%list,%i));
		%itemIndex++;
	}
}

function DRInventoryUI_ShopPrint(%client,%inv,%slot)
{
	%item = %inv.get(%slot);
	if(%item.getid() == DRInventoryUINext.getid())
	{
		return "\c6Next Page";
	}
	
	return formatItem(%item,%client,true);
}

function DRInventoryUI_Shop(%client,%inv,%slot)
{
	%item = %inv.get(%slot);
	
	if(%item.getid() == DRInventoryUINext.getid())
	{
		%client.DRInventoryUI_Offset += 8;
		if(!isObject(%inv.get(0 + %client.DRInventoryUI_Offset)))
		{
			%client.DRInventoryUI_Offset = 0;
		}
		%client.DRInventoryUI_display();
	}
}

function DRInventoryUI_Init()
{
	if(isObject($DRInventoryUI))
	{
		$DRInventoryUI.delete();
	}
	$DRInventoryUI = new ScriptObject(){class = "DRInventoryManager";};

	%o = Inventory_Create();
	%c = -1;
	%o.set(%c++,DRInventoryUIPrimary);
	%o.set(%c++,DRInventoryUISecondary);
	%o.set(%c++,DRInventoryUIMelee);
	%o.set(%c++,DRInventoryUISupport);
	%o.set(%c++,DRInventoryUILoad);
	%o.set(%c++,DRInventoryUISave);
	%o.set(%c++,DRInventoryUIReady);
	%o.print = "DRInventoryUI_SpawnPrint";
	%o.next = "DRInventoryUI_SpawnNext";
	%o.cantClose = true;
	$DRInventoryUI.add(%o,"Spawn");

	%c = -1;
	%o = Inventory_Create();
	%o.set(%c++,DRInventoryUISlotOne);
	%o.set(%c++,DRInventoryUISlotTwo);
	%o.set(%c++,DRInventoryUISlotThree);
	%o.print = "DRInventoryUI_SavePrint";
	%o.use = "DRInventoryUI_Save";
	$DRInventoryUI.add(%o,"Save");

	%c = -1;
	%o = Inventory_Create();
	%o.set(%c++,DRInventoryUISlotOne);
	%o.set(%c++,DRInventoryUISlotTwo);
	%o.set(%c++,DRInventoryUISlotThree);
	%o.print = "DRInventoryUI_LoadPrint";
	%o.use = "DRInventoryUI_Load";
	$DRInventoryUI.add(%o,"Load");

	%c = -1;
	%o = Inventory_Create();
	DRInventoryUI_FillItems(%o,"Primary");
	%o.print = "DRInventoryUI_ShopPrint";
	%o.use = "DRInventoryUI_Shop";
	$DRInventoryUI.add(%o,"Primary");
	
	%c = -1;
	%o = Inventory_Create();
	DRInventoryUI_FillItems(%o,"Secondary");
	%o.print = "DRInventoryUI_ShopPrint";
	%o.use = "DRInventoryUI_Shop";
	$DRInventoryUI.add(%o,"Secondary");

	%c = -1;
	%o = Inventory_Create();
	DRInventoryUI_FillItems(%o,"Melee");
	%o.print = "DRInventoryUI_ShopPrint";
	%o.use = "DRInventoryUI_Shop";
	$DRInventoryUI.add(%o,"Melee");

	%c = -1;
	%o = Inventory_Create();
	DRInventoryUI_FillItems(%o,"Support");
	%o.print = "DRInventoryUI_ShopPrint";
	%o.use = "DRInventoryUI_Shop";
	$DRInventoryUI.add(%o,"Support");
}
DRInventoryUI_Init();

function GameConnection::DRInventoryUI_push(%client,%inv)
{
	commandToClient(%client,'SetActiveTool',0);
	%client.DRInventoryUI_stack = trim(%inv SPC %client.DRInventoryUI_stack);
	%client.DRInventoryUI_display();
}

function GameConnection::DRInventoryUI_pop(%client)
{
	%client.DRInventoryUI_Offset = 0;
	commandToClient(%client,'SetActiveTool',0);
	%client.DRInventoryUI_stack = removeWord(%client.DRInventoryUI_stack,0);
	%client.DRInventoryUI_display();
}

function GameConnection::DRInventoryUI_clear(%client)
{
	%client.DRInventoryUI_stack = "";
	%client.DRInventoryUI_display();
}


function GameConnection::DRInventoryUI_top(%client)
{
	return $DRInventoryUI.get(firstWord(%client.DRInventoryUI_stack));
}

function GameConnection::DRInventoryUI_display(%client)
{
	%client.centerPrint("");
	%curr = %client.DRInventoryUI_top();

	if(%curr $= "")
	{
		%client.currUi = "";
		%player = %client.player;
		if(isObject(%player))
		{
			Inventory::Display(%player,%client,true);
		}
		return;
	}

	%curr.display(%client,true,%client.DRInventoryUI_Offset);
	

	%player = %client.player;
	if(isObject(%player))
	{
		serverCmdUseTool(%client,%player.currTool);
		if(%curr.displayTools)
		{
			Inventory::Display(%player,%client);
		}

		if(!%curr.canUseItems)
		{
			%player.currTool = -1;
			%client.currInv = -1;
			%client.currInvSlot = -1;
			%player.unmountImage(0);
			%player.playThread (1, root);
		}
	}
}

package DRInventoryUI
{
	function serverCmdUnUseTool(%client)
	{
		%curr = %client.DRInventoryUI_top();
		if(%curr !$= "")
		{
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
			if(%slot != -1 && isObject(%curr.tool[%slot]))
			{
				%client.currUi = %slot;
			}
			%slot = %client.currUi;

			if(%slot !$= "")
			{
				%controls = "\c4Click to use";
				if(!%curr.cantClose)
				{
					%controls = %controls NL "Close to cancel";
				}
				%client.centerPrint(trim(call(%curr.print,%client,%curr,%slot + %client.DRInventoryUI_Offset) NL %controls));
			}
			
			if(!%curr.canUseItems)
			{
				return;
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
				%next = call(%curr.next,%client,%curr,%slot + %client.DRInventoryUI_Offset);
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
				call(%curr.use,%client,%curr,%slot + %client.DRInventoryUI_Offset);
				if(!%curr.canUseItems)
				{
					return;
				}
			}
		}
		return parent::ActivateStuff(%player);
	}
};
activatepackage("DRInventoryUI");