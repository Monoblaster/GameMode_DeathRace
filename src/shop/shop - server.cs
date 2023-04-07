$Shop::Server::Version = 7.41;
$Shop::Server::RequiredVersion = 7.3;

$Shop::Link = "https://leopard.hosting.pecon.us/dl/tzkds/Client_DeathRace.zip";
$Donate::Link = "https://www.nfoservers.com/donate.pl?force_recipient=1&recipient=cocoalove886@gmail.com";

if(isPackage(Shop_Server))
	deActivatePackage(Shop_Server);

package Shop_Server
{
	function GameConnection::spawnPlayer(%this)
	{
		Parent::SpawnPlayer(%this);
		if(isObject(%this.minigame) && isObject(%player = %this.player))
		{
			if(%this.Shop_Client)
			{
				commandToClient(%this, 'DRShop', "SET_POINTS", %this.getScore());
				if(%this.DR_GUIPerRound)
					schedule(500, 0, commandToClient, %this, 'DRShop', "OPEN");
			}
			else if(!$Server::DeathRaceTemp::BotheredClient[%this.getBLID()])
			{
				$Server::DeathRaceTemp::BotheredClient[%this.getBLID()] = 1;
				commandToClient(%this, 'MessageBoxOK', "Want the GUI?", "Hello! This is a one time thing. If you are interested in the GUI:\n<a:" @ $Shop::Link @ ">Click here!");
				schedule(1000, 0, commandToClient, %this, 'MessageBoxOK', "Want the GUI?", "Hello! This is a one time thing. If you are interested in the GUI:\n<a:" @ $Shop::Link @ ">Click here!");
			}

			cancel(%this.shopSch);
			%this.shopSch = %this.schedule(2000, Shop_Spawn);
		}
	}

	function GameConnection::setScore(%this, %score)
	{
		Parent::setScore(%this, %score);
		if(%this.Shop_Client)
			commandToClient(%this, 'DRShop', "SET_POINTS", %score);
	}

	function GameConnection::autoAdminCheck(%this)
	{
		%this.Shop_SetData();
		%this.schedule(4000, PingCheck);
		return Parent::autoAdminCheck(%this);
	}
};
activatePackage(Shop_Server);

////////////////////////////////////////////////////////////////////////////////////////

function serverCmdTrails(%client, %wd1, %wd2, %wd3)
{
	return serverCmdTrail(%client, %wd1, %wd2, %wd3);
}

function serverCmdTrail(%client, %wd1, %wd2, %wd3)
{
	%player = %client.player;
	%choice = trim(%wd1 SPC %wd2 SPC %wd3);

	if(%choice $= "none" || %choice $= "off")
	{
		%player.unmountImage(3);
		%client.currentTrail = "";
		return;
	}

	if(isTrail(%choice) && $BoughtItem_[%client.getBLID(), getSafeVariableName("Trail - " @ %choice)])
	{
		%img = ("Trail_" @ strReplace(%choice, " ", "_") @ "_Mounted_Image");
		//Set Trail
		if(isObject(%player))
		{
			if(isObject(%curImg = %player.getMountedImage(3)) && %curImg.getName() $= %img)
			{
				%player.unmountImage(3);
				%client.currentTrail = "";
			}
			else
			{
				%player.mountImage(%img, 3);
				%client.currentTrail = %choice;
			}
		}
	}
	else
	{
		//List Trails
		serverCmdListTrails(%client);
	}
}

function serverCmdListTrails(%client)
{
	%client.chatMessage("<font:verdana:25p>\c6 -----------------------");
	%client.chatMessage("<font:verdana:25p>\c6 - Player Trails Owned -");
	%client.chatMessage("<font:verdana:25p>\c6 -----------------------");

	%trailList = "\c6   None  \c7|\c6 ";
	%count = 1;

	for(%i = 0; %i < $Trails::TrailCount; %i++)
	{
		%trail = $Trails::TrailDisplayName[%i];
		%bought = $BoughtItem_[%client.getBLID(), getSafeVariableName("Trail - " @ %trail)];

		if(!%bought)
			continue;

		%trailList = trim(%trailList SPC %trail);
		%trailList = %trailList SPC " \c7|\c6 ";

		%count++;
		if(%count == 4)
		{
			%client.chatMessage(trim(getWords(%trailList, 0, getWordCount(%trailList)-2)));
			%trailList = "\c6  ";
			%count = 0;
		}
	}

	if(%trailList !$= "\c6  ")
	{
		%client.chatMessage(trim(getWords(%trailList, 0, getWordCount(%trailList)-2)));
	}
}

//Functions
function isTrail(%choice)
{
	return isObject(strReplace("Trail_" @ %choice @ "_Mounted_Image", " ", "_"));
}

//Package
if(isPackage(PlayerTrailsMainPackage))
	deActivatePackage(PlayerTrailsMainPackage);

package PlayerTrailsMainPackage
{
	function GameConnection::ApplyBodyParts(%client)
	{
		parent::ApplyBodyParts(%client);

		%player = %client.player;
		if(isObject(%player))
		{
			%player.mountImage(("Trail_" @ strReplace(%client.currentTrail, " ", "_") @ "_Mounted_Image"), 3);
		}
	}
};

activatePackage(PlayerTrailsMainPackage);

//////////////////////////////////////////////////////////////////////////////////////////

function serverCmdShop_ConfirmLoad(%this)
{
	if(!isObject(%pl = %this.player))
		return;

	%pl.Shop_Load();
}

function serverCmdShop_ConfirmSave(%this)
{
	if(!isObject(%pl = %this.player))
		return;

	%pl.Shop_Save();
}

function serverCmdMyPing(%this)
{
	if(!%this.isPinging)
		%this.PingCheck();
}

function serverCmdGuiLink(%this)
{
	%this.chatMessage("\c6GUI Link: <a:" @ $Shop::Link @ ">Client_DeathRace.zip</a>");
}

function serverCmdDonate(%this)
{
        %this.chatMessage("\c6Donate Link: <a:" @ $Donate::Link @ ">Donate Link</a>");
}

function ShopServ_Debug(%msg)
{
	if($Shop::Server::Debug)
		messageAll('',"\c6[\c4ShopServ_Debug\c6] \c3" @ %msg);
}

