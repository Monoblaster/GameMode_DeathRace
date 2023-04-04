//Special type functions to know:
// ::onRoundStart(%mode, %mini)          - Called when the minigame starts a new round
// ::onRoundEnd(%mode, %mini)            - Called when the minigame end a round
// ::onInit(%mode, %mini)                - Called when the minigame starts a new game type - good for things like setting up teams
// ::onReset(%mode, %mini)               - Called when the minigame pre-resets the round - not called during ::onInit
// ::onSpawn(%mode, %mini, %client)      - Called when a client spawns

function serverCmdSetSpecial(%client, %a1, %a2, %a3, %a4, %a5, %a6, %a7)
{
	if(!%client.isAdmin)
		return;

	if(!isObject(%mini = %client.minigame))
	{
		%client.chatMessage("Need to be in the minigame :)");
		return;
	}

	if(isObject(%mini.special))
	{
		%client.chatMessage("Sorry, there is already a special round going.");
		return;
	}

	for(%i = 1; %i <= 7; %i++)
	{
		if(%a[%i] !$= "")
		{
			if(%name $= "")
				%name = %a[%i];
			else
				%name = %name SPC %a[%i];
		}
	}

	if(isObject(%specials = %mini.specials))
	{
		if(isObject(%special = %specials.find(%name)))
		{
			%mini.messageAll('', '\c3%1 \c6has forced a special round. (\c3%2\c6)', %client.getPlayerName(), %name);
			%mini.setSpecial(%name);
		}
	}
}

function serverCmdSetNextSpecial(%client, %a1, %a2, %a3, %a4, %a5, %a6, %a7)
{
	if(!%client.isAdmin)
		return;

	if(!isObject(%mini = %client.minigame))
	{
		%client.chatMessage("Need to be in the minigame :)");
		return;
	}

	for(%i = 1; %i <= 7; %i++)
	{
		if(%a[%i] !$= "")
		{
			if(%name $= "")
				%name = %a[%i];
			else
				%name = %name SPC %a[%i];
		}
	}

	%mini.messageAll('', '\c3%1 \c6has forced a special for next round. (\c3%2\c6)', %client.getPlayerName(), %name);
	%mini.specialOverride = %name;
}

function serverCmdListSpecials(%client)
{
	if(!%client.isAdmin)
		return;

	if(!isObject(%mini = %client.minigame))
		return;

	if(!isObject(%specials = %mini.specials))
		return;

	%client.chatMessage("\c6List of known specials: (you can put 1 for random special)");
	for(%i = 0; %i < %specials.getCount(); %i++)
	{
		%client.chatMessage(" \c6+ \c3" @ %specials.getObject(%i).uiName);
	}
}

function MinigameSO::setSpecial(%mini, %specialName)
{
	if(!isObject(%mini.specials) || %mini.specials.getCount() == 0)
		return;

	if(%specialName $= "")
		return; 

	if(!isObject(%special = %mini.specials.find(%specialName)))
	{
		warn("WARNING: [" @ %mini.displayName @ "] - Cannot find special " @ %specialName @ "!");
		backtrace();
		return;
	}
	else
	{
		echo("[" @ %mini.displayName @ "] Special set to " @ %special.uiName);
	}

	if(!isObject(%special))
	{
		%mini.special = 0;
		%mini.specialName = "";
		return;
	}
	
	%mini.special = %special;
	%mini.specialName = %special.uiName;

	%special.onInit(%mini);
	if(%special.instructionDescription !$= "")
		%mini.schedule(100, messageAll, '', '\c6Special round rules: %1', %special.instructionDescription);
}

//////////////////////////////////////////////////////////////////////////////////////////

// See games.cs and change your class name if needed
function MiniSpecial::onAdd(%special)
{
	if(!isObject(%special.parent))
	{
		echo("Game is invalid - you must put it in a group: [" @ %special @ "] " @ %special.uiName);
		backtrace();
		%special.schedule(0, "delete");
		return 0;
	}

	if(%special.uiName $= "")
	{
		%special.schedule(0, "delete");
		return 0;
	}

	if(isObject(%found = %special.parent.find(%special.uiName, %special)) && %found.ver >= %special.ver)
	{
		echo("Special already exists: " @ %special.uiName);
		%special.schedule(0, "delete");
		return 0;
	}

	echo("Added special: " @ %special.uiName);
	return 1;
}

function MiniSpecial::onInit(%special, %mini)
{
	//Usually this is handled but just to be sure
	if(!isObject(%mini))
		return 0;

	echo("[" @ %mini.displayName @ "] [" @ %special.uiName @ "] Special initiate success");

	return 1;
}

function MiniSpecial::onRoundStart(%special, %mini)
{
	%scale = 1;
	if(%special.timescale > 0)
		%scale = %special.timescale;

	setTimescale(%scale);
}

function MiniSpecial::onSpawn(%special, %mini, %client)
{
	%speed = 1;
	if(%special.playerSpeed > 0)
		%speed = %special.playerSpeed;

	if(isObject(%player = %client.player))
	{
		%player.setSpeedFactor(%speed);
	}
}