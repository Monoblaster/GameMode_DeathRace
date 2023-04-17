// DeathRace file
if(!isObject(DR_LeaderboardList))
	new GuiTextListCtrl(DR_LeaderboardList);

function Leaderboard_CalculateScore(%client)
{
	%score = 0;
	%score += %data.DR_totalPoints * 0.7;
	%score += %data.DR_giveDamage * 0.3;
	%score += %data.DR_PlayTime/60;
	%score -= %data.DR_totalDeaths * 0.2;

	return %score;
}

function Leaderboard_FieldString(%client)
{
	%data = %client.dataInstance($DR::SaveSlot);
	return
		(%data.DR_totalPoints | 0) TAB 
		(%data.DR_giveDamage | 0) TAB 
		(%data.DR_totalKills | 0) TAB 
		(%data.DR_totalDeaths | 0) TAB 
		(%data.DR_totalWins | 0) TAB 
		(%data.DR_totalRounds | 0) TAB 
		(%data.DR_totalItemsBought | 0) TAB 
		(%data.DR_PlayTime | 0) TAB 
		Leaderboard_CalculateScore(%client);
}

function DR_Leaderboard_Scan()
{
	announce("Scanning information for leaderboard. This will take some time.");
	echo("Scanning information for leaderboard. This will take some time.");
	schedule(50, 0, DR_Leaderboard_Scan2);
}

function DR_Leaderboard_Scan2()
{
	%m = $defaultMinigame;
	DR_LeaderboardList.clear();

	%dummyParent = new ScriptObject();

	%s = %m.dataInstance($DR::SaveSlot).ClientDataString;
	%count = getWordCount(%s);
	// List for tabs (see functions - player.cs): score takeDamage giveDamage totalKills totalDeaths totalWins totalWinsByButton totalRounds totalPoints totalItemsBought FirstWin PlayTime (map data x 7)
	for(%i = 0; %i < %count; %i ++)
	{
		DataInstance_ListLoad($DataInstance::FilePath @ "/" @ getWord(%s,%i) @ ".cs",%dummyParent);
		%data = %dummyParent.dataInstance($DR::SaveSlot);

		%name = %data.playername;
		%blid = %data.blid;

		if(%name $= "" || %blid $= "")
		{
			continue;
		}

   		DR_LeaderboardList.addRow(%blid, %name TAB Leaderboard_FieldString(%client), DR_LeaderboardList.rowCount());
	}

	DR_Leaderboard_Scan3(%outdated);
}

function DR_Leaderboard_Scan3(%outdated)
{
	for(%i = 0; %i < clientGroup.getCount(); %i++)
	{
		%client = clientGroup.getObject(%i);
		%client.UpdateToLeaderboard(1);
	}

	DR_LeaderboardList.sortNumerical(9, 0);
	announce("Leaderboard scan complete. Added clients: " @ DR_LeaderboardList.rowCount());
	echo("Leaderboard scan complete. Added clients: " @ DR_LeaderboardList.rowCount());
}

function GameConnection::UpdateToLeaderboard(%client, %ignoreUpdate)
{
	%blid = %client.getBLID();

	if(DR_LeaderboardList.getRowNumByID(%blid) >= 0)
	{
   		DR_LeaderboardList.setRowByID(%blid, %client.getPlayerName() TAB  Leaderboard_FieldString(%client));
	}
   	else
   	{
   		DR_LeaderboardList.addRow(%blid, %client.getPlayerName() TAB  Leaderboard_FieldString(%client), DR_LeaderboardList.rowCount());
   	}

   	if(!%ignoreUpdate)
   		DR_LeaderboardList.sortNumerical(9, 0);
}

DR_Leaderboard_Scan();

function GameConnection::getLeaderboardRank(%client)
{
	// so far only check total points
	%ranks = "Champion\tExpert\tProfessional\tApprentice\tNovice\tBeginner";
	%place = DR_LeaderboardList.getRowNumByID(%client.getBLID())+1;

	if(%place <= 5)
		return getField(%ranks, 0);
	else if(%place < 20)
		return getField(%ranks, 1);
	else if(%place < 35)
		return getField(%ranks, 2);
	else if(%place < 55)
		return getField(%ranks, 3);
	else if(%place < 80)
		return getField(%ranks, 4);

	return getField(%ranks, 5);
}

function GameConnection::getLeaderboardNumber(%client)
{
	%place = DR_LeaderboardList.getRowNumByID(%client.getBLID())+1;
	return %place;
}

package leaderboard
{
	function GameConnection::OnClientEnterGame(%c)
	{
		%m = $defaultMinigame;
		%id = %c.DataIdentifier();
		if(!%m.dataInstance($DR::SaveSlot).ClientData[%id])
		{
			%m.dataInstance($DR::SaveSlot).ClientData[%id] = true;
			%m.dataInstance($DR::SaveSlot).ClientDataString = trim(%m.dataInstance($DR::SaveSlot).ClientDataString SPC %id);
		}
		
		return parent::OnClientEnterGame(%c);
	}
};
activatePackage("leaderboard");