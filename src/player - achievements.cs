function ServerAchievement::onAdd(%achievement)
{
	if(%achievement.uiName $= "")
	{
		warn("Invalid achievement uiName title" SPC %achievement.getName());
		%achievement.schedule(33,"delete");
		return;
	}

	if(%achievement.rewardTitle !$= "" && !isObject(%achievement.rewardTitle))
	{
		warn("Invalid achievement reward title" SPC %achievement.uiName);
		%achievement.schedule(33,"delete");
		return;
	}
	
	if(%achievement.isSecret || %achievement.isHidden)
	{
		%name = %achievement.uiName;
		%nameLen = strLen(%name);
		for(%i = 0; %i < %nameLen; %i++)
		{
			%char = getSubStr(%name, %i, 1);
			if(%char $= " ")
				%nameWithQ = %nameWithQ @ " ";
			else
				%nameWithQ = %nameWithQ @ "?";
		}

		%achievement.nameHidden = %nameWithQ;
	}

	Server_AchievementGroup.add(%achievement);
	%safeName 		= getSafeVariableName(%name);
	%descriptionStr = %achievement.description;
	%hidden 		= %achievement.isHidden;
	%secret 		= %achievement.isSecret;
	%reward 		= %achievement.rewardText;
	Server_AchievementGroup.objIndex[%safeName] = %achievement;

	%count = ClientGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%client = ClientGroup.getObject(%i);
		%bl_id = %client.getBLID();

		if(%client.Shop_Client)
		{
			%detail 		= %achievement.onCheckDetail(%client);
			%unlocked 		= %client.datainstance($DR::SaveSlot).AchievementComplete[%safeName];
			
			if(%hidden)
			{
				if(!%unlocked)
					continue;
			}
			
			commandToClient(%client, 'DRShop', "AddAchievement", (%secret && !%unlocked ? %achievement.nameHidden : %name),
			 %unlocked, (%hidden ? "Details hidden." : (%secret && !%unlocked ? "This is a secret achievement. Unlock to find out more." : %descriptionStr)),
			  %hidden, %secret, %reward, (%hidden ? "" : (%secret && !%unlocked ? "" : %detail)), %achievement.getID());
		}
	}
}

function ServerAchievement::onRemove(%achievement)
{
	%safeName = getSafeVariableName(%achievement.uiName);
	if(Server_AchievementGroup.objIndex[%safeName] !$= "")
		Server_AchievementGroup.objIndex[%safeName] = 0;
}

function ServerAchievementSet::find(%group, %name)
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

// Make your own achievement class function to avoid the achievement being able to never unlock - See Server_LoadAchievements()
function ServerAchievement::onCheck(%achievement, %client)
{
	if(%achievement.rewardCheck !$= "")
	{
		%rewards = %achievement.rewardCheck;

		%fieldCount = getFieldCount(%rewards);
		for(%i = 0; %i < %fieldCount; %i++)
		{
			%field 		= getField(%rewards, %i);
			%name 		= getWord(%field, 0);
			%rewardAmt 	= getWord(%field, 1);

			if(%name $= "kills")
			{
				if(%client.dataInstance($DR::SaveSlot).DR_totalKills >= %rewardAmt)
					%check++;
			}
			else if(%name $= "points")
			{
				if(%client.dataInstance($DR::SaveSlot).DR_totalPoints >= %rewardAmt)
					%check++;
			}
			else if(%name $= "rounds")
			{
				if(%client.dataInstance($DR::SaveSlot).DR_totalRounds >= %rewardAmt)
					%check++;
			}
			else if(%name $= "deaths")
			{
				if(%client.dataInstance($DR::SaveSlot).DR_totalDeaths >= %rewardAmt)
					%check++;
			}
			else if(%name $= "winsByButton")
			{
				if(%client.dataInstance($DR::SaveSlot).DR_totalWinsByButton >= %rewardAmt)
					%check++;
			}
			else if(%name $= "wins")
			{
				if(%client.dataInstance($DR::SaveSlot).DR_totalWins >= %rewardAmt)
					%check++;
			}
			else if(%name $= "playtime") // minutes
			{
				if((%client.getTotalPlayTime() / 60) >= %rewardAmt)
					%check++;
			}
			else if(%name $= "lols")
			{
				if(%client.lols >= %rewardAmt)
					%check++;
			}
			else if(%name $= "morelols")
			{
				if(%client.morelols >= %rewardAmt)
					%check++;
			}
		}

		if(%check >= %fieldCount)
			return 1;

		return 0;
	}

	return 1;
}

