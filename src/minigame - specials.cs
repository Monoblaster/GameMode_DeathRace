// DeathRace file

HorseArmor.canRide = 1;
HorseArmor.maxTools = 8;
HorseArmor.maxItems = 10;

function MinigameSO::DR_SetSpecial(%this, %special, %group)
{
	if(!isObject(%group))
		%group = nameToID("Brickgroup_888888");

	if(!isObject(%group))
		return;

	%rr = %special;
	if(%special $= "random")
		%rr = getRandom(0, 11);

	if(%this.DR_SpecialBought != 0)
		%rr = %this.DR_SpecialBought;

	switch$(%rr)
	{
		case "1" or "CrazySpeed":
			%msg = "Vehicles now have crazy speed!";
			setVehicleSpeed($Pref::Server::DeathRace_Vehicle, getRandom(40, 60));
			%this.DR_crazyspeed = 1;

		case "2" or "ExtraHealth":
			%msg = "100+ more health!";
			%this.DR_doubleHealth = 1;

		case "3" or "VehicleScale":
			%rrr = getRandom(0, 1);
			if(%rrr == 0)
				%this.vehicleScale = 1.25;
			else
				%this.vehicleScale = 1.5;

			%msg = "Vehicles have a " @ %this.vehicleScale @ "x scale! Good luck with that.";

		case "4" or "PlayerScale":
			%rrr = getRandom(0, 2);
			if(%rrr == 0)
				%this.playerScale = 1.25;
			else if(%rrr == 1)
				%this.playerScale = 1.5;
			else
				%this.playerScale = 1.75;

			%msg = "Players have a " @ %this.playerScale @ "x scale! Good luck with that.";

		case "5" or "ExplodeOnCollision":
			%this.DR_EOC = 1;
			%msg = "Vehicles explode on impact! Remember, they do not explode until 5 seconds after the race started.";

		case "6" or "RandomVehicleScale":
			%this.DR_RandomVehicleScale = 1;
			%msg = "Scrambled vehicle scales!";

		case "7" or "Tanks":
			%this.DR_Vehicle = "gc_PumaVehicle";
			%msg = "TANKS! \c4These aren't the default ones..";

		case "8" or "RandomVehicleScaleLoop":
			%this.DR_RandomVehicleScale = 1;
			%this.DR_RandomVehicleScaleLoop = 1;
			%this.DR_ScrambleVehicleScaleLoop();
			%msg = "Scrambled vehicle scales! Vehicle scales will randomize every 10 seconds.";

		case "9" or "Night" or "Dark":
			LoadEnvironmentFromFile($Pref::Server::MapChanger::Path @ "Night.txt");
			%msg = "Night time!";

		case "10" or "empty" or "fist" or "noweapons" or "no weapons":
			%this.noitems = 1;
			%msg = "Haha! How can you fight.. if do not have any weapons!!";

		case "11" or "Horse" or "Horses":
			%this.tempPlayerData = nameToID("HorseArmor");
			%this.playerScale = 0.75;
			%this.DR_Vehicle = 0;
			%this.avoidVehicleDeathCheck = 1;
			%msg = "Uh oh.. The virus has turned people into TINIER HORSES that are the speed of cars and can ride others!";

		default:
			%this.vehicleDamageMult = getRandom(2,3);
			%msg = "Vehicles now receive \c4" @ %this.vehicleDamageMult @ "x \c6more damage!";
	}

	%this.messageAll('MsgAdminForce',"<" @ "font:Palatino Linotype:30" @ ">\c5Special Round\c6! " @ %msg);

	%countVe = %group.NTObjectCount["_car"];
	for(%b=0;%b<%countVe;%b++)
	{
		%brick = %group.NTObject["_car",%b];
		%brick.disappear(-1);
		%brick.reColorVehicle = 1;
		if(!strLen(%brick.DR_BrickColor))
			%brick.DR_BrickColor = %brick.getColorID();

		%brick.schedule(0, setColor, %brick.DR_BrickColor);
		%brick.setVehicle(nameToID(%this.DR_Vehicle));
		if(isObject(%vehicle = %brick.vehicle))
		{
			if(%this.DR_RandomVehicleScale)
				%vehicle.schedule(100, "setScale", getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
			else
				%vehicle.schedule(100, "setScale", vectorScale("1 1 1", %this.vehicleScale));
		}
	}

	%countVeA = %group.NTObjectCount["_admincar"];
	for(%z=0;%z<%countVeA;%z++)
	{
		%brickV = %group.NTObject["_admincar",%z];
		%brick.disappear(-1);
		%brick.reColorVehicle = 1;

		if(!strLen(%brick.DR_BrickColor))
			%brick.DR_BrickColor = %brick.getColorID();

		%brick.schedule(0, setColor, %brick.DR_BrickColor);
		%brick.setVehicle(nameToID(%this.DR_Vehicle));
		if(isObject(%vehicle = %brick.vehicle))
		{
			if(%this.DR_RandomVehicleScale)
				%vehicle.schedule(100, "setScale", getRandomF(0.2, 1.8) SPC getRandomF(0.2, 1.8) SPC getRandomF(1, 1.75));
			else
				%vehicle.schedule(100, "setScale", vectorScale("1 1 1", %this.vehicleScale));
		}
	}

	%this.DR_SpecialBought = 0;
	%this.DR_SpecialBought = 0;
}