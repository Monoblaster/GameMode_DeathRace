// DeathRace file

function serverCmdWhyNoTools(%this)
{
	if(!$Pref::Server::DeathRace_DrivingTools)
		return;

	%this.chatMessage("\c5Sorry, using any tools while driving is off. People have been demolishing other cars with melee items and using overpowered weapons.");
	%this.chatMessage("  \c5If you want this disabled, ask people to not demolish other cars.");
}

function serverCmdToggleHUD(%this)
{
	%this.dataInstance($DR::SaveSlot).DR_NoHud = !%this.dataInstance($DR::SaveSlot).DR_NoHud;
	%this.chatMessage("\c6HUD is now " @ (%this.dataInstance($DR::SaveSlot).DR_NoHud ? "\c0OFF" : "\c2ON"));
	if(%this.dataInstance($DR::SaveSlot).DR_NoHud)
		%this.centerPrint(" ", 1);
	else if(isObject(%player = %this.player))
	{
		%player.lastDRPrint = "";
		%this.lastDRPrint = %msg;
	}
}

function serverCmdPublicAccess(%this)
{
	if(!%this.isSuperAdmin)
		return;

	%this.chatMessage("You now have trust with the public.");
	setMutualBrickgroupTrust(%this.getBLID(), 888888, 2);
}

function serverCmdBlack(%this)
{
	if(%this.isAdmin)
		%this.joinTeam("Admin Team");
}

function serverCmdBackwardsDriving(%this)
{
	if(!isObject(%player = %this.player))
		return;

	if(!isObject(%vehicle = %player.getObjectMount()))
		return;

	if(%player != %vehicle.getMountNodeObject(0))
		return;

	if($Sim::Time - %player.lastBackwardsTime < 2)
		return;

	%canBackwards = 1;
	if(isObject(%mini = %this.minigame))
		if(%mini.DR_time <= 0 && %mini.DR_time !$= "")
			%canBackwards = 0;

	if(%canBackwards)
	{
		%vehicle.setVelocity("0 0 8");
		%vehicle.setAngularVelocity("0 0 48");
		%player.lastBackwardsTime = $Sim::Time;
	}
}



function serverCmdHelp(%this)
{
	serverCmdDR(%this);
}

function serverCmdDR(%this, %type, %cmd1, %cmd2)
{
	switch$(%type)
	{
		case "gui":
			serverCmdGUI(%this);

		default:
			%this.chatMessage("\c6DeathRace Help \c7- \c6DeathRace code made by Visolator (ID: 48980) \c7- \c6RTV & New voting system added by Clay Hanson [15144]");
			%this.chatMessage("\c3/ToggleHUD \c6- Toggle the centerprint and bottomprint HUD");
			%this.chatMessage("\c3/BackwardsDriving \c6- Must be in a vehicle (and a driver), do a 180 degree");
			%this.chatMessage("\c3/Titles \c6- See your owned titles");
			%this.chatMessage("\c3/TitleShop \c6- See available titles based on your points");
			%this.chatMessage("\c3/Achievements \c6- See most achievements you can unlock, shows unlocked achievements");
			%this.chatMessage("\c3/StereoHelp \c6- See what you can do as a driver, or as a passenger");
	}
}