function serverCmdWhoHasGui(%this)
{
	if(!%this.isAdmin) return;
	for(%i=0;%i<clientGroup.getCount();%i++)
	{
		%cl = clientGroup.getObject(%i);
		if(%cl != %this && %cl.Shop_Client)
			%this.chatMessage("\c4" @ %cl.getPlayerName() @ "\c6 has the shop client.");
		else if(%cl != %this && %cl.Shop_OutOfDate)
			%this.chatMessage("\c4" @ %cl.getPlayerName() @ "\c6 has the shop client, but it is \croutdated\c6.");
	}
}

function serverCmdShopHelp(%this)
{
	if(!%this.isSuperAdmin) return;
	%this.chatMessage("\c4/addCurrentItem price category \c6- Adds the current item in your hand to the shop. You'll need the GUI to edit the description.");
	%this.chatMessage("\c4/removeCurrentItem \c6- Removes the item completely from the shop.");
	%this.chatMessage("\c4/SetCurrentItemAdmin level \c6- Change the item's admin level (set to 0 or blank for non-admins) (1 - admin) (2 - super admin)");
}

function serverCmdGui(%this)
{
	if(%this.Shop_Client)
		commandToClient(%this, 'DRShop', "OPEN");
	else
		%this.chatMessage("\c6Whoa! Looks like you don't have the gui, please download it <a:" @ $Shop::Link @ ">here</a>\c6.");
}

function serverCmdShop(%this, %option, %cmd1, %cmd2, %cmd3, %cmd4, %cmd5, %cmd6)
{
	%this.chatMessage("Using deprecated Shop command - \c3" @ %option);
	serverCmdDRShop(%this, %option, %cmd1, %cmd2, %cmd3, %cmd4, %cmd5, %cmd6);
}

function serverCmdDRShop_Settings(%client, %option, %cmd1, %cmd2)
{
	if(!%client.Shop_Client && %client.hasSpawnedOnce)
		%client.chatMessage("\c6Looks like you don't have the gui, please download it <a:" @ $Shop::Link @ ">here</a>\c6.");

	switch$(%option)
	{
		case "HUD":
			%client.DR_HUD = %cmd1;

		case "GUIHUD":
			%client.DR_GUIHUD = %cmd1;

		case "GUIPerRound":
			%client.DR_GUIPerRound = %cmd1;

		case "MapGUI":
			%client.DR_MapGUI = %cmd1;

		case "HUDPassenger":
			%client.DR_HUDPassenger = %cmd1;

		default:
			if(%option !$= "")
				%client.chatMessage("\c6Invalid setting to set in GUI (" @ %option @ ")!");
	}
}

