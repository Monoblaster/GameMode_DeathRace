if($CACHE::MapChanger::WonRounds $= "") $CACHE::MapChanger::WonRounds = 0;
if($Pref::Server::MapChanger::MaxWinsBeforeRoundChange $= "") $Pref::Server::MapChanger::MaxWinsBeforeRoundChange = 10;

package MapChanger_VotePackage {
	function GameConnection::onClientLeaveGame(%this) {
		if($Server::MapChanger::RTV_Vote_Active) {
			if(%this.rtvVote) {
				%this.rtvVote = 0;
				$Server::MapChanger::RTVCount--;
				messageAll('MsgAdminForce', "\c0RTV\c6: " @ %this.getPlayerName() @ "'" @ (getSubStr(%n=%this.getPlayerName(), strLen(%n) - 1, 1) $= "s" ? "" : "s") SPC " vote no longer counts! (" @  MapChanger_getRTVPercent() @ "%)");
			}
		}
		
		Parent::onClientLeaveGame(%this);
	}
	
	function Slayer_MiniGameSO::endRound(%this, %winningTeamSO, %resetTime, %bypass) {
		if(%bypass) return Parent::endRound(%this, %winningTeamSO, %resetTime);
		
		if($Server::MapChanger::NewVoteSys_Changing) return;
		
		$CACHE::MapChanger::WonRounds++;
		
		if($CACHE::MapChanger::WonRounds >= $Pref::Server::MapChanger::MaxWinsBeforeRoundChange || $CACHE::MapChanger::DoRTVChange) {
			%msg = "Enough people decided to /rtv, you may now vote to change the map.";
			if(!$CACHE::MapChanger::DoRTVChange) %msg = "End of round 10! Time to vote for a new map.";
			messageAll('', "<"@"color:ffffff"@"><"@"font:palatino linotype:26"@">" @ %msg);
			
			$CACHE::MapChanger::DoRTVChange = 0;
			$CACHE::MapChanger::WonRounds = 0;
			$Server::MapChanger::NewVoteSys_Changing = 1;
			MapChanger_StartNewMapVote();
			
			return;
		}
		
		%roundsLeft = $Pref::Server::MapChanger::MaxWinsBeforeRoundChange - $CACHE::MapChanger::WonRounds;
		
		%result = Parent::endRound(%this, %winningTeamSO, %resetTime);
		
		return %result;
	}
};
activatePackage(MapChanger_VotePackage);

function MapChanger_ResetVotes() {
	MapChanger_Debug('',"MapChanger_ResetVotes() \c3Votes and vote count has been reset.");
	$Server::MapChanger::VoteInit ++;
	deleteVariables("$Server::MapChanger::VoteCount*");
	$Server::MapChanger::VoteCount = 0;
}

function GameConnection::voteMap(%this, %map) {
	%map = MapChanger_findMap(%map);
	if(%map $= "ERROR_NONEXISTANTMAP" || %map $= "ERROR_BLANKMAP")
		return 0;


	%mapStr = getSafeVariableName(stripMLControlChars(%map));

	%BL_ID = %this.getBLID();

	if($Server::MapChanger::Votes[%BL_ID] $= "")
		$Server::MapChanger::VoteCount++;
	
	if($Server::MapChanger::Votes[%BL_ID] !$= %map)
	{
		%voteMapStr = getSafeVariableName(stripMLControlChars($Server::MapChanger::Votes[%BL_ID]));

		if($Server::MapChanger::Votes[%BL_ID] !$= "" && $Server::MapChanger::VoteCount[%voteMapStr] > 0)
			$Server::MapChanger::VoteCount[%voteMapStr]--;

		$Server::MapChanger::Votes[%BL_ID] = %map;
		$Server::MapChanger::VoteCount[%mapStr]++;

		%this.chatMessage("\c6You have voted for \c3" @ %map @ "\c6.");
		%votes = MapChanger_GetVoteCount(%map);
		messageAll('', "\c3" @ %this.getPlayerName() @ " \c6has voted for\c3 " @ %map @ " \c6(\c3" @ %votes SPC (%votes == 1 ? "vote" : "votes") @ "\c6) - \c3/voteMap " @ %map);
	}
	else
		%this.chatMessage("\c6You already voted for that map.");

	return 1;
}

function MapChanger_GetVoteCount(%map)
{
	%mapStr = getSafeVariableName(stripMLControlChars(%map));

	if(!strLen(%mapStr))
		return $Server::MapChanger::VoteCount;
	return mFloor($Server::MapChanger::VoteCount[%mapStr]);
}

function MapChanger_Tick(%val, %showTime) {
}

