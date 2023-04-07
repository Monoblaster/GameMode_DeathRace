function DR_CreateBrickgroup()
{
	if(!isObject(BrickGroup_48))
	{
		%group = new SimGroup(BrickGroup_48)
		{
			client = -1;
			bl_id = 48;
			name = "Deathrace";
			DoNotDelete = 1;
		};
		MainBrickgroup.add(%group);
	}
}
schedule(0, 0, DR_CreateBrickgroup);


if(!isObject(StereoHandlerGroup))
	new SimSet(StereoHandlerGroup);

function VehicleStereo_Loop()
{
	cancel($StereoHandlerSch);
	if(StereoHandlerGroup.getCount() <= 0) //Don't keep going until we have something.
		return;

	for(%i = StereoHandlerGroup.getCount()-1; %i >= 0; %i--)
	{
		%obj = StereoHandlerGroup.getObject(%i);
		if(isObject(%mount = %obj.mount))
			%obj.setTransform(%mount.getPosition() SPC %obj.rotation);
		else
			%obj.schedule(0, delete);
	}

	$StereoHandlerSch = schedule($Pref::Server::VehicleStereoTick, 0, VehicleStereo_Loop);
}

$StereoHandlerSch = schedule(100, 0, VehicleStereo_Loop);

function vectorRound(%vec)
{
	return mFloatLength(getWord(%vec, 0), 0) SPC mFloatLength(getWord(%vec, 1), 0) SPC mFloatLength(getWord(%vec, 2), 0);
}

if(!strLen($Pref::Server::VehicleStereoTick))
	$Pref::Server::VehicleStereoTick = 10;

function serverCmdAutoStereo(%client)
{
	%client.autoStereoMusic = !%client.autoStereoMusic;
	%client.chatMessage("\c6Automatic stereo is now " @ 
		(%client.autoStereoMusic ? "\c3ON\c6. When you jump in a vehicle when there is no driver or you are the driver, it will choose the last music you set." 
			: "\c2OFF\c6."));
}

function serverCmdRandomStereo(%client)
{
	if(MusicDataCache.itemCount <= 0)
		return;

	if(!isObject(%player = %client.player))
		return;
	
	if (!isObject(%mount = %player.GetBaseMount()))
		return;

	if($Sim::Time - %player.lastRandomMusicTime < 1.5)
		return;
	
	if(isObject(%driver = %mount.getControllingObject()))
	{
		if(%driver == %player)
			%yes = 1;
		else if(!%driver.client.driverStereoMusic)
			%yes = 1;
	}
	else
		%yes = 1;

	if(%yes)
	{
		if(!isObject(%mount.stereoHandler))
		{
			%mount.stereoHandler = new fxDTSBrick()
			{
				client = %client;
				dataBlock = brickMusicData;
				isPlanted = true;
				isStereo = true;
				mount = %mount;
				position = "0 0 -10000";
			};
			if(isObject(BrickGroup_48))
				BrickGroup_48.add(%mount.stereoHandler);
		}

		%musicData = MusicDataCache.item[getRandom(0, MusicDataCache.itemCount-1)];

		%brick = %mount.stereoHandler;

		%brick.setMusic(%musicData);

		%musicName = %musicData.uiName;
		%client.lastStereoMusic = %musicName;
		if(%musicName !$= %brick.lastMusic)
			for(%i = 0; %i < %mount.getDatablock().numMountPoints; %i++)
			{
				if(isObject(%obj = %mount.getMountedObject(%i)) && isObject(%cl = %obj.client) && %cl.getClassName() $= "GameConnection")
					%cl.chatMessage("\c7[\c4Vehicle\c7] \c1Randomized \c6- \c3" @ %client.getPlayerName() @ " \c6has changed the music to \c3" @ %musicName @ "\c6.");
			}

		if(isObject(%audio = %brick.audioEmitter))
		{
			%audio.mount = %brick.mount;
			if(!StereoHandlerGroup.isMember(%audio))
			{
				StereoHandlerGroup.add(%audio);

				if(!isEventPending($StereoHandlerSch)) //If we know the loop isn't running, let's run it since we have something now.
					VehicleStereo_Loop();
			}
		}

		%player.lastRandomMusicTime = $Sim::Time;
	}
}