function serverCmdDRShop(%this, %option, %cmd1, %cmd2, %cmd3, %cmd4, %cmd5, %cmd6)
{
	if(!isObject(%group = getDRShopGroup()))
		return;

	if(!%this.Shop_Client && %this.hasSpawnedOnce)
		%this.chatMessage("\c6Looks like you don't have the gui, please download it <a:" @ $Shop::Link @ ">here</a>\c6.");

	if(%this.DRClient_Debug)
		%this.chatMessage(" DeathRaceClient --> serverCmdDRShop('" @ %option @ "', '" @ %cmd1 @ "', '" @ %cmd2 @ "', '" @ %cmd3 @ "', '" @ %cmd4 @ "', '" @ %cmd5 @ "', '" @ %cmd6 @ "');");

	switch$(%option)
	{
		case "Achievements":
			%this.sendAchievements();

		case "Titles":
			%this.sendTitles();

		case "Leaderboard":
			%this.sendLeaderboard();

		case "Profile":
			%playTime = %this.getTotalPlayTime();
			%days = mFloor(%playTime / 86400);
			%hours = mFloor(%playTime / 3600);
			%minutes = mFloor((%playTime % 3600) / 60);
			// %seconds = mFloor(%playTime % 3600 % 60);

			%rank = %this.getLeaderboardRank();
			%leaderboard = %this.getLeaderboardNumber();

			commandToClient(%this, 'DRShop', "SetProfile", "Playtime", %days, %hours, %minutes);
			commandToClient(%this, 'DRShop', "SetProfile", "Rank", %rank);
			commandToClient(%this, 'DRShop', "SetProfile", "Leaderboard", %leaderboard);
			commandToClient(%this, 'DRShop', "SetProfile", "Kills", %this.DR_totalKills);
			commandToClient(%this, 'DRShop', "SetProfile", "Deaths", %this.DR_totalDeaths | 0);
			commandToClient(%this, 'DRShop', "SetProfile", "KDR", mFloatLength(%this.DR_totalKills / %this.DR_totalDeaths, 2));
			commandToClient(%this, 'DRShop', "SetProfile", "Points", %this.DR_totalPoints | 0);
			commandToClient(%this, 'DRShop', "SetProfile", "Items", %this.DR_totalItemsBought | 0);
			commandToClient(%this, 'DRShop', "SetProfile", "Rounds", %this.DR_totalRounds | 0);
			commandToClient(%this, 'DRShop', "SetProfile", "WinsByButton", %this.DR_totalWinsByButton | 0);
			commandToClient(%this, 'DRShop', "SetProfile", "Wins", %this.DR_totalWins | 0);

		case "SAVE":
			%this.Shop_Save();

		case "LOAD":
			if(!%this.minigame.noitems)
				%this.Shop_Load();

		case "AUTOLOAD":
			%this.Shop_AutoLoad();

		case "BUY":
			if(!%this.minigame.noitems)
				%this.RequestItem(%cmd1, 5);

		case "DISPLAY":
			%this.DisplayItem(%cmd1, 5);

		case "ADD":
			if(!%this.Shop_Client)
				return;

			if(!%this.isSuperAdmin)
				return;

			if(!isObject(%item = findItemByName(%cmd1)))
			{
				commandToClient(%this, 'DRShop_MessageBoxOk', "Item - Error", "Item does not exist.");
				return;
			}

			if(isObject(%group.findScript(%cmd1)))
			{
				commandToClient(%this, 'DRShop_MessageBoxOk', "Item - Error", "Item already exists, please delete it or modify it.");
				return;
			}

			if(%item.getClassName() !$= "ItemData")
			{
				commandToClient(%this, 'DRShop_MessageBoxOk', "Item - Error", "Item is a different class name. You should never see this error.");
				return;
			}

			%obj = registerDRShopItem(%item.uiName, "datablockName " @ %item.uiName TAB
				"shopClass " @ %cmd2 TAB
				"cost " @ %cmd3 TAB
				"description " @ %cmd4 TAB
				"buyOnce " @ getWord(%cmd5, 0) TAB 
				"weaponLimit " @ getWord(%cmd5, 3) TAB 
				"canSave " @ getWord(%cmd5, 4) TAB 
				"adminLevel "  @ (getWord(%cmd5, 2) ? 2 : getWord(%cmd5, 1)));

			if(isObject(%obj) && $Server::Shop::Debug)
			{
				messageAll('',"Attempted to add \c4" @ %item.uiName @ " \cr(\c3" @ %item.getName() @ "\cr) \c7- \c3" @ %this.getPlayerName());
				messageAll('',"  -> Cost: \c4" @ %cmd3);
				messageAll('',"  -> Category: \c4" @ %cmd2);
				messageAll('',"  -> Description: \c4" @ %cmd4);
				messageAll('',"  -> Admin level: \c4" @ (getWord(%cmd5, 2) ? 2 : getWord(%cmd5, 1)));
				messageAll('',"  -> Buy once: \c4" @ getWord(%cmd5, 0));
				messageAll('',"  -> Can save: \c4" @ getWord(%cmd5, 4));
				messageAll('',"  -> Saved name: \c4" @ %obj.getName());
			}
			else if (isObject(%obj))
				messageAll('MsgAdminForce',"\c0" @ %this.getPlayerName() @ " \c6has added an item: \c3" @ %item.uiName);

		case "DELETE":
			if(!%this.Shop_Client)
				return;

			if(!%this.isSuperAdmin)
				return;

			%item = %cmd1;

			%obj = findItemByName(%item);
			if(!isObject(%obj)) return;
			%groupObj = getDRShopGroup().findScript(%obj.uiName);
			if(isObject(%groupObj))
			{
				if(%groupObj.cannotModify)
					commandToClient(%this, 'MessageBoxOK', "Oops!", "This object cannot be modified.<br>" @ %item);
				else
				{
					messageAll('MsgAdminForce',"\c0" @ %this.getPlayerName() @ " \c6has deleted an item: \c3" @ %groupObj.uiName);
					if(isFile(%file = getDRShopGroup().filePath @ stripChars(%groupObj.uiName, $Shop::Chars) @ ".cs"))
						fileDelete(%file);

					%groupObj.delete();
					Shop_SendDataToAllClients();
				}
			}
			else
				commandToClient(%this, 'MessageBoxOK', "Oops!", "This object doesn't seem to exist.<br>" @ %item);

		case "REFRESH":
			if(!%this.Shop_Client || getSimTime() - %this.Shop_Refresh < 5000)
			{
				%this.chatMessage("Unable to refresh. Try again later.");
				return;
			}

			%this.Shop_Refresh = getSimTime();
			%this.Shop_SendData();

		case "EDIT":
			if(!%this.Shop_Client)
				return;

			if(!%this.isSuperAdmin)
				return;

			%item = %cmd1;
			%value = %cmd2;
			%description = %cmd3;
			%shopClass = %cmd4;
			%canSave = getField(%cmd5, 0);
			%buyOnce = getField(%cmd5, 1);
			%adminLevel = getField(%cmd5, 2);
			switch$(%adminLevel)
			{
				case "Admin" or 1:
					%adminLevelMsg = "Admins";
					%adminLevel = 1;

				case "Super admin" or 2:
					%adminLevelMsg = "Super admins";
					%adminLevel = 2;

				default: //0
					%adminLevelMsg = "Anyone";
					%adminLevel = 0;
			}

			%obj = findItemByName(%item);
			if(!isObject(%obj)) return;
			%groupObj = getDRShopGroup().findScript(%obj.uiName);
			if(isObject(%groupObj))
			{
				if(%groupObj.cannotModify)
					commandToClient(%this, 'MessageBoxOK', "Oops!", "This object cannot be modified.<br>" @ %item);
				else
				{
					if(%value != %groupObj.cost)
					{
						messageAll('MsgAdminForce',"\c4" @ %this.getPlayerName() @ " \c6has updated an item:\c3 " @ %groupObj.uiName);
						%msg = 1;
						%groupObj.cost = %value;
						messageAll('',"   \c6Now costs \c3" @ %value @ " points");
						%counte++;
					}

					if(%description !$= %groupObj.description && trim(%description) !$= "")
					{
						if(!%msg)
						{
							%msg = 1;
							messageAll('MsgAdminForce',"\c4" @ %this.getPlayerName() @ " \c6has updated an item:\c3 " @ %groupObj.uiName);
						}

						%groupObj.description = %description;
						messageAll('',"   \c6Description has been changed.");
						messageAll('',"       - \c6" @ %description);
						%counte++;
					}

					if(%shopClass !$= %groupObj.shopClass && trim(%shopClass) !$= "")
					{
						if(!%msg)
						{
							%msg = 1;
							messageAll('MsgAdminForce',"\c4" @ %this.getPlayerName() @ " \c6has updated an item:\c3 " @ %groupObj.uiName);
						}

						%oldShopClass = %groupObj.shopClass;
						%groupObj.shopClass = %shopClass;
						%categoryList = DRShopGroup.shopCategories;
						if(isObject(%categoryList))
						{
							%idx = %categoryList.findTextIndexHack(%oldShopClass);
							%count = getField(%categoryList.getRowText(%id), 1)-1;
							if(%idx != -1) // index? remove it
								%categoryList.setRowByID(%idx, %oldShopClass TAB %count);

							if(%count <= 0)
								%categoryList.removeRow(%idx);

							%idx = %categoryList.findTextIndexHack(%shopClass);
							%count = getField(%categoryList.getRowText(%id), 1)+1;
							if(%idx != -1) // index? add it
								%categoryList.setRowByID(%idx, %shopClass TAB %count);
							else
								%categoryList.addRow(%categoryList.rowCount(), %shopClass TAB 1, %categoryList.rowCount());

							%categoryList.sort(0);
						}

						messageAll('',"   \c6Category has been changed.");
						messageAll('',"       \c6Belongs to: \c3" @ %shopClass);
						%counte++;
					}

					if(%canSave != %groupObj.canSave && trim(%canSave) !$= "")
					{
						if(!%msg)
						{
							%msg = 1;
							messageAll('MsgAdminForce',"\c4" @ %this.getPlayerName() @ " \c6has updated an item:\c3 " @ %groupObj.uiName);
						}

						%groupObj.canSave = %canSave;
						messageAll('',"   \c6Saving this item is now " @ (%canSave ? "\c2true\c6." : "\c0false\c6."));
						%counte++;
					}

					if(%buyOnce != %groupObj.buyOnce && trim(%buyOnce) !$= "")
					{
						if(!%msg)
						{
							%msg = 1;
							messageAll('MsgAdminForce',"\c4" @ %this.getPlayerName() @ " \c6has updated an item:\c3 " @ %groupObj.uiName);
						}

						%groupObj.buyOnce = %buyOnce;
						messageAll('',"   \c6Buying this item once is now " @ (%buyOnce ? "\c2true\c6." : "\c0false\c6. You have to always keep buying this."));
						%counte++;
					}

					if(%adminLevel != %groupObj.adminLevel && trim(%adminLevel) !$= "")
					{
						if(!%msg)
						{
							%msg = 1;
							messageAll('MsgAdminForce',"\c4" @ %this.getPlayerName() @ " \c6has updated an item:\c3 " @ %groupObj.uiName);
						}

						%groupObj.adminLevel = %adminLevel;
						messageAll('',"   \c6Permission to use this weapon: \c3" @ %adminLevelMsg);
						%counte++;
					}

					if(%counte > 0)
					{
						%groupObj.save(%groupObj.getGroup().filePath @ stripChars(%groupObj.uiName, $Shop::Chars) @ ".cs");
						Shop_SendSingleDataToAllClients(%groupObj);
					}
				}
			}
			else
				commandToClient(%this, 'MessageBoxOK', "Oops!", "This object doesn't seem to exist.<br>" @ %item);

		case "HANDSHAKE":
			echo(%this.getPlayerName() @ " is sending a handshake command.");
			if(!%this.Shop_Client && %this.Shop_Password $= %cmd2)
			{
				echo(" - Replied to the handshake with version " @ %cmd1 @ "/" @ $Shop::Server::Version @ ". (Password accepted)");
				%this.Shop_Password = "";
				if(%cmd1 >= $Shop::Server::Version)
				{
					echo("    - Successful.");
					%this.Shop_Client = 1;

					%this.Shop_SendData();
				}
				else
				{
					if(%cmd1 >= $Shop::Server::RequiredVersion)
					{
						echo("    - Outdated client, but usable.");
						%this.chatMessage("<font:arial bold:22>\c6Your client is \c3out of date\c6! You are allowed to still use your client, but some features may not be available. Download <a:" @ $Shop::Link @ ">here</a> for the updated client.");
					}
					else
					{
						%this.Shop_Client = 0;
						echo("    - Outdated client. Disabled.");
						%this.chatMessage("<font:arial bold:22>\c6Your client is \c3out of date\c6! Please download one from <a:" @ $Shop::Link @ ">here</a>. Your client GUI has been \c0disabled\c6.");
					}

					%this.Shop_OutOfDate = 1;
				}
			}

		default:
			%this.chatMessage("\c6You did not request an option (\c3" @ %option @ "\c6).");
	}
}