function ServerAchievement::onCheckDetail(%achievement, %client)
{
	if(%achievement.rewardCheck !$= "")
	{
		%rewards = %achievement.rewardCheck;

		%fieldCount = getFieldCount(%rewards);
		for(%i = 0; %i < %fieldCount; %i++)
		{
			%field 		= getField(%rewards, %i);
			%name 		= getWord(%field, 0);
			%rewardAmt 	= getWord(%field, 1);

			%tempStr = "";
			if(%name $= "kills")
			{
				%tempStr = (%rewardAmt == 1 ? "kill" : "kills") @ " " @ %client.dataInstance($DR::SaveSlot).DR_totalKills @ " " @ %rewardAmt;
			}
			else if(%name $= "points")
			{
				%tempStr = (%rewardAmt == 1 ? "point" : "points") @ " " @ %client.dataInstance($DR::SaveSlot).DR_totalPoints @ " " @ %rewardAmt;
			}
			else if(%name $= "rounds")
			{
				%tempStr = (%rewardAmt == 1 ? "round" : "rounds") @ " " @ %client.dataInstance($DR::SaveSlot).DR_totalRounds @ " " @ %rewardAmt;
			}
			else if(%name $= "deaths")
			{
				%tempStr = (%rewardAmt == 1 ? "death" : "deaths") @ " " @ %client.dataInstance($DR::SaveSlot).DR_totalDeaths @ " " @ %rewardAmt;
			}
			else if(%name $= "winsByButton")
			{
				%tempStr = (%rewardAmt == 1 ? "win" : "wins") @ " " @ %client.dataInstance($DR::SaveSlot).DR_totalWinsByButton @ " " @ %rewardAmt;
			}
			else if(%name $= "wins")
			{
				%tempStr = (%rewardAmt == 1 ? "win" : "wins") @ " " @ %client.dataInstance($DR::SaveSlot).DR_totalWins @ " " @ %rewardAmt;
			}
			else if(%name $= "playtime") // minutes
			{
				if((%client.getTotalPlayTime() / 60) >= %rewardAmt)
					%tempTime = %rewardAmt @ "+"; // I cannot do this in any other language, but on the client --> "120+"/"120" is actually 1.
				else
					%tempTime = (%client.getTotalPlayTime() / 60);

				%tempStr = (%rewardAmt == 1 ? "minute" : "minutes") @ " " @ %tempTime @ " " @ %rewardAmt;
			}
			else if(%name $= "lols")
			{
				%tempStr = (%rewardAmt == 1 ? "lol" : "lols") @ " " @ %client.lols @ " " @ %rewardAmt;
			}
			else if(%name $= "morelols")
			{
				%tempStr = (%rewardAmt == 1 ? "morelol" : "morelols") @ " " @ %client.morelols @ " " @ %rewardAmt;
			}

			if(%tempStr !$= "")
			{
				if(getWord(%tempStr, 1) $= "")
					setWord(%tempStr, 1, "0");

				if(%str $= "")
					%str = %tempStr;
				else
					%str = %str @ "\t" @ %tempStr;
			}
		}

		return %str;
	}

	return "";
}


function ServerAchievement::onUnlock(%achievement, %client)
{
	if(%achievement.rewardPoints > 0)
		%client.incScore(%achievement.rewardPoints);

	if((%title = %achievement.rewardTitle) !$= "")
		%client.unlockTitle(%title);

	return 1;
}

registerOutputEvent("GameConnection", "unlockAchievement", "string 50 50");
registerOutputEvent("GameConnection", "forceUnlockAchievement", "string 50 50\tbool");