// New voting system

function MapChanger_UpdateAllClientVotes() {
	if(!isObject(%mini = Slayer_MinigameHandlerSO.getObject(0))) return;
	
	for(%i=0;%i<$CACHE::MapChanger::NewVoteSys::MAP_COUNT;%i++) {
		%mapStr         = $CACHE::MapChanger::NewVoteSys::MAP_[%i];
		%index          = $CACHE::MapChanger::NewVoteSys::INDEX_[%mapStr];
		%file           = $CACHE::MapChanger::NewVoteSys::MAPFILE_[%mapStr];
		%mName          = $CACHE::MapChanger::NewVoteSys::MAPNAME_[%mapStr];
		%votes          = $CACHE::MapChanger::NewVoteSys::VOTES_[%mapStr];
		%playerPrint    = $CACHE::MapChanger::NewVoteSys::PLRSTR_[%mapStr];
		%difficultPrint = $CACHE::MapChanger::NewVoteSys::DEFFICULTY_[%mapStr];
		%mapSizePrint   = $CACHE::MapChanger::NewVoteSys::MAPSIZE_[%mapStr];
		
		%mapListing = trim(%mapListing TAB "MCV(%C%," @ %index @ ");" SPC fileBase(%file) SPC "\c7| \c3" @ %votes SPC (%votes == 1 ? "vote" : "votes"));
	}
	
	for(%i=0;%i<%mini.numMembers;%i++) {
		%client = %mini.member[%i];
		
		%client.CPM_Data = strReplace(%mapListing, "%C%", %client);
		%client.updateCenterPrintMenu();
	}
}

function MapChanger_updateTimeLeft() {
	if(!isObject(%mini = Slayer_MinigameHandlerSO.getObject(0))) return;
	if(!isEventPending($CACHE::MapChanger::NewVoteSys::EndSchedule)) return;

        %timeMS = getTimeRemaining($CACHE::MapChanger::NewVoteSys::EndSchedule);
        %timeH  = mFloor(%timeMS/(3600000|0));
        %timeMS = %timeMS % (3600000|0);
        %timeM  = mFloor(%timeMS/60000);
        %timeMS = %timeMS % 60000;
        %timeS  = mFloor(%timeMS/1000);
		%time   = getSubStr("00" @ %timeH, strLen("00" @ %timeH) - 2, 2);
	%time   = %time @ ":" @ getSubStr("00" @ %timeM, strLen("00" @ %timeM) - 2, 2);
	%time   = %time @ ":" @ getSubStr("00" @ %timeS, strLen("00" @ %timeS) - 2, 2);
	
	for(%i=0;%i<%mini.numMembers;%i++) {
		%client = %mini.member[%i];
		
		bottomPrint(%client, "<" @ "just:center" @ ">\c6Time left\r\n\c6" @ %time, 2);
	}
	
	cancel($CACHE::MapChanger::NewVoteSys::timeLeftUpdateSchedule);
	$CACHE::MapChanger::NewVoteSys::timeLeftUpdateSchedule = schedule(950, 0, MapChanger_updateTimeLeft);
}

function MCV(%c, %t) {
	if(%c.alreadyVoted !$= "") {
		if(%t == %c.alreadyVoted) return;
		
		%c.chatMessage("\c6You voted for " @ $CACHE::MapChanger::NewVoteSys::MAPNAME_[$CACHE::MapChanger::NewVoteSys::MAP_[%t]] @ " instead of " @ $CACHE::MapChanger::NewVoteSys::MAPNAME_[$CACHE::MapChanger::NewVoteSys::MAP_[%c.alreadyVoted]] @ "!");
		$CACHE::MapChanger::NewVoteSys::VOTES_[$CACHE::MapChanger::NewVoteSys::MAP_[%c.alreadyVoted]]--;
		$CACHE::MapChanger::NewVoteSys::VOTES_[$CACHE::MapChanger::NewVoteSys::MAP_[%t]]++;
		
		%c.alreadyVoted = %t;
	} else {
		%c.alreadyVoted = %t;
		
		%c.chatMessage("\c6You voted for " @ $CACHE::MapChanger::NewVoteSys::MAPNAME_[$CACHE::MapChanger::NewVoteSys::MAP_[%t]] @ "!");
		$CACHE::MapChanger::NewVoteSys::VOTES_[$CACHE::MapChanger::NewVoteSys::MAP_[%t]]++;
	}
	
	MapChanger_UpdateAllClientVotes();
}

