// DeathRace file
function Server_LoadTitles()
{
	if(isObject(Server_TitleGroup))
		Server_TitleGroup.delete();

	new ScriptGroup(Server_TitleGroup)
	{
		class = ServerTitleSet;
	};

	//TODO - Add button presser, veteran driver

	new ScriptObject("Title_V8_Beta_Tester")					// Titles in safe name, only letters and numbers
	{
		class 			= "ServerTitle";						// Class - do not modify, nothing special for titles
		uiName 			= "V8 Beta";						// Name of achievement
		cost 			= 0;									// Cost of achievement, if <0 it will not be in shop
		hexColor 		= "FF8B1F";								// Hex color to display title, google "hex color picker"
		colorNameStr 	= "";									// Same as uiName but put your color tags in there, overrides hexColor
		font 	 		= "arial bold";							// Font size, leave blank if no font
		fontSize		= 22;									// Font size, leave blank if no font
		bl_idList	 	= "30966 48980 46426";					// List in IDs to instantly reward
		MLTags 			= ""; 									// Such as bitmap
	};



	new ScriptObject("Title_Executioner")
	{
		class 			= "ServerTitle";
		uiName 			= "Executioner";
		cost 			= 0;
		hexColor 		= "66B3FF";
		colorNameStr 	= "";
		font 	 		= "Comic Sans MS";
		fontSize		= 20;
		bl_idList	 	= "48980"; // Visolator
		MLTags 			= "";
	};

	new ScriptObject("Title_Badass_Host")
	{
		class 			= "ServerTitle";
		uiName 			= "Badass Host";
		cost 			= 0;
		hexColor 		= "0000FF";
		colorNameStr 	= "";
		font 	 		= "Arial Bold";
		fontSize		= 20;
		bl_idList	 	= "30966"; // Kong
		MLTags 			= "";
	};

	new ScriptObject("Title_MERASMUS")
	{
		class 			= "ServerTitle";
		uiName 			= "MERASMUS";
		cost 			= 0;
		hexColor 		= "C1002C";
		colorNameStr 	= "";
		font 	 		= "Impact";
		fontSize		= 35;
		bl_idList	 	= "33152"; // Trinko
		MLTags 			= "";
	};
	
	new ScriptObject("Title_SweatyServerGuy")
	{
		class 			= "ServerTitle";
		uiName 			= "SweatyServerGuy";
		cost 			= 0;
		hexColor 		= "D24DFF";
		colorNameStr 	= "";
		font 	 		= "impact";
		fontSize		= 20;
		bl_idList	 	= "3766"; // Xenos
		MLTags 			= "";
	};

	new ScriptObject("Title_Beastmode")
	{
		class 			= "ServerTitle";
		uiName 			= "Beastmode";
		cost 			= 0;
		hexColor 		= "FFA500";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "47279"; // BeasterBunny
		MLTags 			= "";
	};

	new ScriptObject("Title_Cancerous")
	{
		class 			= "ServerTitle";
		uiName 			= "Cancerous";
		cost 			= 0;
		hexColor 		= "FF6699";
		colorNameStr 	= "";
		font 	 		= "Comic Sans MS";
		fontSize		= 20;
		bl_idList	 	= "36522"; // Deneb
		MLTags 			= "";
	};

	new ScriptObject("Title_Custodian")
	{
		class 			= "ServerTitle";
		uiName 			= "Custodian";
		cost 			= 0;
		hexColor 		= "B00000";
		colorNameStr 	= "";
		font 	 		= "Ravie";
		fontSize		= 20;
		bl_idList	 	= "46426"; // Monoblaster
		MLTags 			= "";
	};

	new ScriptObject("Title_LetAPOSs_All_Love")
	{
		class 			= "ServerTitle";
		uiName 			= "Let's All Love";
		cost 			= 0;
		hexColor 		= "FFB3EC";
		colorNameStr 	= "";
		font 	 		= "Lucida Console";
		fontSize		= 20;
		bl_idList	 	= "11167"; // Ranma
		MLTags 			= "";
	};

	new ScriptObject("Title_Vin_Diesel")
	{
		class 			= "ServerTitle";
		uiName 			= "Vin Diesel";
		cost 			= 0;
		hexColor 		= "737373";
		colorNameStr 	= "";
		font 	 		= "Arial Bold";
		fontSize		= 20;
		bl_idList	 	= "12815"; // Jizzmo
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_Addicted")
	{
		class 			= "ServerTitle";
		uiName 			= "Addicted";
		cost 			= 0;
		hexColor 		= "FF3399";
		colorNameStr 	= "";
		font 	 		= "Impact";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Map_Maker")
	{
		class 			= "ServerTitle";
		uiName 			= "Map Maker";
		cost 			= 0;
		hexColor 		= "99FF66";
		colorNameStr 	= "";
		font 	 		= "Courier New";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Discord")
	{
		class 			= "ServerTitle";
		uiName 			= "Discord";
		cost 			= 0;
		hexColor 		= "0099FF";
		colorNameStr 	= "";
		font 	 		= "Verdana Bold";
		fontSize		= 18;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_Bloodthirsty")
	{
		class 			= "ServerTitle";
		uiName 			= "Bloodthirsty";
		cost 			= 0;
		hexColor 		= "B30000";
		colorNameStr 	= "";
		font 	 		= "chiller";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_i_tried")
	{
		class 			= "ServerTitle";
		uiName 			= "i tried";
		cost 			= 0;
		hexColor 		= "FFFFFF";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "<bitmap:base/client/ui/ci/star>";
	};

	new ScriptObject("Title_Annoying")
	{
		class 			= "ServerTitle";
		uiName 			= "Annoying";
		cost 			= 0;
		hexColor 		= "000099";
		colorNameStr 	= "";
		font 	 		= "Arial Bold";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "<div:1>";
	};

	// Achievement title
	new ScriptObject("Title_Uber")
	{
		class 			= "ServerTitle";
		uiName 			= "Uber";
		cost 			= 0;
		hexColor 		= "B000B0";
		colorNameStr 	= "";
		font 	 		= "Arial Bold";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_Demolition_Uber")
	{
		class 			= "ServerTitle";
		uiName 			= "Demolition Uber";
		cost 			= 0;
		hexColor 		= "800080";
		colorNameStr 	= "";
		font 	 		= "Arial Bold";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_Veteran_Driver")
	{
		class 			= "ServerTitle";
		uiName 			= "Veteran Driver";
		cost 			= 0;
		hexColor 		= "A742F5";
		colorNameStr 	= "";
		font 	 		= "Arial Bold";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_Button_Presser")
	{
		class 			= "ServerTitle";
		uiName 			= "Button Presser";
		cost 			= 0;
		hexColor 		= "F5424B";
		colorNameStr 	= "";
		font 	 		= "Arial Bold";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Admon")
	{
		class 			= "ServerTitle";
		uiName 			= "Admon";
		cost 			= 0;
		hexColor 		= "FFFFFF";
		colorNameStr 	= "<color:009999>A<color:33cccc>dmon";
		font 	 		= "impact";
		fontSize		= "20";
		bl_idList	 	= "admin";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_Unstoppable")
	{
		class 			= "ServerTitle";
		uiName 			= "Unstoppable";
		cost 			= 0;
		hexColor 		= "CC0000";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// -------------------- Map ------------------------ \\

	// Achievement title
	new ScriptObject("Title_Mt_Luneth")
	{
		class 			= "ServerTitle";
		uiName 			= "Mt. Luneth";
		cost 			= 0;
		hexColor 		= "B266FF";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_Tropical")
	{
		class 			= "ServerTitle";
		uiName 			= "Tropical";
		cost 			= 0;
		hexColor 		= "ED8756";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_Explosive")
	{
		class 			= "ServerTitle";
		uiName 			= "Explosive";
		cost 			= 0;
		hexColor 		= "B53737";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_Off_Road")
	{
		class 			= "ServerTitle";
		uiName 			= "Off Road";
		cost 			= 0;
		hexColor 		= "87CEEB";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_Aquifer")
	{
		class 			= "ServerTitle";
		uiName 			= "Aquifer";
		cost 			= 0;
		hexColor 		= "33A532";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_All_Terrain")
	{
		class 			= "ServerTitle";
		uiName 			= "All Terrain";
		cost 			= 0;
		hexColor 		= "654321";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// Achievement title
	new ScriptObject("Title_Chop_Wood")
	{
		class 			= "ServerTitle";
		uiName 			= "Chop Wood";
		cost 			= 0;
		hexColor 		= "CC0000";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	// -------------------- Shop ------------------------ \\

	new ScriptObject("Title_Overpowered")
	{
		class 			= "ServerTitle";
		uiName 			= "Overpowered";
		cost 			= 160;
		hexColor 		= "FFFFFF";
		colorNameStr 	= "<color:BFBFBF>Over<color:666666>powered";
		font 	 		= "Lucida Console";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Critical")
	{
		class 			= "ServerTitle";
		uiName 			= "Critical";
		cost 			= 40;
		hexColor 		= "FFFFFF";
		colorNameStr 	= "<color:00ff00>C<color:339933>ritical";
		font 	 		= "impact";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Fashionable")
	{
		class 			= "ServerTitle";
		uiName 			= "Fashionable";
		cost 			= 45;
		hexColor 		= "CC33FF";
		colorNameStr 	= "";
		font 	 		= "impact";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Dank")
	{
		class 			= "ServerTitle";
		uiName 			= "Dank";
		cost 			= 60;
		hexColor 		= "FFFFFF";
		colorNameStr 	= "<color:FF0000>D<color:CC0000>a<color:FF0000>n<color:CC0000>k";
		font 	 		= "impact";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Classic")
	{
		class 			= "ServerTitle";
		uiName 			= "Classic";
		cost 			= 80;
		hexColor 		= "FFA64D";
		colorNameStr 	= "";
		font 	 		= "Book Antiqua";
		fontSize		= 18;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Leader")
	{
		class 			= "ServerTitle";
		uiName 			= "Leader";
		cost 			= 75;
		hexColor 		= "CC0000";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_The_Glorious")
	{
		class 			= "ServerTitle";
		uiName 			= "The Glorious";
		cost 			= 120;
		hexColor 		= "00FFCC";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Driver")
	{
		class 			= "ServerTitle";
		uiName 			= "Driver";
		cost 			= 35;
		hexColor 		= "FF3333";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Electric")
	{
		class 			= "ServerTitle";
		uiName 			= "Electric";
		cost 			= 90;
		hexColor 		= "FFCC00";
		colorNameStr 	= "";
		font 	 		= "chiller";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Gladiator")
	{
		class 			= "ServerTitle";
		uiName 			= "Gladiator";
		cost 			= 62;
		hexColor 		= "75A3A3";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Racer")
	{
		class 			= "ServerTitle";
		uiName 			= "Racer";
		cost 			= 60;
		hexColor 		= "CC9900";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Moron")
	{
		class 			= "ServerTitle";
		uiName 			= "Moron";
		cost 			= 15;
		hexColor 		= "A6A6A6";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Ragequitter")
	{
		class 			= "ServerTitle";
		uiName 			= "Ragequitter";
		cost 			= 50;
		hexColor 		= "3366FF";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Baller")
	{
		class 			= "ServerTitle";
		uiName 			= "Baller";
		cost 			= 45;
		hexColor 		= "CC8800";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Jeep_Expert")
	{
		class 			= "ServerTitle";
		uiName 			= "Jeep Expert";
		cost 			= 55;
		hexColor 		= "B3CCFF";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_I_Break_Physics")
	{
		class 			= "ServerTitle";
		uiName 			= "I Break Physics";
		cost 			= 76;
		hexColor 		= "669900";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_The_Rektor")
	{
		class 			= "ServerTitle";
		uiName 			= "The Rektor";
		cost 			= 56;
		kills			= 20;
		hexColor 		= "9966FF";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_HEADSHOT")
	{
		class 			= "ServerTitle";
		uiName 			= "HEADSHOT";
		cost 			= 80;
		killsHeadshot 	= 5;
		hexColor 		= "FF8080";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_Camper")
	{
		class 			= "ServerTitle";
		uiName 			= "Camper";
		cost 			= 48;
		hexColor 		= "FFB84D";
		colorNameStr 	= "";
		font 	 		= "Comic Sans MS";
		fontSize		= 20;
		bl_idList	 	= "";
		MLTags 			= "";
	};

	new ScriptObject("Title_90APOSs_Kid")
	{
		class 			= "ServerTitle";
		uiName 			= "90's Kid";
		cost 			= 30;
		hexColor 		= "CC8800";
		colorNameStr 	= "";
		font 	 		= "";
		fontSize		= "";
		bl_idList	 	= "";
		MLTags 			= "";
	};

	$Server::TitlesLoaded = 1;
}

function ServerTitleSet::find(%group, %name)
{
	if(isObject(%name) && %group.isMember(%name))
		return %name;

	if(isObject(%foundIdx = %group.objIndex[getSafeVariableName(%name)]))
		return %foundIdx;

	%foundPartPos = 9999;
	for(%i = 0; %i < %group.getCount(); %i++)
	{
		%obj = %group.getObject(%i);

		if(%obj.getName() $= %name)
			%foundObjName = %obj;
		
		if(%obj.uiName $= %name)
			%foundFullName = %obj;
		
		if((%foundPos = striPos(%obj.uiName, %name)) >= 0 && %foundPos < %foundPartPos)
		{
			%foundPartName = %obj;
			%foundPartPos = %foundPos;
		}
	}

	if(isObject(%foundObjName))
		return %foundObjName;

	if(isObject(%foundFullName))
		return %foundFullName;

	if(isObject(%foundPartName))
		return %foundPartName;

	return 0;
}

function ServerTitle::onAdd(%title)
{
	if(isObject(%obj = Server_TitleGroup.find(%title.uiName)))
		%obj.delete();

	Server_TitleGroup.add(%title);

	if(%title.colorNameStr $= "")
	{
		if(%title.hexColor !$= "")
			%title.colorNameStr = "<color:" @ %title.hexColor @ ">" @ %title.uiName;
		else
			%title.colorNameStr = %title.uiName;
	}

	if(%title.font !$= "" && %title.fontSize > 10)
	{
		%title.fontStr = "<font:" @ %title.font @ ":" @ %title.fontSize @ ">";
	}

	%name 			= %title.uiName;
	%safeName 		= getSafeVariableName(%name);
	%description 	= %title.description;
	%cost 			= %title.cost;
	%colorNameStr 	= %title.colorNameStr;
	%font 			= %title.font;
	%fontSize 		= %title.fontSize;
	%MLTags 		= %title.MLTags;
	%realID 		= %title.getID();

	Server_TitleGroup.objIndex[%safeName] = %title;

	%clientCount = ClientGroup.getCount();
	for(%i = 0; %i < %clientCount; %i++)
	{
		%client = ClientGroup.getObject(%i);
		%bl_id 	= %client.getBLID();

		if(%client.Shop_Client)
		{
			%unlocked = $Server::Titles::Unlocked_[%bl_id, %safeName];
			if(%cost > 0 || %unlocked)
				commandToClient(%client, 'DRShop', "AddTitle", %name, %unlocked, %description, %cost, %colorNameStr, %font, %fontSize, %MLTags, %realID);
		}
	}
}

function ServerTitle::onRemove(%title)
{
	%safeName = getSafeVariableName(%title.uiName);
	if(Server_TitleGroup.objIndex[%safeName] !$= "")
		Server_TitleGroup.objIndex[%safeName] = 0;

	%clientCount = ClientGroup.getCount();
	for(%i = 0; %i < %clientCount; %i++)
	{
		%client = ClientGroup.getObject(%i);
		%bl_id 	= %client.getBLID();

		if(%client.Shop_Client && (%title.cost > 0 || $Server::Titles::Unlocked_[%bl_id, %safeName]))
		{
			commandToClient(%client, 'DRShop', "RemoveTitle", %name, %realID);
		}
	}
}

function GameConnection::lockTitle(%this, %title)
{
	%bl_id = %this.getBLID();
	if(!isObject(%title) || %title.class !$= "ServerTitle" || !Server_TitleGroup.isMember(%title))
		if(!isObject(%title = Server_TitleGroup.find(%title)))
			return -1;

	%safeName = getSafeVariableName(%title.uiName);
	if(!$Server::Titles::Unlocked_[%bl_id, %safeName])
		return 0;

	$Server::Titles::Unlocked_[%bl_id, %safeName] = 0;
	if(%this.Shop_Client)
		commandToClient(%this, 'DRShop', "setTitleLocked", %title, 1);

	echo(%this.getPlayerName() @ " has a locked title: " @ %title.uiName);
	export("$Server::Titles::Unlocked_" @ %bl_id @ "*", "config/server/SavedTitles/" @ %bl_id @ ".cs");
	return 1;
}

function GameConnection::unlockTitle(%this, %title)
{
	%bl_id = %this.getBLID();
	if(!isObject(%title) || %title.class !$= "ServerTitle" || !Server_TitleGroup.isMember(%title))
		if(!isObject(%title = Server_TitleGroup.find(%title, 1)))
			return -1;

	%safeName = getSafeVariableName(%title.uiName);
	if($Server::Titles::Unlocked_[%bl_id, %safeName])
		return 0;

	$Server::Titles::Unlocked_[%bl_id, %safeName] = 1;
	if(%this.Shop_Client)
		commandToClient(%this, 'DRShop', "setTitleLocked", %title, 0);

	%this.chatMessage("\c6You have a new title (/Titles): " @ %title.MLTags @ %title.fontStr @ %title.colorNameStr);

	echo(%this.getPlayerName() @ " has a new title: " @ %title.uiName);
	export("$Server::Titles::Unlocked_" @ %bl_id @ "*", "config/server/SavedTitles/" @ %bl_id @ ".cs");
	return 1;
}

function GameConnection::buyTitle(%this, %title)
{
	%bl_id = %this.getBLID();
	if(!isObject(%title) || !Server_TitleGroup.isMember(%title))
		if(!isObject(%title = Server_TitleGroup.find(%title, 1)))
		{
			%this.chatMessage("\c6That title doesn't exist!");
			return -1;
		}

	if(%title.cost <= 0)
	{
		%this.chatMessage("\c6You can't get this title!");
		return 1;
	}

	%safeName = getSafeVariableName(%title.uiName);
	if($Server::Titles::Unlocked_[%bl_id, %safeName])
	{
		%this.chatMessage("\c6You already own this title!");
		return 1;
	}

	%cost = %title.cost;
	%current = %this.score;
	if(%current >= %cost)
	{
		%this.incScore(-%cost);
		%this.unlockTitle(%title);
	}
	else
		%this.chatMessage("\c6You do not have enough for this title!");
}

function GameConnection::setTitle(%this, %title, %silent)
{
	%bl_id = %this.getBLID();

	if(%title $= "none" || %title $= "")
	{
		if(isObject(%oldTitle = Server_TitleGroup.find($Server::Titles::Title[%this.getBLID()], 1)) && %oldTitle.variable !$= "")
			%this.TitleData[%oldTitle.variable] = 0;

		%this.chatMessage("\c6Title removed.");
		if(%this.oldClanPrefix $= "")
			%this.clanPrefix = %this.oldClanPrefix;

		if(%this.oldClanSuffix $= "")
			%this.clanSuffix = %this.oldClanSuffix;

		%this.titleName = "";
		%this.titleUIName = "";
		%this.titleNameML = "";
		%this.TitleMLTags = "";
		%this.titleFontStr = "";

		if(%this.Shop_Client)
			commandToClient(%this, 'DRShop', "setCurrentTitle", 0);

		return;
	}

	if(!isObject(%title = Server_TitleGroup.find(%title, 1)))
	{
		if(!%silent)
			%this.chatMessage("\c6This title does not exist!");

		return;
	}

	%safeName = getSafeVariableName(stripMLControlChars(%title.uiName));
	if(!$Server::Titles::Unlocked_[%bl_id, %safeName])
	{
		if(!%silent)
			%this.chatMessage("\c6You do not own this title!");

		return;
	}

	if(isObject(%oldTitle = Server_TitleGroup.find($Server::Titles::Title[%this.getBLID()])) && %oldTitle.variable !$= "")
		%this.TitleData[%oldTitle.variable] = 0;

	if(%this.oldClanPrefix $= "")
		%this.oldClanPrefix = %this.clanPrefix;

	if(%this.oldClanSuffix $= "")
		%this.oldClanSuffix = %this.clanSuffix;

	$Server::Titles::Title[%this.getBLID()] = stripMLControlChars(%title.uiName);

	%this.titleName = getSafeVariableName(stripMLControlChars(%title.uiName));
	%this.titleUIName = %title.uiName;
	%this.titleNameML = %title.colorNameStr;
	%this.TitleMLTags = %title.MLTags;
	%this.titleFontStr = %title.fontStr;

	if(%this.Shop_Client)
		commandToClient(%this, 'DRShop', "setCurrentTitle", %title);

	if(!%silent)
		%this.chatMessage("\c6Title set to " @ %title.MLTags @ %title.fontStr @ %title.colorNameStr);
}

function GameConnection::getTitle(%this)
{
	return Server_TitleGroup.find($Server::Titles::Title[%this.getBLID()]);
}

function GameConnection::sendTitles(%client, %clear)
{
	if(!%client.Shop_Client)
		return;

	%bl_id = %client.getBLID();

	if(%clear)
		commandToClient(%client, 'DRShop', "ClearTitles");

	%count = Server_TitleGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%titleObj 	= Server_TitleGroup.getObject(%i);
		%name 		= %titleObj.uiName;
		%safeName 	= getSafeVariableName(%name);
		%unlocked 	= $Server::Titles::Unlocked_[%bl_id, %safeName];
		if(%titleObj.cost > 0 || %unlocked)
		{
			commandToClient(%client, 'DRShop', "AddTitle", %name, %unlocked, %titleObj.description, %titleObj.cost, %titleObj.colorNameStr, %titleObj.font, %titleObj.fontSize, %titleObj.MLTags, %titleObj.getID());
		}
	}
}

function GameConnection::loadTitle(%client)
{
	%bl_id = %client.getBLID();
	if(isFile("config/server/SavedTitles/" @ %bl_id @ ".cs") && !$Server::Titles::Loaded[%bl_id])
	{
		$Server::Titles::Loaded[%bl_id] = 1;
		exec("config/server/SavedTitles/" @ %bl_id @ ".cs");
	}

	%count = Server_TitleGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%titleObj 	= Server_TitleGroup.getObject(%i);
		if(hasItemOnList(%titleObj.bl_idList, %bl_id) || %client.isAdmin && %titleObj.bl_idList $= "admin")
			%client.unlockTitle(%titleObj);
	}

	if(isObject(%title = Server_TitleGroup.find($Server::Titles::Title[%bl_id])))
		%client.setTitle(%title, 1);
}

function serverCmdSetTitle(%this, %a0, %a1, %a2, %a3, %a4, %a5)
{
	for(%i = 0; %i < 5; %i++)
	{
		if(%a[%i] !$= "")
		{
			if(%name $= "")
				%name = %a[%i];
			else
				%name = %name SPC %a[%i];
		}
	}

	%this.setTitle(%name);
}

function serverCmdBuyTitle(%this, %a0, %a1, %a2, %a3, %a4, %a5)
{
	for(%i = 0; %i < 5; %i++)
	{
		if(%a[%i] !$= "")
		{
			if(%name $= "")
				%name = %a[%i];
			else
				%name = %name SPC %a[%i];
		}
	}

	%this.buyTitle(%name);
}

function serverCmdTitleShop(%this)
{
	if(Server_TitleGroup.getCount() == 0)
		return;

	%bl_id = %this.getBLID();
	%titles = 0;
	%count = Server_TitleGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%title = Server_TitleGroup.getObject(%i);
		%safeName = getSafeVariableName(%title.uiName);

		if(%title.cost > 0)
		{
			if(%title.cost > 0 && !$Server::Titles::Unlocked_[%bl_id, %safeName])
			{
				if(!%init)
				{
					%init = 1;
					%this.chatMessage("\c6------- Available titles to buy -------");
				}

				%this.chatMessage("  \c3" @ %title.cost @ " pt" @ (%title.cost != 1 ? "s" : "") @ "\c6   <sPush>" @ %title.MLTags @ %title.fontStr @ %title.colorNameStr @ "<sPop>");
				%titles++;
			}
		}
		else
			%hiddenTitles++;
	}

	if(%titles == 0)
		%this.chatMessage("\c6There are no available titles for you. Get more points!");
	else
		%this.chatMessage("\c6/BuyTitle title - Get more points to see new titles!");
}

function serverCmdTitles(%this)
{
	if(Server_TitleGroup.getCount() == 0)
		return;

	%bl_id = %this.getBLID();
	%titles = 0;
	%count = Server_TitleGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%title = Server_TitleGroup.getObject(%i);
		%safeName = getSafeVariableName(%title.uiName);

		if($Server::Titles::Unlocked_[%bl_id, %safeName])
		{
			if(!%init)
			{
				%init = 1;
				%this.chatMessage("\c6--------- Titles ---------");
			}

			%this.chatMessage("  \c6- " @ %title.MLTags @ %title.fontStr @ %title.colorNameStr);
			%titles++;
		}
	}

	if(%titles == 0)
		%this.chatMessage("\c6You do not own any titles.");
	else
		%this.chatMessage("\c6/SetTitle name - Set a title, make sure you fully type it!");
}

function serverCmdAllTitles(%this)
{
	if(!%this.isSuperAdmin)
		return;

	if(Server_TitleGroup.getCount() == 0)
		return;

	%bl_id = %this.getBLID();
	%titles = 0;
	%count = Server_TitleGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%title = Server_TitleGroup.getObject(%i);
		%safeName = getSafeVariableName(%title.uiName);

		if(!%init)
		{
			%init = 1;
			%this.chatMessage("\c6--------- All Titles ---------");
		}

		%this.chatMessage("  \c6Title " @ %title.MLTags @ %title.fontStr @ %title.colorNameStr);
		%titles++;
	}

	%this.chatMessage("\c6/GiveTitle name title - Be careful, this can give anyone hidden titles!");
}

function serverCmdGiveTitle(%this, %targName, %a0, %a1, %a2, %a3, %a4, %a5)
{
	if(!%this.isSuperAdmin)
		return;

	if(Server_TitleGroup.getCount() == 0)
		return;

	if(!isObject(%targ = findClientByName(%targName)))
	{
		%this.chatMessage("\c6Invalid target! /GiveTitle name title name");
		return;
	}

	for(%i = 0; %i < 5; %i++)
	{
		if(%a[%i] !$= "")
		{
			if(%name $= "")
				%name = %a[%i];
			else
				%name = %name SPC %a[%i];
		}
	}

	if(!isObject(%title = Server_TitleGroup.find(%name)))
	{
		%this.chatMessage("\c6Invalid title!");
		return;
	}

	%bl_id = %targ.getBLID();
	%safeName = getSafeVariableName(stripMLControlChars(%title.uiName));
	if($Server::Titles::Unlocked_[%bl_id, %safeName])
	{
		%this.chatMessage("\c3" @ %targ.getPlayerName() @ " \c6already has this title.");
		return;
	}

	messageAll('', "\c3" @ %this.getPlayerName() @ " \c6has given \c3" @ %targ.getPlayerName() @ " \c6a title: " @ %title.MLTags @ %title.fontStr @ %title.colorNameStr);
	%targ.unlockTitle(%title);
}

function serverCmdWhoHasTitles(%this, %targName)
{
	if(!%this.isSuperAdmin)
		return;

	if(Server_TitleGroup.getCount() == 0)
		return;

	if(!isObject(%targ = findClientByName(%targName)))
	{
		%this.chatMessage("\c6Invalid target! /GiveTitle name title name");
		return;
	}

	%this.chatMessage("\c6" @ %targ.getPlayerName() @ " unlocked titles:");
	%bl_id = %targ.getBLID();
	%titles = 0;
	%count = Server_TitleGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%title = Server_TitleGroup.getObject(%i);
		%safeName = getSafeVariableName(stripMLControlChars(%title.uiName));

		if($Server::Titles::Unlocked_[%bl_id, %safeName])
		{
			%this.chatMessage("  \c6Title " @ %title.MLTags @ %title.fontStr @ %title.colorNameStr);
			%titles++;
		}
	}
}

if(isPackage("Server_Titles"))
	deactivatePackage("Server_Titles");

package Server_Titles
{
	function GameConnection::autoAdminCheck(%this)
	{
		%this.loadTitle();
		return Parent::autoAdminCheck(%this);
	}

	function serverCmdMessageSent(%this, %message)
	{
		%oldPre = %this.clanPrefix;
		if(%this.titleUIName !$= "")
			%this.clanPrefix = "<sPush>" @ %this.TitleMLTags @ %this.titleFontStr @ %this.titleNameML @ "<sPop> \c7" @ %this.clanPrefix;

		Parent::serverCmdMessageSent(%this, %message);
		%this.clanPrefix = %oldPre;
	}

	function serverCmdTeamMessageSent(%this, %message)
	{
		%oldPre = %this.clanPrefix;
		if(%this.titleUIName !$= "")
			%this.clanPrefix = "<sPush>" @ %this.TitleMLTags @ %this.titleFontStr @ %this.titleNameML @ "<sPop> \c7" @ %this.clanPrefix;

		Parent::serverCmdTeamMessageSent(%this, %message);
		%this.clanPrefix = %oldPre;
	}
};
activatePackage("Server_Titles");

Server_LoadTitles();
announce("\c6Titles have been reloaded. You might have to refresh your GUI using the refresh button to fix your Profile tab.");