function serverCmdStereo(%client)
{
	if(!isObject(%player = %client.player))
		return;
	
	if (!isObject(%mount = %player.GetBaseMount()))
		return;
	
	if(!isObject(%mount.stereoHandler))
	{
		%mount.stereoHandler = new fxDTSBrick()
		{
			client = %client;
			dataBlock = brickMusicData;
			isPlanted = true;
			isStereo = true;
			mount = %mount;
			position = "0 0 -10000";
		};
		if(isObject(BrickGroup_48))
			BrickGroup_48.add(%mount.stereoHandler);
	}

	if(isObject(%driver = %mount.getControllingObject().client))
	{
		if(%driver != %client)
			if(%driver.driverStereoMusic)
			{
				%client.chatMessage("\c6Sorry, the driver currently has locked the stereo for non-drivers in the vehicle.");
				return;
			}
	}

	%client.wrenchBrick = %mount.stereoHandler;
	%client.wrenchBrick.sendWrenchSoundData(%client);
	commandToClient(%client, 'openWrenchSoundDlg', "Vehicle Stereo", 1);
}

function serverCmdDriverMusic(%client)
{
	%client.driverStereoMusic = !%client.driverStereoMusic;
	%client.chatMessage("\c6Driver stereo music is now " @ 
		(%client.driverStereoMusic ? "\c3ON\c6. Nobody but you can change the music for any vehicle you drive." 
			: "\c2OFF\c6. Anyone can change the music when you are the driver."));
}

function serverCmdStereoHelp(%client)
{
	%client.chatMessage("\c6Welcome to the new stereo mod made by \c4Kyuande, BL_ID: 48980\c6.");
	%client.chatMessage("  - \c6What's new? - Added driver only music; if you have music off in your options, it will no longer play music; use your sit command to open the stereo when in a vehicle!");
	%client.chatMessage(" ");
	%client.chatMessage("/DriverMusic \c7- \c6Toggles if only the driver (you) can change the music. Useful if you don't want idiots changing it to weird music.");
	%client.chatMessage("/AutoStereo \c7- \c6Chooses the last song you choose when entering a vehicle (checks are required before song is played).");
	%client.chatMessage("/Stereo \c7- \c6Opens the stereo if in a vehicle. May not open if the driver locks the stereo to \c3driver only\c6.");
	%client.chatMessage("/RandomStereo \c7- \c6Randomizes the stereo. May not work if the driver locks the stereo to \c3driver only\c6. \c3You can also double tap your love emote key to do this.");
	%client.chatMessage("  - \c6You can also do /sit or use your sit button to open the stereo.");
	%client.chatMessage("You may need to page up if you didn't see the \"Welcome\" message.");
}