function testMapChanger() {
	$Server::MapChanger::NewVoteSys_Changing = 1;
	
	if(!isObject(%mini = Slayer_MinigameHandlerSO.getObject(0))) return;
	MapChanger_StartNewMapVote();
	%mapC = $CACHE::MapChanger::NewVoteSys::MAP_COUNT;
	
	for(%i=0;%i<(%mapC >= %mini.numMembers ? %mini.numMembers : %mapC);%i++) {
		%client = %mini.member[%i];
		MCV(%client, (%i <= 1 ? 0 : %i - 1));
	}
	
	schedule(1000, 0, MapChanger_EndVote, %mini);
}

function MapChanger_EndVote(%mini) {
	$Server::MapChanger::NewVoteSys_Changing = 0;
	cancel($CACHE::MapChanger::NewVoteSys::EndSchedule);
	
	for(%i=0;%i<%mini.numMembers;%i++) {
		%client = %mini.member[%i];
		
		%client.alreadyVoted = "";
		%client.displayCenterPrintMenu(0, "stop");

		%client.dataInstance($DR::SaveSlot).DR_NoHud = %client.oldDR_NoHud;
	}
	
	%score = "0 ?";
	
	for(%i=0;%i<$CACHE::MapChanger::NewVoteSys::MAP_COUNT;%i++) {
		%mapStr  = $CACHE::MapChanger::NewVoteSys::MAP_[%i];
		%mapFile = $CACHE::MapChanger::NewVoteSys::MAPFILE_[%mapStr];
		%votes   = $CACHE::MapChanger::NewVoteSys::VOTES_[%mapStr];
		
		if(%votes > getWord(%score, 0)) { %score = %votes SPC %mapFile; continue; }
	}
	
	if(%score $= "0 ?") {
		Slayer_MiniGameSO::endRound(%mini, 0, 0, 1);
		announce("\c6No one voted; staying on same map.");
		return;
	}
	
	for(%i=0;%i<$CACHE::MapChanger::NewVoteSys::MAP_COUNT;%i++) {
		%mapStr  = $CACHE::MapChanger::NewVoteSys::MAP_[%i];
		%mapFile = $CACHE::MapChanger::NewVoteSys::MAPFILE_[%mapStr];
		%votes   = $CACHE::MapChanger::NewVoteSys::VOTES_[%mapStr];
		
		if(%mapFile !$= removeWord(%score, 0) && %votes == getWord(%score, 0)) %tieMap[-1 + %tieMapCount++] = %mapFile;
	}
	
	%mapFile = removeWord(%score, 0);
	
	if(%tieMap[0] !$= "") {
		%tieMap[-1 + %tieMapCount++] = %mapFile;
		
		for(%i=0;%i<%tieMapCount;%i++) {
			%str = (%str $= "" ? "" : (%i == %tieMapCount - 1 ? %str @ " and " : %str @ ", ")) @ fileBase(%tieMap[%i]);
			
			if(strLen(%str) + 51 >= 1000) {
				%str = %tieMapCount SPC "map" @ (%tieMapCount == 1 ? "" : "s");
				break;
			}
		}
		
		announce("\c6There was a tie between " @ %str @ "; Picking a map at random.");
		%mapFile = %tieMap[mFloor(getRandom(0, %tieMapCount * 100) / 100)];
		announce("\c6Randomly picked " @ fileBase(%mapFile) @ "!");
	} else {
		announce("\c6Most votes were found on \"" @ fileBase(%mapFile) @ "\"!");
	}
	
	if(fileBase(%mapFile) $= $Server::MapChanger::CurrentMap) {
		announce("\c6" @ getWord(%score, 0) @ " user(s) voted to stay on the same map!");
		%mini.schedule(6000, reset);
		return;
	}
	
	$Server::MapChanger::DoChange = 1;
	$Server::MapChanger::ChangeTo = %mapFile;
	
	%mini.endRound();
}

