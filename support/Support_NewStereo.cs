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

cancel($StereoHandlerSch);
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

function NewStereo_Set(%mount,%musicData)
{
	if(!isObject(%mount.stereoHandler) && isObject(%musicData))
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

	if(isObject(%mount.stereoHandler))
	{
		%brick = %mount.stereoHandler;

		%brick.setMusic(%musicData);

		%brick.lastMusic = %musicData;
		if(isObject(%audio = %brick.audioEmitter))
		{
			%audio.mount = %brick.mount;
			if(!StereoHandlerGroup.isMember(%audio))
			{
				StereoHandlerGroup.add(%audio);
			}
		}
		cancel($StereoHandlerSch);
		$StereoHandlerSch = schedule(100, 0, VehicleStereo_Loop);
	}
}

function NewStereo_Auto(%p)
{
	%c = %p.client;
	%music = %c.lastStereoMusic;

	if(!isObject(%c) || !%c.autoStereoMusic || !isObject(%music))
	{
		return false;
	}

	%vehicle = %p.GetBaseMount();
	if(!isObject(%vehicle) || !%vehicle.isEnabled() || !%vehicle.getDataBlock().rideable)
	{
		return false;
	}

	if(%vehicle.getControllingObject() != %p)
	{
		return false;
	}

	if($Sim::Time - %c.lastStereoMusicTime < 5)
	{
		%c.ChatMessage("You are changing stereo too fast!", 3);
		return false;
	}
	%c.lastStereoMusicTime = $Sim::Time;

	NewStereo_Menu(%vehicle,%c,%music);
	return true;
}

function NewStereo_Menu(%mount,%client,%musicData)
{
	%message = " \c6has turned the stereo off.";
	if(isObject(%musicData))
	{
		if(%mount.stereoHandler.audioEmitter.profile.getId() == %musicData.getId())
		{
			return true;
		}
		%message = " \c6has changed the music to \c3" @ %musicData.uiName @ "\c6.";
	}
	NewStereo_Set(%mount,%musicData);
	%client.lastStereoMusic = %musicData;
	%client.lastStereoMusicMount = %mount;

	%mount.MessageClients("\c7[\c4Vehicle\c7] \c3" @ %client.getPlayerName() @ %message);
	return true;
}

//this uses a object created within the gamemode
function NewStereo_GetRandom()
{
	return %musicData = MusicDataCache.item[getRandom(0, MusicDataCache.itemCount-1)];
}

function serverCmdRandomStereo(%client)
{
	if(MusicDataCache.itemCount <= 0)
		return false;

	if(!isObject(%player = %client.player))
		return false;

	%mount = %player.GetBaseMount();
	if (!isObject(%mount) || !%mount.getDataBlock().rideable)
		return false;

	if(isObject(%driver = %mount.getControllingObject().client))
	{
		if(%driver != %client)
			if(%driver.driverStereoMusic)
			{
				%client.chatMessage("\c6Sorry, the driver currently has locked the stereo for non-drivers in the vehicle.");
				return false;
			}
	}

	if($Sim::Time - %player.lastRandomMusicTime < 1.5)
		return false;
	%player.lastRandomMusicTime = $Sim::Time;
	
	%musicData = NewStereo_GetRandom();
	NewStereo_Set(%mount,%musicData);

	%client.lastStereoMusic = %musicData;
	%client.lastStereoMusicMount = %mount;
	%mount.MessageClients("\c7[\c4Vehicle\c7] \c1Randomized \c6- \c3" @ %client.getPlayerName() @ " \c6has changed the music to \c3" @ %musicData.uiName @ "\c6.");
	return true;
}

function serverCmdStereo(%client)
{
	if(!isObject(%player = %client.player))
		return;
	
	if (!isObject(%mount = %player.GetBaseMount()) || !%mount.getDataBlock().rideable)
		return;

	if(isObject(%driver = %mount.getControllingObject().client))
	{
		if(%driver != %client)
			if(%driver.driverStereoMusic)
			{
				%client.chatMessage("\c6Sorry, the driver currently has locked the stereo for non-drivers in the vehicle.");
				return;
			}
	}

	%client.newStereo_MenuMount = %mount;
	if(isObject(%mount.stereoHandler))
	{
		%mount.stereoHandler.sendWrenchSoundData(%client);
	}
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

function serverCmdStereoMe(%c,%random)
{
	%p = %c.player;

	if(!isObject(%p) || !%c.isAdmin)
		return;

	if(!%random)
	{
		%c.newStereo_MenuMount = %p;
		if(isObject(%p.stereoHandler))
		{
			%p.stereoHandler.sendWrenchSoundData(%c);
		}
		else
		{
			NewStereo_Set(%p,"");
		}
		commandToClient(%c, 'openWrenchSoundDlg', "Vehicle Stereo", 1);
		return;
	}

	if(MusicDataCache.itemCount <= 0)
		return false;

	%musicData = NewStereo_GetRandom();
	NewStereo_Set(%p,%musicData);

	%c.lastStereoMusic = %musicData;
	%c.lastStereoMusicMount = %mount;
	%p.MessageClients("\c7[\c4Vehicle\c7] \c1Randomized \c6- \c3" @ %c.getPlayerName() @ " \c6has changed the music to \c3" @ %musicData.uiName @ "\c6.");
	return true;
}


package VehicleStereo
{
	function serverCmdSetWrenchData(%client, %data)
	{
		%player = %client.player;
		%mount = %client.newStereo_MenuMount;
		if(isObject(%mount) && isObject(%player))
		{
			%currMount = %player.GetBaseMount();
			%client.newStereo_MenuMount = "";
			%musicData = getWord(getField(%data,1),1);
			if(%mount == %currMount && NewStereo_Menu(%mount,%client,%musicData))
			{
				return;
			}
		}
		Parent::serverCmdSetWrenchData(%client, %data);
	}
	
	function ShapeBase::OnRemove(%data,%obj)
	{
		if(isObject(%handler = %obj.stereoHandler))
			%handler.delete();

		Parent::OnRemove(%data,%obj);
	}

	function serverCmdLove(%c)
	{
		if($VehicleStereoLoveBypass || !isObject(%p) || !isObject(%mount = %p.GetBaseMount()) || !%mount.getDataBlock().rideable)
		{
			$VehicleStereoLoveBypass = false;
			return parent::serverCmdLove(%c);
		}

		if(!%c.doRandomStereo)
		{
			cancel(%c.TimeoutRandomStereo);
			%c.doRandomStereo = true;
			%c.TimeoutRandomStereo = %c.schedule(600, TimeoutRandomStereo);
			return;
		}
		else
		{
			cancel(%c.TimeoutRandomStereo);
			%c.doRandomStereo = false;
			if(serverCmdRandomStereo(%c))
			{
				return;
			}
		}
		return parent::serverCmdLove(%c);
	}

	function serverCmdSit(%client)
	{
		serverCmdStereo(%client);
		return Parent::serverCmdSit(%client);
	}

	function Armor::onMount(%this, %player, %obj, %a, %b, %c, %d, %e, %f)
	{
		
		%r = Parent::onMount(%this, %player, %obj, %a, %b, %c, %d, %e, %f);
		NewStereo_Auto(%player);
		return  %r;
	}
};
activatePackage(VehicleStereo);

function GameConnection::TimeoutRandomStereo(%this)
{
	%this.doRandomStereo = false;
	$VehicleStereoLoveBypass = true;
	serverCmdLove(%this);
}