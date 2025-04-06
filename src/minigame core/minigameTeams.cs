//Make sure everyone is on a team

function GameConnection::joinTeam(%client, %team)
{
	if(isObject(%mini = %client.minigame))
		%mini.joinTeam(%client, %team);
}

function CustomMinigameSO::joinTeam(%mini, %client, %teamStr)
{
	if(!isObject(%client) || %teamStr $= "")
		return;

	if(!isObject(%teams = %mini.Teams))
		return;

	%teamCount = %teams.getCount();
	for(%i = 0; %i < %teamCount; %i++)
	{
		%team = %teams.getObject(%i);
		if(%team.name $= %teamStr)
			%found = %team;
	}

	if(isObject(%found))
	{
		%mini.messageAll('', '\c3%1 \c6has joined %2%3\c6.', %client.getPlayerName(), %found.colorHexStr, %found.name);
		%found.addClient(%client);
	}
	else
		%client.chatMessage("Invalid team '" @ %teamStr @ "'");
}

function CustomMinigameSO::sortTeams(%mini)
{
	if(!isObject(%teams = %mini.Teams))
		return 0;

	if((%teamCount = %teams.getCount()) == 0)
		return;

	%count = %mini.numMembers;
	for(%i = 0; %i < %count; %i++)
	{
		%members = %members SPC %mini.member[%i];
	}
	%members = lTrim(%members);

	for(%i = 0; %i < %count; %i++)
	{
		%r = getRandom(0,%count-1);
		%temp = getWord(%members,%r);
		%members = setWord(setWord(%members,%r,getWord(%members,%i)),%i,%temp);
	}

	if(%teamCount == 1)
	{
		%team = %teams.getObject(0);
		for(%i = 0; %i < %mini.count; %i++)
		{
			%team.addClient(%mini.member[%i], 1);
		}
	}
	else
	{
		for(%i = 0; %i < %teamCount; %i++)
		{
			if(%teams.getObject(%i).avoidAutoJoin)
				continue;
			
			if(%list $= "")
				%list = %teams.getObject(%i);
			else
				%list = %list SPC %teams.getObject(%i);
		}
		%nList = %list;

		for(%i = 0; %i < %count; %i++)
		{
			if(%nList $= "")
				%nList = %list;

			%r = getRandom(0, getWordCount(%nList)-1);
			%team = getWord(%nList, %r);

			%nList = removeWord(%nList, %r);

			%team.addClient(getWord(%members,%i), 1);
		}
	}

	return 1;
}

function CustomMinigameSO::createTeamListFromString(%mini, %string)
{
	if(isObject(%mini.Teams))
	{
		while(%mini.Teams.getCount() > 0)
			%mini.Teams.getObject(0).delete();
	}
	else
	{
		%mini.Teams = new ScriptGroup(%mini @ "_Teams")
		{
			class = "MinigameTeamsSO";
		};
	}

	%teams = 0;
	%lines = getLineCount(%string);
	for(%i = 0; %i < %lines; %i++)
	{
		%line = getLine(%string, %i);
		%name = getField(%line, 0);
		%color = getColorF(getField(%line, 1));
		%avoidAutoJoin = getField(%line, 2) | 0;

		%mini.Teams.add(new ScriptObject()
		{
			class = "CustomMinigameTeamSO";
			uiName = %name;
			name = %name;
			color = %color;
			colorHex = rgbToHex(%color);
			colorHexStr = "<color:" @ rgbToHex(%color) @ ">";
			colorID = findClosestColor(%color);
			teamID = %teams;
			avoidAutoJoin = %avoidAutoJoin;

			numMembers = 0;
		});
		%teams++;
	}

	echo("[" @ %mini.displayName @ "] Created teams: " @ %teams);
}

/////////////////////////////////////////////////////////

function CustomMinigameSO::CleanTeams(%mini)
{
	if(isObject(%teams = %mini.teams) && %teams.getCount() > 0)
	{
		for(%i = 0; %i < %teams.getCount(); %i++)
		{
			%teams.getObject(%i).CleanUp();
		}
	}
}

