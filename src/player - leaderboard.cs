// DeathRace file
if(!isObject(DR_LeaderboardList))
	new GuiTextListCtrl(DR_LeaderboardList);

function DR_Leaderboard_Scan()
{
	announce("Scanning information for leaderboard. This will take some time.");
	echo("Scanning information for leaderboard. This will take some time.");
	schedule(50, 0, DR_Leaderboard_Scan2);
}

function DR_Leaderboard_Scan2()
{
	DR_LeaderboardList.clear();
	%path = $DeathRace::Profiles @ "*.DeathRaceProfile";

	// List for tabs (see functions - player.cs): score takeDamage giveDamage totalKills totalDeaths totalWins totalWinsByButton totalRounds totalPoints totalItemsBought FirstWin PlayTime (map data x 7)
	for(%file = findFirstFile(%path); %file !$= ""; %file = findNextFile(%path))
	{
		%bl_id = fileBase(%file);

		%count++;

		%curDates = strReplace(firstWord(getDateTime()), "/", " ");
		%curMonth = getWord(%curDates, 0);
		%curYear  = getWord(%curDates, 2);

		%dates = strReplace(firstWord(getFileModifiedTime(%file)), "/", " ");
		%month = getWord(%dates, 0);
		%day   = getWord(%dates, 1);
		%year  = getWord(%dates, 2);

		// if(%count % 50)
		// {
		// 	echo("ModTime: " @ %dates);
		// 	echo(" Diffs: (cur year): " @ (%curYear - %year) @ ", (cur month): " @ (%curMonth - %month));
		// }

		if(%curYear - %year > 1 || (%month == 3 && %day == 18 && %year == 20))
		{
			%outdated++;
			continue;
		}
		else if(%curMonth - %month > 6)
		{
			%outdated++;
			continue;
		}

   		%io = new FileObject();
		%io.openForRead(%file);

		%line = %io.readLine(); // score/name
		%score = getField(%line, 0);
		%name = getField(%line, 1);

		if(trim(%name) $= "" || %name $= "// Player's score")
			%name = getField($ConnectedConfig::BL_IDName[%bl_id], getFieldCount($ConnectedConfig::BL_IDName[%bl_id])-1);

		while(!%io.isEOF())
		{
			%line = %io.readLine();
			%var = getField(%line, 0);
			%val = getField(%line, 1);

			%fieldToAdd[%var] = %val;
		}
		%io.close();
		%io.delete();
		
   		%fieldString = (%fieldToAdd["totalPoints"] | 0) TAB (%fieldToAdd["giveDamage"] | 0) TAB (%fieldToAdd["totalKills"] | 0) TAB (%fieldToAdd["totalDeaths"] | 0) TAB (%fieldToAdd["totalWins"] | 0) TAB (%fieldToAdd["totalRounds"] | 0) TAB (%fieldToAdd["totalItemsBought"] | 0) TAB (%fieldToAdd["PlayTime"] | 0);
   		%scoreLead   = %fieldToAdd["totalPoints"] * 0.7 + %fieldToAdd["giveDamage"] * 0.3 + %fieldToAdd["totalKills"] * 0.2 - %fieldToAdd["totalDeaths"] * 0.5 + (%fieldToAdd["totalRounds"] - %fieldToAdd["totalWins"]) * 0.55 + %fieldToAdd["totalItemsBought"] * 1 + %fieldToAdd["PlayTime"] /60;
   		DR_LeaderboardList.addRow(%bl_id, %name TAB %fieldString TAB %scoreLead, DR_LeaderboardList.rowCount());
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
	announce("Leaderboard scan complete. Added clients: " @ DR_LeaderboardList.rowCount() @ (%outdated > 1 ? " (outdated clients: " @ %outdated @ ")" : ""));
	echo("Leaderboard scan complete. Added clients: " @ DR_LeaderboardList.rowCount() @ (%outdated > 1 ? " (outdated clients: " @ %outdated @ ")" : ""));
}

function GameConnection::UpdateToLeaderboard(%client, %ignoreUpdate)
{
	%bl_id = %client.getBLID();
	%scoreLead   = %client.dataInstance($DR::SaveSlot).DR_totalPoints * 0.7 + %client.dataInstance($DR::SaveSlot).DR_giveDamage * 0.3 + %client.dataInstance($DR::SaveSlot).DR_totalKills * 0.2 - %client.dataInstance($DR::SaveSlot).DR_totalDeaths * 0.5 + (%client.dataInstance($DR::SaveSlot).DR_totalRounds - %client.dataInstance($DR::SaveSlot).DR_totalWins) * 0.55 + %client.dataInstance($DR::SaveSlot).DR_totalItemsBought * 1 + %client.dataInstance($DR::SaveSlot).DR_PlayTime/60;
	%fieldString = (%client.dataInstance($DR::SaveSlot).DR_totalPoints | 0) TAB 
		(%client.dataInstance($DR::SaveSlot).DR_giveDamage | 0) TAB 
		(%client.dataInstance($DR::SaveSlot).DR_totalKills | 0) TAB 
		(%client.dataInstance($DR::SaveSlot).DR_totalDeaths | 0) TAB 
		(%client.dataInstance($DR::SaveSlot).DR_totalWins | 0) TAB 
		(%client.dataInstance($DR::SaveSlot).DR_totalRounds | 0) TAB 
		(%client.dataInstance($DR::SaveSlot).DR_totalItemsBought | 0) TAB 
		(%client.dataInstance($DR::SaveSlot).DR_PlayTime | 0) TAB 
		%scoreLead;

	if(DR_LeaderboardList.getRowNumByID(%bl_id) >= 0)
	{
   		DR_LeaderboardList.setRowByID(%bl_id, %client.getPlayerName() TAB %fieldString);
	}
   	else
   	{
   		DR_LeaderboardList.addRow(%bl_id, %client.getPlayerName() TAB %fieldString, DR_LeaderboardList.rowCount());
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