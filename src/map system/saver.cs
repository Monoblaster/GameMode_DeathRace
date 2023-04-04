function MC_Save1_begin(%name)
{
	%name = trim(stripMLControlChars(%name));
	if(%name $= "")
		return;

	if($Server::MCSaver::IsInUse)
		return;

	MC_Save_SetState("(AS1) Save init");
	echo("[MapChangerSaver] - Attempting to save bricks...");
	echo("  - Ownership");
	echo("  - Events");

	deleteVariables("$Server::MCSaverGps::group*");
	if(!isObject($Server::MCSaver::SaveObject))
		$Server::MCSaver::SaveObject = new GuiTextListCtrl("Server_MapChangerSaverList");
	else
		$Server::MCSaver::SaveObject.clear();

	if(getBrickCount() <= 0)
	{
		MC_Save_SetState("(AS1) No bricks");
		announce("\c6There are no bricks to save.");
		echo("[MapChangerSaver] - No bricks to save.");
		return;
	}

	$Server::MCSaver::IsInUse = 1;

	announce("\c6(\c3MapChanger\c6) \c6Saving bricks... ");

	$time_beg = $Sim::Time;

	$Server::MCSaver::SaveName = %name;
	$Server::MCSaverGps::group_count = 0;
	$Server::MCSaverGps::cur_group = 0;
	$Server::MCSaverGps::brick_count = 0;
	$Server::MCSaverGps::event_count = 0;

	MC_Save_SetState("(AS1) Save - Collecting");
	for(%i = 0; %i < mainBrickGroup.getCount(); %i++)
	{
		%g = mainBrickGroup.getObject(%i);
		%b = %g.getCount();
		if(%b > 0)
		{
			%g.MC_Save_stop = %b;
			$Server::MCSaverGps::group[$Server::MCSaverGps::group_count] = %g;
			$Server::MCSaverGps::group_count++;
		}
	}

	MC_Save_SetState("(AS1) Save - Finished collecting");

	if(isObject($Server::MCSaverGps::group[0]))
	{
		MC_Save_SetState("(AS1) Begin to save " @ $Server::MCSaverGps::group_count @ " group" @ ($Server::MCSaverGps::group_count != 1 ? "s" : ""));
		MC_Save1_nextGroup();
	}
	else
	{
		announce("\c6There are no bricks to save.");
		echo("[MapChangerSaver] - No bricks to save.");
	}
}

function MC_Save1_nextGroup()
{
	if(!$Server::MCSaver::IsInUse)
		return;

	if($Server::MCSaverGps::cur_group == $Server::MCSaverGps::group_count)
	{
		%count = $Server::MCSaver::SaveObject.rowCount();
		MC_Save_SetState("(AS1) Saved " @ %count @ " brick" @ (%count != 1 ? "s" : "") @ ", sorting");
		$Server::MCSaver::SaveObject.sortNumerical(0, 1);
		return MC_Save2_begin();
	}

	%g = $Server::MCSaverGps::group[$Server::MCSaverGps::cur_group];
	$Server::MCSaverGps::cur_group++;

	MC_Save1_nextBrick(%g, 0);
}

function MC_Save1_nextBrick(%g, %c)
{
	if(!$Server::MCSaver::IsInUse)
		return;

	if(!isObject(%g))
		return;

	if(%c >= %g.getCount())
		return MC_Save1_nextGroup();

	%brick = nameToID(%g.getObject(%c));

	if(!isObject($Server::MCSaver::SaveObject))
		return;

	if(%brick.isPlanted)
	{
		if($Server::MCSaver::SaveObject.getRowNumByID(%brick) == -1)
			$Server::MCSaver::SaveObject.addRow(%brick, %brick.getDistanceFromGround());

		$Server::MCSaverGps::brick_count += 1;
	}
	else if(isObject(%brick))
	{
		%del = 1;
		for(%i = 0; %i < ClientGroup.getCount(); %i++)
		{
			if(ClientGroup.getObject(%i).player.tempBrick == %brick)
				%del = 0;
		}

		if(%del) //This helps delete unwanted temp bricks that don't belong to anyone
			%brick.schedule(0, "delete");
	}

	//if($Pref::Server::AS::SlowDownAfter > 0 && %c > $Pref::Server::AS::SlowDownAfter)
	//	return schedule(1, 0, "MC_Save1_nextBrick", %g, %c + 1);
		
	return schedule(0, 0, "MC_Save1_nextBrick", %g, %c + 1);
}