package VehicleStereo
{
	function serverCmdSetWrenchData(%client, %data)
	{
		%brick = %client.wrenchBrick;
		if(%brick.isStereo)
		{
			if(isObject(%mount = %brick.mount))
			{
				if(isObject(%driver = %mount.getControllingObject().client))
					if(%driver != %client)
						if(%driver.driverStereoMusic)
						{
							%client.chatMessage("\c6Sorry, the driver currently has locked the stereo for non-drivers in the vehicle.");
							return;
						}

				if(%client.lastStereoMusic $= %musicName && %client.lastStereoMusicMount == %mount)
					return;

				%musicData = getWord(getField(%data, 1), 1);
				%brick.setMusic(%musicData);
				if(!isObject(%musicData))
				{
					for(%i = 0; %i < %mount.getDatablock().numMountPoints; %i++)
					{
						if(isObject(%obj = %mount.getMountedObject(%i)) && isObject(%cl = %obj.client) && %cl.getClassName() $= "GameConnection")
							%cl.chatMessage("\c7[\c4Vehicle\c7] \c3" @ %client.getPlayerName() @ " \c6has turned the stereo off.");
					}
					%client.lastStereoMusic = 0;
					return;
				}

				%musicName = %musicData.uiName;
				%client.lastStereoMusic = %musicName;
				%client.lastStereoMusicMount = %mount;
				if(%musicName !$= %brick.lastMusic)
					for(%i = 0; %i < %mount.getDatablock().numMountPoints; %i++)
					{
						if(isObject(%obj = %mount.getMountedObject(%i)) && isObject(%cl = %obj.client) && %cl.getClassName() $= "GameConnection")
							%cl.chatMessage("\c7[\c4Vehicle\c7] \c3" @ %client.getPlayerName() @ " \c6has changed the music to \c3" @ %musicName @ "\c6.");
					}

				%brick.lastMusic = %musicName;

				if(isObject(%audio = %brick.audioEmitter))
				{
					%audio.mount = %brick.mount;
					if(!StereoHandlerGroup.isMember(%audio))
					{
						StereoHandlerGroup.add(%audio);

						if(!isEventPending($StereoHandlerSch)) //If we know the loop isn't running, let's run it since we have something now.
							VehicleStereo_Loop();
					}
				}
			}
		}
		else
			Parent::serverCmdSetWrenchData(%client, %data);
	}
	
	function ShapeBase::OnRemove(%data,%obj)
	{
		if(isObject(%handler = %obj.stereoHandler))
			%handler.delete();

		Parent::OnRemove(%data,%obj);
	}

	function serverCmdLove(%this)
	{
		if(%this.useStereoLight)
		{
			%this.useStereoLight = 0;
			%this.stereoClick = 0;
			return Parent::serverCmdLove(%this);
		}

		if(!isObject(%player = %this.player))
			return;

		if(!isObject(%vehicle = %player.GetBaseMount()))
			return Parent::serverCmdLove(%this);

		if(%this.stereoClick <= 0)
		{
			cancel(%this.toggleStereoLightSch);
			%this.stereoClick = 1;
			%this.toggleStereoLightSch = %this.schedule(600, ToggleStereoLight);
		}
		else
		{
			cancel(%this.toggleStereoLightSch);
			%this.stereoClick = 0;
			serverCmdRandomStereo(%this);
		}
	}

	function serverCmdSit(%client)
	{
		if(isObject(%player = %client.player) && isObject(%mount = %player.GetBaseMount()))
		{
			serverCmdStereo(%client);
			return;
		}

		return Parent::serverCmdSit(%client);
	}

	function Armor::onMount(%this, %player, %obj, %a, %b, %c, %d, %e, %f)
	{
		if(isObject(%vehicle = %player.GetBaseMount()) && %vehicle.getDamageLevel() < %vehicle.getDatablock().maxDamage && isObject(%client = %player.client) && %client.autoStereoMusic)
		{
			if(isObject(%driver = %vehicle.getMountNodeObject(0)))
			{
				if(%driver == %player)
					%yes = 1;
			}
			else if(%client.autoStereoMusic)
				%yes = 1;

			if($Sim::Time - %client.lastStereoMusicTime < 5)
			{
				%client.centerPrint("You are changing stereo too fast!", 3);
				Parent::onMount(%this, %player, %obj, %a, %b, %c, %d, %e, %f);
				return;
			}

			if(%yes && (%music = %client.lastStereoMusic) !$= "" && !isObject(%vehicle.stereoHandler))
			{
				%client.lastStereoMusicTime = $Sim::Time;
				%vehicle.stereoHandler = new fxDTSBrick()
				{
					client = %client;
					dataBlock = brickMusicData;
					isPlanted = true;
					isStereo = true;
					mount = %vehicle;
					position = "0 0 -10000";
				};
				if(isObject(BrickGroup_48))
					BrickGroup_48.add(%vehicle.stereoHandler);
				%brick = %vehicle.stereoHandler;

				%brick.setMusic(nameToID(findMusicByName(%music)));

				if(isObject(%audio = %brick.audioEmitter))
				{
					%audio.mount = %brick.mount;
					if(!StereoHandlerGroup.isMember(%audio))
					{
						StereoHandlerGroup.add(%audio);

						if(!isEventPending($StereoHandlerSch)) //If we know the loop isn't running, let's run it since we have something now.
							VehicleStereo_Loop();
					}
				}
			}
		}

		Parent::onMount(%this, %player, %obj, %a, %b, %c, %d, %e, %f);
	}
};
deactivatePackage(VehicleStereo);
activatePackage(VehicleStereo);

function GameConnection::ToggleStereoLight(%this)
{
	%this.useStereoLight = 1;
	serverCmdLove(%this);
}