function CustomMinigameTeamSO::CleanUp(%team)
{
	%clients = 0;
	for(%i = 0; %i < %team.numMembers; %i++)
	{
		if(isObject(%client = %team.member[%i]) && isObject(%client.minigame))
		{
			%client[%clients] = %client;
			%clients++;
		}

		%team.member[%i] = 0;
	}

	if(%clients > 0)
	{
		for(%i = 0; %i < %clients; %i++)
		{
			%team.member[%i] = %client[%i];
		}
	}

	%team.numMembers = %clients;
	return %team.numMembers;
}

function CustomMinigameTeamSO::getLiving(%team)
{
	%liveMembers = 0;
	for(%i = 0; %i < %team.numMembers; %i++)
	{
		if(isObject(%client = %team.member[%i]) && isObject(%player = %client.player) && %player.getState() !$= "dead")
			%liveMembers++;
	}

	return %liveMembers;
}

function CustomMinigameTeamSO::addClient(%team, %client, %noRespawn, %noInit)
{
	if(!isObject(%client) || %client.getClassName() !$= "GameConnection")
		return 0;

	if(isObject(%clTeam = %client.team))
	{
		if(%clTeam == %team)
			return %team;

		for(%i = 0; %i < %clTeam.numMembers; %i++)
		{
			if(%clTeam.member[%i] == %client)
			{
				for(%j = %i + 1; %j < %clTeam.numMembers; %j++)
				{
					%clTeam.member[%j - 1] = %clTeam.member[%j];
				}

				%clTeam.member[%clTeam.numMembers - 1] = "";
				%clTeam.numMembers--;

				break;
			}
		}
	}

	%team.member[%team.numMembers] = %client;
	%team.numMembers++;

	%client.team = %team;
	%client.teamID = %team.teamID;
	if(isObject(%player = %client.player))
	{
		if(!%noRespawn)
			%player.schedule(0, "instantRespawn");
		else if(!%noInit)
			%client.doTeamInit();
	}

	if(isObject(%mini = %client.minigame) && !%mini.isPreRound)
		%mini.checkLastManStanding();

	return %team;
}

function CustomMinigameTeamSO::removeClient(%team, %client, %noRespawn, %noInit)
{
	if(!isObject(%client) || %client.getClassName() !$= "GameConnection")
	{
		return 0;
	}

	for(%i = 0; %i < %team.numMembers; %i++)
	{
		if(%team.member[%i] == %client)
		{
			for(%j = %i; (%j + 1) < %team.numMembers; %j++)
			{
				%team.member[%j] = %team.member[%j + 1];
			}
			%team.numMembers--;

			break;
		}
	}

	%client.team = 0;
	%client.teamID = -1;
	if(isObject(%player = %client.player))
	{
		if(!%noRespawn)
			%player.schedule(0, "instantRespawn");
		else if(!%noInit)
			%client.doTeamInit();
	}

	if(isObject(%mini = %client.minigame) && !%mini.isPreRound)
		%mini.checkLastManStanding();

	return 1;
}

function GameConnection::setNewTeam(%client)
{
	%minigame = %client.minigame;
	if(isObject(%teams = %minigame.Teams))
	{
		%num = 99;
		for(%i = 0; %i < %teams.getCount(); %i++)
		{
			%teamObj = %teams.getObject(%i);
			if(%teamObj.numMembers < %num && !%teamObj.avoidAutoJoin)
			{
				%teamSel = %teamObj;
				%num = %teamObj.numMembers;
			}
		}

		if(isObject(%teamSel))
		{
			%team = %teamSel;
			%team.addClient(%client, 1, 1);
		}
		else
		{
			%team = %teams.getObject(getRandom(0, %teams.getCount()-1)).addClient(%client, 1, 1);
		}
	}
}

function GameConnection::doTeamInit(%client)
{
	if(!isObject(%player = %client.player))
		return 0;

	%client.deathObj = 0;
	if(isObject(%minigame = %client.minigame))
	{
		if(isObject(%team = %client.team))
		{
			%client.nameColor = %team.colorHex;
			%player.setShapeNameColor(%team.color);
		}
		else
		{
			%client.nameColor = "919191";
			%player.setShapeNameColor("0.6 0.6 0.6 1");

			if(isObject(%minigame.Teams))
			{
				if(!isObject(%client.team))
					%client.setNewTeam();
				
				if(isObject(%team = %client.team))
				{
					%client.nameColor = %team.colorHex;
					%player.setShapeNameColor(%team.color);
				}
			}
		}
	}

	return 1;
}