function MC_Save2_begin()
{
	$Server::MCSaver::LastPrint = "";
	MC_Save_SetState("(AS2) Write init");

	%dir = $Server::MapSys_Path;

	if(isObject($Server::MCSaver::TempB))
	{
		$Server::MCSaver::TempB.close();
		$Server::MCSaver::TempB.delete();
	}

	if(!$Server::MCSaver::IsInUse)
		return;

	$Server::MCSaver::TempB = new FileObject();

	$Server::MCSaver::TempB.path = %dir @ "SAVETEMP.bls";

	$Server::MCSaver::TempB.openForWrite($Server::MCSaver::TempB.path);
	$Server::MCSaver::TempB.writeLine("This is a Blockland save file.  You probably shouldn't modify it cause you'll mess it up.");
	$Server::MCSaver::TempB.writeLine("1");
	$Server::MCSaver::TempB.writeLine(%desc);

	for(%i = 0; %i < 64; %i++)
		$Server::MCSaver::TempB.writeLine(getColorIDTable(%i));

	$Server::MCSaver::TempB.writeLine("Linecount " @ $Server::MCSaver::SaveObject.rowCount());

	$Server::MCSaverGps::brick_count = 0;
	MC_Save2_nextLine($Server::MCSaver::TempB, 0);
}

function MC_Save2_nextLine(%f, %c)
{
	if(!$Server::MCSaver::IsInUse)
	{
		if(isObject(%f))
		{
			%f.close();
			%f.delete();
		}

		return;
	}

	%events = 1;
	%ownership = 1;
	%count = $Server::MCSaver::SaveObject.rowCount();

	if(%c < %count)
	{
		%brick = $Server::MCSaver::SaveObject.getRowID(%c);
		if(isObject(%brick))
		{
			$Server::MCSaverGps::brick_count++;

			//next
			if(%brick.getDataBlock().hasPrint)
			{
				%texture = getPrintTexture(%brick.getPrintId());
				%path = filePath(%texture);
				%underscorePos = strPos(%path, "_");
				%name = getSubStr(%path, %underscorePos + 1, strPos(%path, "_", 14) - 14) @ "/" @ fileBase(%texture);
				if($printNameTable[%name] !$= "")
					%print = %name;
			}

			%f.writeLine(%brick.getDataBlock().uiName @ "\" " @ %brick.getPosition() SPC %brick.getAngleID() SPC %brick.isBasePlate() SPC %brick.getColorID() 
				SPC %print SPC %brick.getColorFXID() SPC %brick.getShapeFXID() SPC %brick.isRayCasting() SPC %brick.isColliding() SPC %brick.isRendering());

			if(%ownership && !$Server::LAN)
				%f.writeLine("+-OWNER " @ getBrickGroupFromObject(%brick).bl_id);

			if(%events)
			{
				if(%brick.getName() !$= "")
					%f.writeLine("+-NTOBJECTNAME " @ %brick.getName());

				for(%b = 0; %b < %brick.numEvents; %b++)
				{
					$Server::MCSaverGps::event_count++;
					//Get rid of this garbage code
					//%targetClass = %brick.eventTargetIdx[%b] >= 0 ? getWord(getField($InputEvent_TargetListfxDTSBrick_[%brick.eventInputIdx[%b]], %brick.eventTargetIdx[%b]), 1) : "fxDtsBrick";
					//%paramList = $OutputEvent_parameterList[%targetClass, %brick.eventOutputIdx[%b]];
					//for(%j = 1; %j < 4; %j++)
					//{
					//	%curParam = %brick.eventOutputParameter[%b, %j];
					//	if(getWordCount(%curParam) == 1 && isObject(%curParam))
					//		if((%curParamName = %curParam.getName()) !$= "")
					//			%curParam = %curParamName;

					//	%params = %params TAB %curParam;
					//}

					%params = getFields(%brick.serializeEventToString(%b), 7, 10);
					%f.writeLine("+-EVENT" TAB %b TAB %brick.eventEnabled[%b] TAB %brick.eventInput[%b] TAB %brick.eventDelay[%b] TAB %brick.eventTarget[%b] 
						TAB %brick.eventNT[%b] TAB %brick.eventOutput[%b] TAB %params);
				}
			}
			
			if(isObject(%emitter = %brick.emitter) && isObject(%emitterData = %emitter.getEmitterDatablock()) && (%emitterName = %emitterData.uiName) !$= "")
				%f.writeLine("+-EMITTER " @ %emitterName @ "\" " @ %brick.emitterDirection);

			if(isObject(%light = %brick.getLightID()) && isObject(%lightData = %light.getDataBlock()) && (%lightName = %lightData.uiName) !$= "")
				%f.writeLine("+-LIGHT " @ %lightName @ "\" "); // Not sure if something else comes after the name

			if(isObject(%item = %brick.item) && isObject(%itemData = %item.getDataBlock()) && (%itemName = %itemData.uiName) !$= "")
				%f.writeLine("+-ITEM " @ %itemName @ "\" " @ %brick.itemPosition SPC %brick.itemDirection SPC %brick.itemRespawnTime);
			else if (%brick.itemDirection != 2.0 && %brick.itemDirection !$= "" || %brick.itemPosition != 0.0 || (%brick.itemRespawnTime != 0.0 && %brick.itemRespawnTime != 4000.0))
			{
				%line = "+-ITEM NONE\" " @ %brick.itemPosition SPC %brick.itemDirection SPC %brick.itemRespawnTime;
				%f.writeLine(%line);
			}

			if(isObject(%audioEmitter = %brick.audioEmitter) && isObject(%audioData = %audioEmitter.getProfileID()) && (%audioName = %audioData.uiName) !$= "")
				%f.writeLine("+-AUDIOEMITTER " @ %audioName @ "\" "); // Not sure if something else comes after the name

			if(isObject(%spawnMarker = %brick.vehicleSpawnMarker) && (%spawnMarkerName = %spawnMarker.uiName) !$= "")
				%f.writeLine("+-VEHICLE " @ %spawnMarkerName @ "\" " @ %brick.reColorVehicle);
		}
		
		return schedule(0, 0, "MC_Save2_nextLine", %f, %c + 1);
	}
	else
	{
		MC_Save_SetState("(AS2) Finalize save writing");
		schedule(0, 0, MC_Save2_end, %f);
	}
}