function MapChanger_StartNewMapVote(%this) {
	if(!isObject(%mini = Slayer_MinigameHandlerSO.getObject(0))) return;
	
	MapChanger_ResetVotes();
	
	%path = $Pref::Server::MapChanger::Path @ "*.bls";
	
	%mapListing = "";
	%index      = -1;
	
	deleteVariables("$CACHE::MapChanger::NewVoteSys::*");
	
	for(%file = findFirstFile(%path); %file !$= ""; %file = findNextFile(%path)) {
		%index++;
		%mapStr = getSafeVariableName(stripMLControlChars(fileBase(%file)));
		%difficultPrint = "";
		%mapSizePrint = "";
		%playerPrint = "";
		%col = "";
		%difficult = "";
		%mCol = "";
		%size = "";

		switch$($MapChanger::MapDifficult[%mapStr]) {
			case "Unknown":
				%col = "\c7";
				%difficult = "Unknown";
			case "Very easy":
				%col = "\c4";
				%difficult = "Very easy";
			case "Easy":
				%col = "\c2";
				%difficult = "Easy";
			case "Medium":
				%col = "\c3";
				%difficult = "Medium";
			case "Hard":
				%col = "\c0";
				%difficult = "Hard";
			case "Very hard":
				%col = "\c0";
				%difficult = "Very hard";
			case "Nightmare":
				%col = "\c8";
				%difficult = "Nightmare";
			default:
				%noDifficultPrint = 1;
		}

		if(!%noDifficultPrint)
		%difficultPrint = "\c6(Difficult: " @ %col @ %difficult @ "\c6)";

		switch$($MapChanger::MapSize[%mapStr]) {
			case "Unknown":
				%mCol = "\c7";
				%size = "Unknown";
			case "Very small":
				%mCol = "\c4";
				%size = "Very small";
			case "Small":
				%mCol = "\c2";
				%size = "Small";
			case "Medium":
				%mCol = "\c3";
				%size = "Medium";
			case "Large":
				%mCol = "\c0";
				%size = "Large";
			case "Very large":
				%mCol = "\c0";
				%size = "Very large";
			case "Planet":
				%mCol = "\c8";
				%size = "Planet";
			default:
				%noSizePrint = 1;
		}
		
		if(!%noSizePrint)
			%mapSizePrint = "\c6(Map size: " @ %mCol @ %size @ "\c6)";
		
		if((%players = $MapChanger::MapRequirePlayers[%mapStr]) > 0)
			%playerPrint = "\c6(Players max: " @ (ClientGroup.getCount() > %players ? "\c0" : "\c2") @ %players @ "\c6)";
		
		$CACHE::MapChanger::NewVoteSys::DEFFICULTY_[%mapStr] = %difficult;
		$CACHE::MapChanger::NewVoteSys::MAPSIZE_[%mapStr] = %size;
		$CACHE::MapChanger::NewVoteSys::PLRSTR_[%mapStr]  = %playerPrint;
		$CACHE::MapChanger::NewVoteSys::INDEX_[%mapStr]   = %index;
		$CACHE::MapChanger::NewVoteSys::VOTES_[%mapStr]   = 0;
		$CACHE::MapChanger::NewVoteSys::MAPFILE_[%mapStr] = %file;
		$CACHE::MapChanger::NewVoteSys::MAPNAME_[%mapStr] = fileBase(%file);
		$CACHE::MapChanger::NewVoteSys::MAP_[-1 + $CACHE::MapChanger::NewVoteSys::MAP_COUNT++] = %mapStr;

		if((%fileName = fileBase(%file)) !$= "") {
			%votes = MapChanger_GetVoteCount(%fileName);
			//%mapListing = trim(%mapListing TAB "MCV(%C%," @ %index @ ");" SPC fileBase(%file) SPC trim(%difficultPrint SPC %mapSizePrint SPC %playerPrint SPC "\c7| \c3") @ %votes SPC (%votes == 1 ? "vote" : "votes"));
			%mapListing = trim(%mapListing TAB "MCV(%C%," @ %index @ ");" SPC fileBase(%file) SPC "\c7| \c3" @ %votes SPC (%votes == 1 ? "vote" : "votes"));
		}
	}
	
	cancel($CACHE::MapChanger::NewVoteSys::EndSchedule);
	$CACHE::MapChanger::NewVoteSys::EndSchedule = schedule(60000, 0, MapChanger_EndVote, %mini);
	
	// Make it so the brickmenu menu cannot be changed
	$CACHE::CenterPrintMenu::CanBeClosedAfterDone = 0;
	$CACHE::CenterPrintMenu::CanBeClosed = 0;
	
	for(%i=0;%i<%mini.numMembers;%i++) {
		%client = %mini.member[%i];
		
		if(isObject(%player = %client.player) && %client.getControlObject() != %client.camera) {
			//%client.camera.setControlObject(-1);
			//%client.camera.setOrbitMode(%player, "0 0 0", 0, 6, 6, 1);
			//%client.setControlObject(%client.camera);
			//%player.invulnerable = 1;
			%player.kill();
		}
		
		//if(isObject(%veh = %player.getObjectMount())) %veh.setVelocity("0 0 0");
		//else %player.setVelocity("0 0 0");
		%client.oldDR_NoHud = %client.dataInstance($DR::SaveSlot).DR_NoHud;
		%client.dataInstance($DR::SaveSlot).DR_NoHud    = 1;
		%client.alreadyVoted = "";
		%client.schedule(1000, displayCenterPrintMenu, 0, strReplace(%mapListing, "%C%", %client));
	}
	
	MapChanger_updateTimeLeft();
}