function GameConnection::forceUnlockAchievement(%client, %name, %noBruteMsg)
{
	%achievementObj = Server_AchievementGroup.find(%name);
	if(!isObject(%achievementObj))
	{
		return -1;
	}

	%name = %achievementObj.uiName;
	%achTx = getSafeVariableName(%name);
	%bl_id = %client.getBLID();

	if(%client.dataInstance($DR::SaveSlot).AchievementComplete[%achTx])
	{
		return 1;
	}

	%client.dataInstance($DR::SaveSlot).AchievementComplete[%achTx] = 1;

	%rewardStrML = (%achievementObj.hasReward ? " \c6(Reward: \c3" @ %achievementObj.rewardTextML @ "\c6)" : "");
	%rewardStr = (%achievementObj.hasReward ? " (Reward: " @ %achievementObj.rewardText @ ")" : "");
	if(%noBruteMsg)
	{
		if(%achievementObj.isSecret == 1)
		{
			echo(%client.getPlayerName() @ " has completed a secret achievement: " @ %name @ %rewardStr);
			%client.chatMessage("\c6Secret achievement \c4" @ %name @ " \c6completed." @ %rewardStrML);
		}
		else
		{
			echo(%client.getPlayerName() @ " has completed an achievement: " @ %name @ %rewardStr);
			messageAllExcept(%client, -1, 'MsgUploadEnd', '\c3%2 \c6has completed achievement \c3%1\c6!', %name, %client.getPlayerName());
			%client.chatMessage("\c6Achievement \c4" @ %name @ " \c6completed. " @ %rewardStrML);
		}
	}
	else
	{
		if(%achievementObj.isSecret == 1)
		{
			echo(%client.getPlayerName() @ " has completed a secret achievement (brute force): " @ %name @ %rewardStr);
			%client.chatMessage("\c6Secret achievement \c4" @ %name @ " \c6completed in brute force." @ %rewardStrML);
		}
		else
		{
			echo(%client.getPlayerName() @ " has completed an achievement (brute force): " @ %name @ %rewardStr);
			messageAllExcept(%client, -1, 'MsgUploadEnd', '\c3%2 \c6has completed achievement \c3%1\c6 against his will.', %name, %client.getPlayerName());
			%client.chatMessage("\c6Achievement \c4" @ %name @ " \c6completed in brute force. " @ %rewardStrML);
		}
	}

	if(%achievementObj.hasReward)
		%achievementObj.onUnlock(%client);

	if(%client.Shop_Client)
	{
		%safeName 		= getSafeVariableName(%name);
		%descriptionStr = %achievementObj.description;
		%hidden 		= %achievementObj.isHidden;
		%secret 		= %achievementObj.isSecret;
		%reward 		= %achievementObj.rewardText;

		%detail 		= %achievementObj.onCheckDetail(%client);

		if(%secret)
		{
			%detail = "";
		}
		
		if(%hidden)
		{
			%descriptionStr = "";
			%detail = "";
		}

		commandToClient(%client, 'DRShop', "AddAchievement", %name, %achTx, 1, %descriptionStr, %hidden, %secret, %reward, %detail, %ach.getID());
	}

	return 1;
}

function GameConnection::lockAchievement(%client, %name)
{
	%achievementObj = Server_AchievementGroup.find(%name);
	if(!isObject(%achievementObj))
	{
		return -1;
	}

	%name = %achievementObj.uiName;
	%achTx = getSafeVariableName(%name);
	%bl_id = %client.getBLID();

	if(!%client.dataInstance($DR::SaveSlot).AchievementComplete[%achTx])
	{
		return 1;
	}

	%client.dataInstance($DR::SaveSlot).AchievementComplete[%achTx] = 0;

	if(%client.Shop_Client)
	{
		%safeName 		= getSafeVariableName(%name);
		%descriptionStr = %achievementObj.description;
		%hidden 		= %achievementObj.isHidden;
		%secret 		= %achievementObj.isSecret;
		%reward 		= %achievementObj.rewardText;

		%detail 		= %achievementObj.onCheckDetail(%client);

		if(%secret)
		{
			%detail = "";
		}
		
		if(%hidden)
		{
			%descriptionStr = "";
			%detail = "";
		}

		commandToClient(%client, 'DRShop', "AddAchievement", %name, %achTx, 0, %descriptionStr, %hidden, %secret, %reward, %detail, %achievementObj.getID());
	}
	return 1;
}