function MC_Save2_end(%f)
{
	%f.close();

	%dir = $Server::MCSaver::SaveName;
	if(%dir $= "")
		%dir = "save";

	%direc = $Server::MapSys_Path @ %dir @ ".bls";

	if(isFile(%direc)) //Overwrite it, just delete it for sure
	{
		fileDelete(%direc);
		%overwrite = " \c6(\c0OVERWRITING\c6)";
	}

	if(isFile(%f.path))
		fileCopy(%f.path, %direc);

	if(isFile(%f.path))
		fileDelete(%f.path);

	%f.delete();

	$Server::MCSaver::IsInUse = 0;

	%diff = ($Sim::Time - $time_beg);
	%time = %diff;
	%msg = "in \c3"@ mFloatLength(%time, 2) @" \c6second" @ (%time == 1 ? "" : "s");
	if(%time < 1)
		%msg = "\c3instantly";

	%bGroups = $Server::MCSaverGps::cur_group;

	$Pref::Server::MapChanger["LastMCSaver"] = %direc;
	if(isFile(%f.path))
		fileCopy(%f.path, "base/server/temp/temp.bls");

	%saveMsg = " Saved as \c3" @ $Pref::Server::MapChanger["LastMCSaver"] @ "\c6.";

	announce("\c6Saved \c3" @ $Server::MCSaverGps::brick_count @" \c6brick" @ ($Server::MCSaverGps::brick_count == 1 ? "" : "s")
		@ " " @ %msg @ %overwrite @ "\c6." @ %saveMsg);

	announce("  \c6- \c3" @ $Server::MCSaverGps::event_count @ " event" @ ($Server::MCSaverGps::event_count == 1 ? "" : "s") @
		" \c6and \c3" @ %bGroups @ " group" @ (%bGroups == 1 ? "" : "s") @ " \c6have been saved.");
	
	echo("[MapChangerSaver] - Saved " @ $Server::MCSaverGps::brick_count @ " bricks " @ %msg @ ". Saved as " @ $Pref::Server::AS["LastMCSaver"] @ %overwrite @ ".");

	if($Pref::Server::AS::Report)
	{
		if(!$Server::MCSaver::RelatedBrickCount)
			echo("   - Saved " @ $Server::MCSaverGps::event_count @ " event" @ ($Server::MCSaverGps::event_count == 1 ? "" : "s") @
				" and " @ %bGroups @ " group" @ (%bGroups == 1 ? "" : "s") @ ".");
	}

	MC_Save_SetState("(AS2) - Write complete");
	MC_Save3_ClearList(1); //Clear list will call the MC_Save1_begin
	setModPaths(getModPaths());
}

function MC_Save3_ClearList(%a)
{
	if(%a)
		MC_Save_SetState("(AS3) - Clearing list");

	if(isObject(%brickList = $Server::MCSaver::SaveObject))
	{
		$Server::MCSaver::DoNotSave = true;
		if(%brickList.rowCount() == 0)
			%brickList.delete();
		else
			%brickList.removeRow(0);
	}
	else
	{
		MC_Save_SetState("(AS3) - Cleared list");
		$Server::MCSaver::DoNotSave = false;
		return;
	}

	$Server::MCSaver::ClearSch = schedule(0, 0, "MC_Save3_ClearList");
}

function MC_Save_SetState(%state)
{
	if(%state $= "")
		return;

	if($Server::MCSaver::State $= %state)
		return;

	$Server::MCSaver::State = %state;
	if($Server::MCSaver::Debug)
	{
		announce("MC_AS_SetState() - Mode set to \c2" @ %state);
		echo("MC_AS_SetState() - Mode set to \c2" @ %state);
	}
}