// RTV

function MapChanger_ResetRTV() {
	$Server::MapChanger::RTV_Vote_Active = 0;
	$Server::MapChanger::RTVCount = 0;
	for(%i=0;%i<clientGroup.getCount();%i++) clientGroup.getObject(%i).rtvVote = 0;
}

function MapChanger_RTVFail() {
	echo("The RTV failed (not enough people voted).");
	
	messageAll('', "\c0RTV\c6: Not enough people did \c7/rtv\c6.");
	$CACHE::MapChanger::LastRTVFail = getSimTime();
	MapChanger_ResetRTV();
}

function MapChanger_getRTVPercent() {
	%max     = ClientGroup.getCount() / 2;
	%percent = mClamp(($Server::MapChanger::RTVCount / %max) * 100, 0, 100);
	
	return %percent;
}

function serverCmdRTV(%this) {
	if(getSimTime() - $CACHE::MapChanger::LastRTVFail < $Pref::MapChanger::RTVFailWait) {
		%timeMS = $Pref::MapChanger::RTVFailWait - (getSimTime() - $CACHE::MapChanger::LastRTVFail);
		%timeH  = mFloor(%timeMS / (3600000|0));
		%timeMS = %timeMS % (3600000|0);
		%timeM  = mFloor(%timeMS / 60000);
		%timeMS = %timeMS % 60000;
		%timeS  = mFloor(%timeMS / 1000);
		%time   = getSubStr("00" @ %timeH, strLen("00" @ %timeH) - 2, 2);
		%time   = %time @ ":" @ getSubStr("00" @ %timeM, strLen("00" @ %timeM) - 2, 2);
		%time   = %time @ ":" @ getSubStr("00" @ %timeS, strLen("00" @ %timeS) - 2, 2);
		
		messageClient(%this, '', "\c0RTV\c6: Time remaining before next RTV: " @ %time);
		return;
	}
	
	if($CACHE::MapChanger::WonRounds <= 1) {
		%this.chatMessage("\c0RTV\c6: Please wait a few rounds before using RTV.");
		return;
	}
	
	if($Server::MapChanger::NewVoteSys_Changing) {
		%this.chatMessage("\c0RTV\c6: The map vote is taking place right now!");
		return;
	}
	
	if($CACHE::MapChanger::DoRTVChange) {
		%this.chatMessage("\c0RTV\c6: The map will change after this round. Be patient!");
		return;
	}
	
	if($Server::MapChanger::DoChange) {
		%this.chatMessage("\c0RTV\c6: You currently cannot vote right now. (Changing soon)");
		return;
	}
	
	if($Server::MapChanger::Changing) {
		%this.chatMessage("\c0RTV\c6: You currently cannot vote right now. (Currently changing)");
		return;
	}
	
	if(!%this.hasSpawnedOnce) {
		%this.chatMessage("\c0RTV\c6: Please wait until you spawn in.");
		return;
	}
	
	if(%this.rtvVote) {
		%this.chatMessage("\c0RTV\c6: You already rocked the vote!");
		return;
	}
	
	$Server::MapChanger::RTVCount++;
	
	%percent = MapChanger_getRTVPercent();
	
	if(!isEventPending($MapChanger_RTVFailSchedule)) {
		echo(%this.getSimpleName() SPC "(BL_ID:" @ %this.getBLID() @ ") started a RTV chain.");
		$Server::MapChanger::RTV_Vote_Active = 1;
		$MapChanger_RTVFailSchedule = schedule(60000, 0, MapChanger_RTVFail);
	} else {
		echo(%this.getSimpleName() SPC "(BL_ID:" @ %this.getBLID() @ ") rocked the vote (" @ %percent @ "%)");
	}
	
	%this.rtvVote = 1;
	
	messageAll('MsgAdminForce', "\c0RTV\c6: " @ %this.getPlayerName() @ " wants to rock the vote! Do \c7/rtv\c6 to rock the vote! (" @ %percent @ "%)");
	
	if(%percent >= 100) {
		messageAll('', "\c0RTV\c6: RTV succeeded! Map change will happen next round.");
		cancel($MapChanger_RTVFailSchedule);
		
		for(%i=0;%i<clientGroup.getCount();%i++) clientGroup.getObject(%i).rtvVote = 0;
		
		$Server::MapChanger::RTVCount = 0;
		$CACHE::MapChanger::DoRTVChange = 1;
	}
}