function GameConnection::unlockAchievement(%client, %name)
{
	%achievementObj = Server_AchievementGroup.find(%name);
	if(!isObject(%achievementObj))
	{
		return -1;
	}

	%name = %achievementObj.uiName;
	%achTx = getSafeVariableName(%name);
	%bl_id = %client.getBLID();

	if(%client.dataInstance($DR::SaveSlot).AchievementComplete[%achTx])
	{
		return 1;
	}

	if(%achievementObj.onCheck(%client) != 1)
	{
		return 0;
	}

	%client.dataInstance($DR::SaveSlot).AchievementComplete[%achTx] = 1;

	%rewardStrML = (%achievementObj.hasReward ? " \c6(Reward: \c3" @ %achievementObj.rewardTextML @ "\c6)" : "");
	%rewardStr = (%achievementObj.hasReward ? " (Reward: " @ %achievementObj.rewardText @ ")" : "");
	if(%achievementObj.isSecret == 1)
	{
		echo(%client.getPlayerName() @ " has completed a secret achievement: " @ %name @ %rewardStr);
		%client.chatMessage("\c6Secret achievement \c4" @ %name @ " \c6completed." @ %rewardStrML);
	}
	else
	{
		echo(%client.getPlayerName() @ " has completed an achievement: " @ %name @ %rewardStr);
		messageAllExcept(%client, -1, 'MsgUploadEnd', '\c3%2 \c6has completed achievement \c3%1\c6!', %name, %client.getPlayerName());
		%client.chatMessage("\c6Achievement \c4" @ %name @ " \c6completed. " @ %rewardStrML);
	}

	if(%achievementObj.hasReward)
		%achievementObj.onUnlock(%client);


	if(%client.Shop_Client)
	{
		%safeName 		= getSafeVariableName(%name);
		%descriptionStr = %achievementObj.description;
		%hidden 		= %achievementObj.isHidden;
		%secret 		= %achievementObj.isSecret;
		%reward 		= %achievementObj.rewardText;

		%detail 		= %achievementObj.onCheckDetail(%client);

		if(%secret)
		{
			%detail = "";
		}
		
		if(%hidden)
		{
			%descriptionStr = "";
			%detail = "";
		}

		commandToClient(%client, 'DRShop', "AddAchievement", %name, %achTx, 1, %descriptionStr, %hidden, %secret, %reward, %detail, %achievementObj.getID());
	}

	return 1;
}

if(isPackage("Server_Achievements"))
	deactivatePackage("Server_Achievements");


function GameConnection::sendAchievements(%client, %clear)
{
	if(!%client.Shop_Client)
		return;

	%bl_id = %client.getBLID();

	if(%clear)
		commandToClient(%client, 'DRShop', "ClearAchievements");

	%count = Server_AchievementGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%ach 			= Server_AchievementGroup.getObject(%i);
		%name 			= %ach.uiName;
		%safeName 		= getSafeVariableName(%name);
		%descriptionStr = %ach.description;
		%hidden 		= %ach.isHidden;
		%secret 		= %ach.isSecret;
		%reward 		= %ach.rewardText;

		%detail 		= %ach.onCheckDetail(%client);
		%unlocked 		= %client.datainstance($DR::SaveSlot).AchievementComplete[%safeName];

		if(%secret)
		{
			if(!%unlocked)
			{
				%name = %ach.nameHidden;
				%descriptionStr = "This is a secret achievement. Unlock to find more.";
			}

			%detail = "";
		}
		
		if(%hidden)
		{
			if(!%unlocked && !%client.isSuperAdmin)
				continue;

			%descriptionStr = "Details hidden.";
			%detail = "";
		}
		
		commandToClient(%client, 'DRShop', "AddAchievement", %name, %unlocked, %descriptionStr, %hidden, %secret, %reward, %detail, %ach.getID());
	}
}