function serverCmdShop_AcceptItem(%this)
{
	if(!isObject(%this.itemRequest)) %this.itemRequest = getDRShopGroup().findScript(%this.itemRequest);
	if(!isObject(%this.itemRequest))
	{ 
		%this.chatMessage("\c6You can not accept an agreement from nothing!");
		return;
	}
	%this.BuyItem(%this.itemRequest, %this.timeRequest);
	%this.itemRequest = 0;
	%this.timeRequest = 0;
}

/////////////////////////////////////////////////////////////////////////////////////

function GameConnection::Shop_MiniSpecial(%this, %command)
{
	if(!isObject(%mini = %this.minigame))
		return;

	if(%mini.DR_SpecialBought != 0)
	{
		%this.chatMessage("\c6Sorry! Minigame special is already bought for next round.");
		return;
	}

	if($Sim::Time - %mini.lastSpecialBought < (6 * 60))
	{
		%this.chatMessage("\c6Sorry! Minigame special cannot be bought right now (cool down). Please try again later.");
		return;	
	}

	switch$(%command)
	{
		case "CrazySpeed":
			%this.chatMessage("\c6You have bought crazy speed for next round!");
			%mini.messageAll('', '\c3%1 \c6has bought a special for next round: \c4Crazy speed', %this.getPlayerName());
			%mini.DR_SpecialBought = 1;
			%this.incScore(-mAbs(%this.TempDR_Shop));
			%this.TempDR_Shop = 0;
			%mini.lastSpecialBought = $Sim::Time;

		case "VehicleScale":
			%this.chatMessage("\c6You have bought bigger vehicles for next round!");
			%mini.messageAll('', '\c3%1 \c6has bought a special for next round: \c4Bigger vehicles', %this.getPlayerName());
			%mini.DR_SpecialBought = 3;
			%this.incScore(-mAbs(%this.TempDR_Shop));
			%this.TempDR_Shop = 0;
			%mini.lastSpecialBought = $Sim::Time;

		case "Horse":
			%this.chatMessage("\c6You have bought horses for next round!");
			%mini.messageAll('', '\c3%1 \c6has bought a special for next round: \c4Horses', %this.getPlayerName());
			%mini.DR_SpecialBought = 11;
			%this.incScore(-mAbs(%this.TempDR_Shop));
			%this.TempDR_Shop = 0;
			%mini.lastSpecialBought = $Sim::Time;

		default:
			%this.chatMessage("\c6Invalid minigame special to buy!");
	}
}