function Server_LoadTestAchievements()
{
	new ScriptObject("DRAch_Test0")
	{
		class = "ServerAchievement";
		uiName = "Test 0";
		isSecret = 0;
		isHidden = 0;
		description = "Get 5 lols.";
		hasReward = 1;
		rewardCheck = "lols 5";
		rewardPoints = 25;
		rewardTextML = "\c325 points";
		rewardText = "25 points";
	};

	new ScriptObject("DRAch_Test1")
	{
		class = "ServerAchievement";
		uiName = "Test 1";
		isSecret = 1;
		isHidden = 0;
		description = "Get 10 lols and 10 more lols.";
		hasReward = 0;
		rewardCheck = "lols 10\tmorelols 10";
		rewardPoints = 0;
		rewardTextML = "";
		rewardText = "";
	};

	new ScriptObject("DRAch_Test2")
	{
		class = "ServerAchievement";
		uiName = "Test 2";
		isSecret = 0;
		isHidden = 1;
		description = "Get 25 lols.";
		hasReward = 1;
		rewardCheck = "lols 25";
		rewardPoints = 25;
		rewardTextML = "\c325 points";
		rewardText = "25 points";
	};

	new ScriptObject("DRAch_Test3")
	{
		class = "ServerAchievement";
		uiName = "Test 3";
		isSecret = 1;
		isHidden = 1;
		description = "Get a total of 50 more lols.";
		hasReward = 1;
		rewardCheck = "morelols 25";
		rewardPoints = 25;
		rewardTextML = "\c325 points";
		rewardText = "25 points";
	};

	new ScriptObject("DRAch_Test4")
	{
		class = "ServerAchievement";
		uiName = "Test 4";
		isSecret = 0;
		isHidden = 0;
		description = "Get a total of 50 lols and 50 more lols.";
		hasReward = 1;
		rewardCheck = "morelols 50\tlols 50";
		rewardPoints = 10;
		rewardTextML = "\c310 points and being gay";
		rewardText = "10 points and being gay";
	};
}

function GameConnection::addLols(%client, %lol)
{
	%client.lols += %lol;
	%client.unlockAchievement("Test 0");
	%client.unlockAchievement("Test 1");
	%client.unlockAchievement("Test 2");
	%client.unlockAchievement("Test 3");
	%client.unlockAchievement("Test 4");
}

function GameConnection::addMoreLols(%client, %lol)
{
	%client.morelols += %lol;
	%client.unlockAchievement("Test 0");
	%client.unlockAchievement("Test 1");
	%client.unlockAchievement("Test 2");
	%client.unlockAchievement("Test 3");
	%client.unlockAchievement("Test 4");
}