function GameConnection::Shop_VehicleUpgrades(%this, %command)
{
	if(!isObject(%player = %this.player))
		return;

	if(%player.getState() $= "dead")
		return;

	if(!isObject(%vehicle = %player.getObjectMount()))
		return;
	
	if(!(%vehicle.getType() & $TypeMasks::VehicleObjectType))
		return;

	if(!isObject(%brick = %vehicle.spawnBrick))
		return;

	if(isObject(%mini = %this.minigame) && %mini.DR_time <= 0 && !%this.bypassShop)
		return;

	switch$(%command)
	{
		case "Post-Apoc Pickup":
			if($Pref::Server::DeathRace_AnnounceVehiclePurchase)
				announce(%this.getPlayerName() @ " has bought a vehicle: \c3Post-Apoc Pickup");
			%this.chatMessage("\c6Vehicle set to \c3Post-Apoc Pickup\c6.");
 			%brick.setVehicle(nameToID(findVehicleByName("Post Apocalyptic Pickup")));
 			if(isObject(%newVehicle = %brick.vehicle))
 			{
 				%newVehicle.mountObject(%player, 0);
 				if(isObject(%mini))
 				{
	 				if(%mini.DR_RandomVehicleScale)
	 					%newVehicle.schedule(100, setScale, getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
	 				else
						%newVehicle.schedule(100, setScale, vectorScale("1 1 1", %mini.vehicleScale));
				}
 			}

 			if(isObject(%mini))
 				%brick.schedule(50, setVehiclePowered, %mini.DR_time <= 0, %this);
                 	
                case "Euro Turbo":
			if($Pref::Server::DeathRace_AnnounceVehiclePurchase)
				announce(%this.getPlayerName() @ " has bought a vehicle: \c3Euro Turbo");
			%this.chatMessage("\c6Vehicle set to \c3Euro Turbo\c6.");
 			%brick.setVehicle(nameToID(findVehicleByName("Euro Turbo")));
 			if(isObject(%newVehicle = %brick.vehicle))
 			{
 				%newVehicle.mountObject(%player, 0);
 				if(isObject(%mini))
 				{
	 				if(%mini.DR_RandomVehicleScale)
	 					%newVehicle.schedule(100, setScale, getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
	 				else
						%newVehicle.schedule(100, setScale, vectorScale("1 1 1", %mini.vehicleScale));
				}
 			}

 			if(isObject(%mini))
 				%brick.schedule(50, setVehiclePowered, %mini.DR_time <= 0, %this);
                  

 		case "Post-Apoc Fordor":
 			if($Pref::Server::DeathRace_AnnounceVehiclePurchase)
 				announce(%this.getPlayerName() @ " has bought a vehicle: \c3Post-Apoc Fordor");
			%this.chatMessage("\c6Vehicle set to \c3Post-Apoc Fordor\c6.");
 			%brick.setVehicle(nameToID(findVehicleByName("Post Apocalyptic Fordor")));
 			if(isObject(%newVehicle = %brick.vehicle))
 			{
 				%newVehicle.mountObject(%player, 0);
 				if(isObject(%mini))
 				{
	 				if(%mini.DR_RandomVehicleScale)
	 					%newVehicle.schedule(100, setScale, getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
	 				else
						%newVehicle.schedule(100, setScale, vectorScale("1 1 1", %mini.vehicleScale));
				}
 			}

 			if(isObject(%mini = %this.minigame))
 				%brick.schedule(50, setVehiclePowered, %mini.DR_time <= 0, %this);

 		case "Tank":
 			if($Pref::Server::DeathRace_AnnounceVehiclePurchase)
 				announce(%this.getPlayerName() @ " has bought a vehicle: \c3Tank (IFV Puma)");
			%this.chatMessage("\c6Vehicle set to \c3Tank (IFV Puma)\c6.");
 			%brick.setVehicle(nameToID(findVehicleByName("IFV Puma")));
 			if(isObject(%newVehicle = %brick.vehicle))
 			{
 				%newVehicle.mountObject(%player, 0);
 				if(isObject(%mini))
 				{
	 				if(%mini.DR_RandomVehicleScale)
	 					%newVehicle.schedule(100, setScale, getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
	 				else
						%newVehicle.schedule(100, setScale, vectorScale("1 1 1", %mini.vehicleScale));
				}
 			}

 			if(isObject(%mini = %this.minigame))
 				%brick.schedule(50, setVehiclePowered, %mini.DR_time <= 0, %this);

 		case "Jeep":
 			if($Pref::Server::DeathRace_AnnounceVehiclePurchase)
 				announce(%this.getPlayerName() @ " has bought a vehicle: \c3Jeep");
			%this.chatMessage("\c6Vehicle set to \c3Jeep\c6.");
 			%brick.setVehicle(nameToID(findVehicleByName("Jeep")));
 			if(isObject(%newVehicle = %brick.vehicle))
 			{
 				%newVehicle.mountObject(%player, 0);
 				if(isObject(%mini))
 				{
	 				if(%mini.DR_RandomVehicleScale)
	 					%newVehicle.schedule(100, setScale, getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
	 				else
						%newVehicle.schedule(100, setScale, vectorScale("1 1 1", %mini.vehicleScale));
				}
 			}
                        
                        if(isObject(%mini = %this.minigame))
 				%brick.schedule(50, setVehiclePowered, %mini.DR_time <= 0, %this);

 		case "Phantom":
 			if($Pref::Server::DeathRace_AnnounceVehiclePurchase)
 				announce(%this.getPlayerName() @ " has bought a vehicle: \c3Phantom");
			%this.chatMessage("\c6Vehicle set to \c3Phantom\c6.");
 			%brick.setVehicle(nameToID(findVehicleByName("Phantom")));
 			if(isObject(%newVehicle = %brick.vehicle))
 			{
 				%newVehicle.mountObject(%player, 0);
 				if(isObject(%mini))
 				{
	 				if(%mini.DR_RandomVehicleScale)
	 					%newVehicle.schedule(100, setScale, getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
	 				else
						%newVehicle.schedule(100, setScale, vectorScale("1 1 1", %mini.vehicleScale));
				}
 			}

 			if(isObject(%mini))
 				%brick.schedule(50, setVehiclePowered, %mini.DR_time <= 0, %this);

 		case "Oldtimer":
 			if($Pref::Server::DeathRace_AnnounceVehiclePurchase)
 				announce(%this.getPlayerName() @ " has bought a vehicle: \c3Oldtimer");
			%this.chatMessage("\c6Vehicle set to \c3Oldtimer\c6.");
 			%brick.setVehicle(nameToID(findVehicleByName("Oldtimer")));
 			if(isObject(%newVehicle = %brick.vehicle))
 			{
 				%newVehicle.mountObject(%player, 0);
 				if(isObject(%mini))
 				{
	 				if(%mini.DR_RandomVehicleScale)
	 					%newVehicle.schedule(100, setScale, getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
	 				else
						%newVehicle.schedule(100, setScale, vectorScale("1 1 1", %mini.vehicleScale));
				}
 			}
                        
                        
 			if(isObject(%mini))
 				%brick.schedule(50, setVehiclePowered, %mini.DR_time <= 0, %this);

 		case "Skateboard":
 			if($Pref::Server::DeathRace_AnnounceVehiclePurchase)
 				announce(%this.getPlayerName() @ " has bought a vehicle: \c3Skateboard");
			%this.chatMessage("\c6Vehicle set to \c3Skateboard\c6.");
 			%brick.setVehicle(nameToID(findVehicleByName("Basic Skateboard")));
 			if(isObject(%newVehicle = %brick.vehicle))
 			{
 				%newVehicle.mountObject(%player, 0);
 				if(isObject(%mini))
 				{
	 				if(%mini.DR_RandomVehicleScale)
	 					%newVehicle.schedule(100, setScale, getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
	 				else
						%newVehicle.schedule(100, setScale, vectorScale("1 1 1", %mini.vehicleScale));
				}
 			}

 			if(isObject(%mini = %this.minigame))
 				%brick.schedule(50, setVehiclePowered, %mini.DR_time <= 0, %this);

 		case "Invisible":
 			if($Pref::Server::DeathRace_AnnounceVehiclePurchase)
 				announce(%this.getPlayerName() @ " has bought a vehicle upgrade: \c3Invisibility");
			%this.chatMessage("\c6Vehicle upgrade added: \c3Invisibility");
 			%vehicle.hideNode("ALL");

		default:
			%error = 1;
			%this.chatMessage("\c6Invalid option to choose for player upgrades.");
	}

	if(!%error)
	{
		if(isObject(%vehicle = %brick.vehicle) && %vehicle.getClassName() !$= "AIPlayer" && (%speed = %vehicle.getDatablock().maxWheelSpeed) > 20)
			%vehicle.getDatablock().maxWheelSpeed = 30;

		%this.incScore(-mAbs(%this.TempDR_Shop));
	}

	%this.TempDR_Shop = 0;
}

function GameConnection::Shop_Bots(%this, %command)
{
	if(!isObject(%player = %this.player))
		return;

	if(%player.getState() $= "dead")
		return;

	if(!isObject(%vehicle = %player.getObjectMount()))
		return;
	
	if(!(%vehicle.getType() & $TypeMasks::VehicleObjectType))
		return;

	if(!isObject(%brick = %vehicle.spawnBrick))
		return;

	if(isObject(%mini = %this.minigame) && %mini.DR_time <= 0 && !%this.bypassShop)
		return;

	switch$(%command)
	{
 		case "Driver":
 			if($Pref::Server::DeathRace_AnnounceVehiclePurchase)
 				announce(%this.getPlayerName() @ " has bought a bot: \c3Driver");
			%this.chatMessage("\c6Bot purchased: \c3Driver\cr. (Does not work yet)");
 			%brick.spawnVehicle();
 			if(isObject(%newVehicle = %brick.vehicle))
 			{
 				%newVehicle.mountObject(%player, 0);
 				//%newVehicle.schedule(100, setScale, vectorScale("1 1 1", %mini.vehicleScale));
 			}

 			if(isObject(%mini = %this.minigame))
 				%brick.schedule(50, setVehiclePowered, %mini.DR_time <= 0, %this);

		default:
			%error = 1;
			%this.chatMessage("Invalid option to choose for bots.");
	}

	if(!%error)
		%this.incScore(-mAbs(%this.TempDR_Shop));

	%this.TempDR_Shop = 0;
}

function GameConnection::Shop_TrailUpgrades(%client, %command)
{
	if(!isObject(%player = %client.player))
		return;

	if(%player.getState() $= "dead")
		return;

	if(isTrail(%command) || %command $= "None")
	{
		if($BoughtItem_[%client.getBLID(), getSafeVariableName("Trail - " @ %command)])
		{
			//%client.chatMessage("\c6Trail already purchased: \c3" @ %command @ "\c6. Say /Trail for more information!");
			serverCmdTrail(%client, %command);
			%client.TempDR_Shop = 0;
			return;
		}

		%client.incScore(-mAbs(%client.TempDR_Shop));
		$BoughtItem_[%client.getBLID(), getSafeVariableName("Trail - " @ %command)] = 1;
		commandToClient(%client, 'DRShop', "SET_BOUGHT", "Trail - " @ %command, 1);
		%client.chatMessage("\c6Trail purchased: \c3" @ %command @ "\c6. Say /Trail for more information!");
	}
	else
	{
		%client.chatMessage("\c6Invalid trail to buy!");
	}	

	%client.TempDR_Shop = 0;
}

function GameConnection::Shop_PlayerUpgrades(%this, %command)
{
	if(!isObject(%player = %this.player))
		return;

	if(%player.getState() $= "dead")
		return;

	switch$(%command)
	{
		case "Health":
			%player.addMaxHealth(10);
			%this.chatMessage("\c6Max health is now at \c3" @ %player.getMaxHealth() @ " (+10)\c6.");

		default:
			%error = 1;
			%this.chatMessage("\c6Invalid option to choose for player upgrades.");
	}

	if(!%error)
		%this.incScore(-mAbs(%this.TempDR_Shop));

	%this.TempDR_Shop = 0;
}

function Player::Shop_Load(%this, %bypass)
{
	if(!isObject(%this)) return;
	if(%this.getState() $= "dead") return;
	%client = %this.client;
	if(!isObject(%client)) return;
	if(isObject(%client.Shop_LoadFile)) {%client.Shop_LoadFile.close(); %client.Shop_LoadFile.delete();}

	if(isObject(%mini = getMiniGameFromObject(%this)) && !%bypass)
		if(%mini.DR_time <= 0)
		{
			%client.chatMessage("\c6Cannot load weapons. Race already started :D");
			return;
		}

	if(%mini.noitems)
		return;

	%client.Shop_LoadFileCheck = new fileobject();
	%client.Shop_LoadFileCheck.openforread("config/server/SavedItems/" @%client.getBLID() @ ".txt");
	for(%h=0;%h<%this.getDatablock().maxTools;%h++)
	{
		%weapon = %client.Shop_LoadFileCheck.readLine();
		if(isObject(%weapon))
			%count++;
	}
	%client.Shop_LoadFileCheck.close();
	%client.Shop_LoadFileCheck.delete();

	if(%count <= 0)
	{
		%client.chatMessage("\c6You don't have any weapons to load!");
		return;
	}

	%this.clearTools();

	%client.Shop_LoadFile = new fileobject();
	%client.Shop_LoadFile.openforread("config/server/SavedItems/" @%client.getBLID() @ ".txt");
	for(%i=0;%i<%this.getDatablock().maxTools;%i++)
	{
		%weapon = %client.Shop_LoadFile.readLine();
		if(isObject(%weapon))
		{
			if(isObject(%shopObj = getDRShopGroup().findScript(%weapon.uiName)))
			{
				if(%shopObj.canSave || %shopObj.canSave $= "")
				{
					%shopObj.canSave = 1;
					%this.addNewItem(%weapon, 1);
				}
				else
					%client.chatMessage("\c6Sorry, you can't load this item: \c3" @ %weapon.uiName);
			}
			//else
			//	%this.addNewItem(%weapon);
		}
	}
	%client.Shop_LoadFile.close();
	%client.Shop_LoadFile.delete();
	%client.chatMessage("\c6You have loaded your weapons.");
}

function Player::Shop_Save(%this)
{
	if(!isObject(%this)) return;
	if(%this.getState() $= "dead") return;
	%client = %this.client;
	if(!isObject(%client)) return;
	if(isObject(%client.Shop_SaveFile)) {%client.Shop_SaveFile.close(); %client.Shop_SaveFile.delete();}

	for(%j = 0;%j < %this.getDatablock().maxTools; %j++)
		if(isObject(%this.tool[%j])) %count++;

	if(%count <= 0)
	{
		%client.chatMessage("You don't have any weapons to save!");
		return;
	}

	if(%client.minigame.noitems)
		return;

	%client.Shop_SaveFile = new fileobject();
	%client.Shop_SaveFile.openforwrite("config/server/SavedItems/" @ %client.getBLID() @ ".txt");
	//talk(%client.getPlayerName() @ " -> Saving items");
	for(%i = 0;%i < %this.getDatablock().maxTools; %i++)
	{
		if(isObject(%tool = %this.tool[%i]))
		{
			%weapon = %tool.getName();
			//talk("   --> " @ %weapon);
			if(isObject(%shopObj = getDRShopGroup().findScript(%weapon.uiName)))
			{
				if(%shopObj.canSave || %shopObj.canSave $= "")
				{
					%shopObj.canSave = 1;
					%client.Shop_SaveFile.writeLine(%weapon);
				}
				else
					%cannotSaveCount++;
			}
			// else
			//	%client.Shop_SaveFile.writeLine(%weapon);
		}
	}
	%client.Shop_SaveFile.close();
	%client.Shop_SaveFile.delete();
	%client.chatMessage("\c6You have saved your weapons.");
	if(%cannotSaveCount > 0)
		%client.chatMessage(" \c6- You have 1 or more items that were not saved and are removed.");
}

function GameConnection::RequestItem(%this,%item,%time)
{
	if(%this.minigame.noitems)
		return;
	
	if(isObject(%item))
		%item = %item.uiName;
	
	%groupObj = getDRShopGroup().findScript(%item);
	if(isObject(%groupObj))
	{
		%groupObj = getDRShopGroup().findScript(%item);
		if(!isObject(%groupObj))
		{
			%this.chatMessage("Invalid shop item to request.");
			return 0;
		}

		//Player function stuff
		if(isFunction(%this.getClassName(), %groupObj.func))
		{
			%adminLevel = 0 + %this.isSuperAdmin + %this.isAdmin;
			if(%adminLevel < %groupObj.adminLevel)
			{
				if(%this.Shop_Client) commandToClient(%this,'DRShop_MessageBoxOK',"Shop - ERROR","You cannot purchase this item at this time!");
				else %this.chatMessage("<font:impact:20>ERROR: You cannot purchase this item at this time.",%time);
				return -1;
			}

			if(%this.getScore() < %groupObj.cost && !%this.bypassShop && %groupObj.cost > 0 && !$BoughtItem_[%this.getBLID(), getSafeVariableName(%groupObj.uiName)])
			{
				if(%this.Shop_Client) commandToClient(%this,'DRShop_MessageBoxOK',"Shop - ERROR","Not enough points!");
				else %this.chatMessage("<font:impact:20>ERROR: Not enough points!");
				return;
			}

			if(!%this.bypassShop)
				%this.TempDR_Shop = %groupObj.cost;

			%this.call(%groupObj.func, %groupObj.func_call);
			return 1;
		}
	}
	else
	{
		%this.chatMessage("\c6Contact a super admin, something is wrong with the shop system. (Invalid object \"" @ %item @ "\")");
		return 0;
	}

	if(%time < 1) %time = 3;
	%this.itemRequest = 0;
	%this.itemRequest = %groupObj.uiName;
	%this.timeRequest = %time;

	%strName = stripChars(%groupObj.uiName, $Shop::Chars);
	%strName = strReplace(%strName, " ", "_");

	if($BoughtItem_[%this.getBLID(), %strName] || %groupObj.cost <= 0)
	{
		%this.BuyItem(%groupObj.uiName, %time);
		return;
	}

	if(isObject(%item = findItemByName(%groupObj.uiName)) && $BoughtItem_[%this.getBLID(), %item.getName()])
	{
		%this.BuyOldItem(%item.getName());
		return;
	}

	if(!%groupObj.canSave)
		%msg = "You cannot save this item.";

	if(!%groupObj.buyOnce)
	{
		if(%msg $= "")
			%msg = "You have to rebuy this item.";
		else
			%msg = %msg @ "<br>You have to rebuy this item.";
	}

	if(%msg !$= "")
		%msg = "<br><br>Warning:<br>" @ %msg;

	commandToClient(%this,'MessageBoxYesNo',"Buy: " @ %groupObj.uiName @ "?",
		"Purchase this for " @ %groupObj.cost @ " points?" @ %msg,
		'Shop_AcceptItem');
}

function GameConnection::BuyItem(%this,%item,%time)
{
	if(isObject(%item))
		%item = %item.uiName;

	%obj = findItemByName(%item);
	%groupObj = getDRShopGroup().findScript(%item);
	if(!isObject(%groupObj))
	{
		%this.chatMessage("\c6Invalid shop item (\"\c3" @ %item @ "\c6\"). Please contact your super administrator.");
		return 0;
	}

	%strName = stripChars(%groupObj.uiName, $Shop::Chars);
	%strName = strReplace(%strName, " ", "_");

	if(!%groupObj.buyOnce)
	{
		$BoughtItem_[%this.getBLID(), %strName] = 0;
		commandToClient(%this, 'DRShop', "SET_BOUGHT", %groupObj.uiName, $BoughtItem_[%this.getBLID(), %strName]);
	}

	if(%time < 1) %time = 3;
	%adminLevel = 0 + %this.isSuperAdmin + %this.isAdmin;
	if(%adminLevel < %groupObj.adminLevel && !$BoughtItem_[%this.getBLID(), %strName])
	{
		%this.centerPrint("<font:impact:20>ERROR: You cannot purchase this item at this time.",%time);
		return -1;
	}

	if(%this.getScore() < %groupObj.cost && !$BoughtItem_[%this.getBLID(), %strName] && !%this.bypassShop && %groupObj.cost > 0)
	{
		if(%this.Shop_Client) commandToClient(%this,'DRShop_MessageBoxOK',"Shop - ERROR","Not enough points!");
		%this.chatMessage("<font:impact:20>ERROR: Not enough points!");
		return;
	}
	if(!$BoughtItem_[%this.getBLID(), %strName] && !%this.bypassShop)
		%this.incScore(-%groupObj.cost);

	if($BoughtItem_[%this.getBLID(), %strName])
	{
		if(isObject(%pl=%this.player))
			if(%pl.addNewItem(%obj.uiName)) %this.chatMessage("\c3" @ %obj.uiname @ "\c6 added to your inventory.");

		return;
	}

	if(%groupObj.buyOnce)
	{
		$BoughtItem_[%this.getBLID(), %strName] = 1;
		if(%groupObj.cost > 0) %this.chatMessage("\c6You have bought this item: \c4" @ %groupObj.uiName @ "\c6, you now equip it! You do not ever have to buy this weapon again, yay!");
	}

	if(isObject(%pl=%this.player))
		if(%pl.addNewItem(%obj.uiName))
			%this.chatMessage("\c3" @ %obj.uiname @ "\c6 added to your inventory.");

	commandToClient(%this, 'DRShop', "SET_BOUGHT", %groupObj.uiName, $BoughtItem_[%this.getBLID(), %strName]);
	export("$BoughtItem_*", "config/server/ShopBuyPrefs.cs");
}

function GameConnection::BuyOldItem(%this, %item, %doNotEquip)
{
	%obj = findItemByName(%item);
	if(!isObject(%obj))
	{
		%this.chatMessage("Invalid item.");
		return 0;
	}

	if($BoughtItem_[%this.getBLID(), %obj.getName()])
	{
		if(isObject(%newItem = getDRShopGroup().findScript(%obj.uiName)))
		{
			%this.chatMessage("\c6We found an old item in our system, we will update this for you now - " @ %newItem.uiName);
			%strName = stripChars(%newItem.uiName, $Shop::Chars);
			%strName = strReplace(%strName, " ", "_");

			$BoughtItem_[%this.getBLID(), %strName] = 1;
			commandToClient(%this, 'DRShop', "SET_BOUGHT", %newItem.uiName, 1);
			if(!%doNotEquip)
				%this.BuyItem(%newItem.uiName);
		}
	}
}

function GameConnection::DisplayItem(%this,%item,%time)
{
	%obj = findItemByName(%item);
	if(!isObject(%obj))
	{
		%groupObj = getDRShopGroup().findScript(%item);
		if(!isObject(%groupObj))
		{
			%this.chatMessage("\c6Invalid shop item.");
			return 0;
		}

		%strName = stripChars(%groupObj.uiName, $Shop::Chars);
		%strName = strReplace(%strName, " ", "_");

		if(%time < 1) %time = 3;
		%adminLevel = 0 + %this.isSuperAdmin + %this.isAdmin;
		if(%adminLevel < %groupObj.adminLevel && !$BoughtItem_[%this.getBLID(), %strName]) 
		{
			%this.centerPrint("<font:impact:20>ERROR: This item cannot be displayed to you.",%time);
			return 0;
		}
		%cost = %groupObj.cost;
		if(%cost == 0) %cost = "FREE";
		else if($BoughtItem_[%this.getBLID(), %strName]) %cost = "\c3Already bought";
		else %cost = %cost @ " Points";
		%costCol = "\c2";
		if(%this.getScore() < %cost) %costCol = "\c0";
		%this.centerPrint("<font:impact:20>\c4" @ %groupObj.uiName @ " \c6- " @ %costCol @ %cost @ "<br><br>\c6" @
			%groupObj.description, %time);

		return 1;
	}

	%groupObj = getDRShopGroup().findScript(%obj.uiName);
	if(!isObject(%groupObj))
	{
		%this.chatMessage("\c6Invalid item.");
		return 0;
	}

	%strName = stripChars(%groupObj.uiName, $Shop::Chars);
	%strName = strReplace(%strName, " ", "_");

	if($BoughtItem_[%this.getBLID(), %obj.getName()] && !$BoughtItem_[%this.getBLID(), %strName])
		%this.BuyOldItem(%obj.getName(), 1);

	if(%time < 1) %time = 3;
	%adminLevel = 0 + %this.isSuperAdmin + %this.isAdmin;
	if(%adminLevel < %groupObj.adminLevel && !$BoughtItem_[%this.getBLID(), %strName]) 
	{
		%this.centerPrint("<font:impact:20>ERROR: This item cannot be displayed to you.",%time);
		return 0;
	}
	%cost = %groupObj.cost;
	if(%cost == 0) %cost = "FREE";
	else if($BoughtItem_[%this.getBLID(), %strName]) %cost = "\c3Already bought";
	else %cost = %cost @ " Points";
	%costCol = "\c2";
	if(%this.getScore() < %cost) %costCol = "\c0";
	%this.centerPrint("<font:impact:20>\c4" @ %groupObj.uiName @ " \c6- " @ %costCol @ %cost @ "<br><br>\c6" @
		%groupObj.description, %time);
}