function Server_LoadAchievements()
{
	if(isObject(Server_AchievementGroup))
	{
		Server_AchievementGroup.deleteall();
		Server_AchievementGroup.delete();
	}
	
	new ScriptGroup(Server_AchievementGroup)
	{
		class = ServerAchievementSet;
	};

	// Example of achievement
	// Make sure your achievement has class::onCheck(%achievement, %client) and class::onWin(%achievement, %client)
	// Explanation of each variable on the object (TODO)

	// new ScriptObject("DRAch_Die")
	// {
	// 	class = "ServerAchievement";
	// 	uiName = "Die";
	// 	isSecret = 0;
	// 	isHidden = 0;
	// 	description = "Welcome to the death world.";
	//	hasReward = 1;
	//	rewardCheck = "deaths";
	//	rewardPoints = 35;
	//	rewardTitle = "";
	// 	rewardText = "35 points";
	// };

	// example of a live object - some stuff is just simple and no need to create a custom function (such as points, titles, value checks)
	// Add superclass back if you make your own class
	new ScriptObject("DRAch_Unstoppable")											// Name of achievement in safe name
	{
		class 			= "ServerAchievement";										// Same as superclass unless we have a complicated/custom check using class::onCheck/onUnlock/onCheckDetail
		uiName			= "Unstoppable";											// Name of the achievement, careful with non-english/custom chars since it uses game default safe name check
		isSecret		= 1;														// Hides description/name until it is unlocked, yes this can be spoiled easier, see isHidden if you really don't want players to see how they unlock it
		isHidden		= 0;														// Hides all stats (unlocked/locked) and does not appear in achievement list unless unlocked
		description		= "Get a total of 100 player kills.";						// Description of achievement
		hasReward		= 1;														// If it has an award it'll call the default onUnlock unless it has a custom one as class::onUnlock
		rewardPoints	= 0;														// How many points to award the player
		rewardCheck		= "kills 100";												// Multiple checks are accepted in tabs (\t), first one is value name, second one is value check, see above for types of value checks
		rewardTitle		= Server_TitleGroup.objIndex["Unstoppable"];													// Reward title in ID/constant so it's easier to find and award the player
		rewardTextML	= "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop>";	// ML text for chat of the reward
		rewardText		= "title " @ %title.uiName;									// No ML text for console or other case of the reward
	};																				// Needs a ; to create object (you get a compiler error anyway

	new ScriptObject("DRAch_On_the_kill")
	{
		class = "ServerAchievement";
		uiName = "On the kill";
		isSecret = 0;
		isHidden = 0;
		description = "Get a total of 25 player kills.";
		hasReward = 1;
		rewardCheck = "kills 25";
		rewardPoints = 25;
		rewardTextML = "\c325 points";
		rewardText = "25 points";
	};

	new ScriptObject("DRAch_I_win")
	{
		class = "ServerAchievement";
		uiName = "I win!";
		isSecret = 0;
		isHidden = 0;
		description = "Win a game by pressing the button at the end of the race. (Also counts with team)";
		hasReward = 1;
		rewardCheck = "winsByButton 1";
		rewardPoints = 15;
		rewardTitle = Server_TitleGroup.objIndex["Button_Presser"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c315 points";
		rewardText = "title " @ %title.uiName @ " and 10 points";
	};

	new ScriptObject("DRAch_Veteran_Driver")
	{
		class = "ServerAchievement";
		uiName = "Veteran Driver";
		isSecret = 1;
		isHidden = 0;
		description = "Win a game by pressing the button at the end of the race 100 times. (Also counts with team)";
		hasReward = 1;
		rewardCheck = "winsByButton 100";
		rewardPoints = 75;
		rewardTitle = Server_TitleGroup.objIndex["Veteran_Driver"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c375 points";
		rewardText = "title " @ %title.uiName @ " and 75 points";
	};

	new ScriptObject("DRAch_I_am_a_winner")
	{
		class = "ServerAchievement";
		uiName = "I am a winner!";
		isSecret = 0;
		isHidden = 0;
		description = "Win a game by pressing the button at the end of the race 10 times. (Also counts with team)";
		hasReward = 1;
		rewardCheck = "winsByButton 10";
		rewardPoints = 25;
		rewardTitle = Server_TitleGroup.objIndex["Uber"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c325 points";
		rewardText = "title " @ %title.uiName @ " and 25 points";
	};
	
	new ScriptObject("DRAch_Experienced_Driver")
	{
		class = "ServerAchievement";
		uiName = "Experienced Driver";
		isSecret = 0;
		isHidden = 0;
		description = "Win a game by pressing the button at the end of the race 25 times. (Also counts with team)";
		hasReward = 1;
		rewardCheck = "winsByButton 25";
		rewardPoints = 40;
		rewardTitle = Server_TitleGroup.objIndex["Demolition_Uber"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c340 points";
		rewardText = "title " @ %title.uiName @ "and 40 points";
	};

	new ScriptObject("DRAch_Marathon")
	{
		class = "ServerAchievement";
		uiName = "Deathrace Marathon";
		isSecret = 0;
		isHidden = 0;
		description = "Play DeathRace for a total time of 2 hours.";
		hasReward = 1;
		rewardCheck = "playtime 120"; // checks by minutes
		rewardPoints = 30;
		rewardTitle = "";
		rewardTextML = "\c330 points";
		rewardText = "30 points";
	};

	new ScriptObject("DRAch_DeathRace_Addiction")
	{
		class = "ServerAchievement";
		uiName = "Deathrace Addiction";
		isSecret = 0;
		isHidden = 0;
		description = "Play DeathRace for a total time of 10 hours.";
		hasReward = 1;
		rewardCheck = "playtime 600";
		rewardPoints = 0;
		rewardTitle = Server_TitleGroup.objIndex["Addicted"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop>";
		rewardText = "title " @ %title.uiName;
	};

	new ScriptObject("DRAch_Bloodthirsty")
	{
		class = "ServerAchievement";
		uiName = "Bloodthirsty";
		isSecret = 0;
		isHidden = 0;
		description = "Get a triple kill.";
		hasReward = 1;
		rewardPoints = 50;
		rewardTitle = Server_TitleGroup.objIndex["Bloodthirsty"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c350 points";
		rewardText = "title " @ %title.uiName @ " and 50 points";
	};

	// Map achievements
	new ScriptObject("DRAch_Mountain_of_Death_Expert")
	{
		class = "ServerAchievement";
		uiName = "Mountain of Death Expert";
		isSecret = 1;
		isHidden = 0;
		description = "Finish Mountain of Death by pressing the win button 10 times.";
		hasReward = 1;
		rewardPoints = 20;
		rewardTitle = Server_TitleGroup.objIndex["Mt_Luneth"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c320 points";
		rewardText = "title " @ %title.uiName @ " and 20 points";
	};

	new ScriptObject("DRAch_Beachland_Expert")
	{
		class = "ServerAchievement";
		uiName = "Beachland Expert";
		isSecret = 1;
		isHidden = 0;
		description = "Finish Beachland by pressing the win button 10 times.";
		hasReward = 1;
		rewardPoints = 20;
		rewardTitle = Server_TitleGroup.objIndex["Tropical"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c320 points";
		rewardText = "title " @ %title.uiName @ " and 20 points";
	};

	new ScriptObject("DRAch_Lethal_Lava_Jumps_Expert")
	{
		class = "ServerAchievement";
		uiName = "Lethal Lava Jumps Expert";
		isSecret = 1;
		isHidden = 0;
		description = "Finish Lethal Lava Jumps by pressing the win button 10 times.";
		hasReward = 1;
		rewardPoints = 20;
		rewardTitle = Server_TitleGroup.objIndex["Explosive"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c320 points";
		rewardText = "title " @ %title.uiName @ " and 20 points";
	};

	new ScriptObject("DRAch_Rocky_Road_Expert")
	{
		class = "ServerAchievement";
		uiName = "Rocky Road Expert";
		isSecret = 1;
		isHidden = 0;
		description = "Finish Rocky Road by pressing the win button 10 times.";
		hasReward = 1;
		rewardPoints = 20;
		rewardTitle = Server_TitleGroup.objIndex["Off_Road"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c320 points";
		rewardText = "title " @ %title.uiName @ " and 20 points";
	};

	new ScriptObject("DRAch_Rough_Rapids_Expert")
	{
		class = "ServerAchievement";
		uiName = "Rough Rapids Expert";
		isSecret = 1;
		isHidden = 0;
		description = "Finish Rough Rapids by pressing the win button 10 times.";
		hasReward = 1;
		rewardPoints = 20;
		rewardTitle = Server_TitleGroup.objIndex["Aquifer"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c320 points";
		rewardText = "title " @ %title.uiName @ " and 20 points";
	};

	new ScriptObject("DRAch_Desert_Trove_Expert")
	{
		class = "ServerAchievement";
		uiName = "Desert Trove Expert";
		isSecret = 1;
		isHidden = 0;
		description = "Finish Desert Trove by pressing the win button 10 times.";
		hasReward = 1;
		rewardPoints = 20;
		rewardTitle = Server_TitleGroup.objIndex["All_Terrain"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c320 points";
		rewardText = "title " @ %title.uiName @ " and 20 points";
	};

	new ScriptObject("DRAch_Finicky_Forest_Expert")
	{
		class = "ServerAchievement";
		uiName = "Finicky Forest Expert";
		isSecret = 1;
		isHidden = 0;
		description = "Finish Finicky Forest by pressing the win button 10 times.";
		hasReward = 1;
		rewardPoints = 20;
		rewardTitle = Server_TitleGroup.objIndex["Chop_Wood"];
		rewardTextML = "\c6title <sPush>" @ %title.fontStr @ %title.colorNameStr @ "<sPop> \c6and \c320 points";
		rewardText = "title " @ %title.uiName @ " and 20 points";
	};
}
schedule(1000,0,"Server_LoadAchievements");

function serverCmdAchievements(%client)
{
	if(!%client.hasSpawnedOnce)
		return;
	
	%bl_id = %client.getBLID();
	%client.chatMessage("\c7--------------- <sPush><font:impact:22>\c4Achievements<sPop> \c7---------------");

	%unlockedStr = "\c7[ \c2x \c7] \c5";
	%lockedStr	 = "\c7[    ] \c5";
	%count = Server_AchievementGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%ach = Server_AchievementGroup.getObject(%i);

		%name 			= %ach.uiName;
		%safeName 		= getSafeVariableName(%name);
		%descriptionStr = " \c7| \c3" @ %ach.description;
		%hidden 		= %ach.isHidden;
		%secret 		= %ach.isSecret;
		%isReward 		= %ach.hasReward;
		%reward 		= %ach.rewardTextML;

		%detail 		= %ach.onCheckDetail(%client);
		%detailStr 		= "";
		%unlocked 		= %client.datainstance($DR::SaveSlot).AchievementComplete[%safeName];

		if((%hidden || %secret) && !%unlocked)
			%name = %ach.nameHidden;

		%chatStr 		= (%unlocked ? %unlockedStr : %lockedStr) @ %name;
		if(%isReward && %unlocked)
			%rewardStr 	= " \c7| \c6Reward: " @ %reward;
		else
			%rewardStr 	= "";

		if(!%secret && !%hidden)
		{
			if(%unlocked)
			{
				%fieldCount = getFieldCount(%detail);
				if(%fieldCount == 1)
				{
					%part1 = getWord(%detail, 1);
					%part2 = getWord(%detail, 2);
					%type = getWord(%detail, 0);

					%detailStr = " \c7| \c6" @ %part1 @ "/" @ %part2 @ " " @ %type;
				}
				else if(%fieldCount > 0)
				{
					for(%j = 0; %j < %fieldCount; %j++)
					{
						%field = getField(%detail, %j);
						
						%part1 = getWord(%detail, 1);
						%part2 = getWord(%field, 2);
						%type = getWord(%field, 0);

						if(%detailStr $= "")
							%detailStr = " \c7| \c6" @ %part1 @ "/" @ %part2 @ " " @ %type;
						else
							%detailStr = %detailStr @ "\c7, \c6" @ %part1 @ "/" @ %part2 @ " " @ %type;
					}
				}
			}
			else
			{
				%fieldCount = getFieldCount(%detail);
				if(%fieldCount == 1)
				{
					%part1 = getWord(%detail, 1);
					%part2 = getWord(%detail, 2);
					%type = getWord(%detail, 0);

					%detailStr = " \c7| " @ Chat_GetColorLevel(%part1, %part2) @ %part1 @ "\c6/" @ %part2 @ " " @ %type;
				}
				else if(%fieldCount > 0)
				{
					for(%j = 0; %j < %fieldCount; %j++)
					{
						%field = getField(%detail, %j);
						
						%part1 = getWord(%field, 1);
						%part2 = getWord(%field, 2);
						%type = getWord(%field, 0);

						if(%detailStr $= "")
							%detailStr = " \c7| " @ Chat_GetColorLevel(%part1, %part2) @ %part1 @ "\c6/" @ %part2 @ " " @ %type;
						else
							%detailStr = %detailStr @ "\c7, " @ Chat_GetColorLevel(%part1, %part2) @ %part1 @ "\c6/" @ %part2 @ " " @ %type;
					}
				}
			}
		}

		if(%secret)
		{
			if(!%unlocked)
			{
				%descriptionStr = "\c7| \c3This is a secret achievement. Unlock to find out.";
			}

			%detailStr = "";
		}
		
		if(%hidden)
		{
			if(!%unlocked && !%client.isSuperAdmin)
				continue;

			%descriptionStr = "\c7| \c3Details hidden.";
			%detailStr = "";
		}

		%chatStr = %chatStr @ %descriptionStr @ %detailStr @ %rewardStr;
		%client.chatMessage(%chatStr);
	}

	%client.chatMessage("\c6You may have to page up (PGUP) to see all achievements.");
}

function Chat_GetColorLevel(%min, %max)
{
	if(%max < 1) %max = 1;
	if(%min < 0) %min = 0;
	if(%min >= %max)
		%min = %max;

	%main = mFloor(%min / %max * 100);
	%color = RGBToHex(greenToRed(%main / 100));
	return "<color:" @ %color @ ">" @ %word;
}

announce("\c6Achievements have been reloaded. You might have to refresh your GUI using the refresh button to fix your Profile tab.");