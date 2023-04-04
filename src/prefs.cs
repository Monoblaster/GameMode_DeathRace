if(!strLen($Pref::Server::VehicleLimitTime)) $Pref::Server::VehicleLimitTime = 60;
if(!strLen($Pref::Server::VehicleLimitTimeDeath)) $Pref::Server::VehicleLimitTimeDeath = 20;
if(!strLen($Pref::Server::VehicleInitTime)) $Pref::Server::VehicleInitTime = 60;
if(!strLen($Pref::Server::DeathRace_Health)) $Pref::Server::DeathRace_Health = -20;
if(!strLen($Pref::Server::DRFont)) $Pref::Server::DRFont = "arial";
if(!strLen($Pref::Server::DeathRace_MaxTeamTime)) $Pref::Server::DeathRace_MaxTeamTime = 8;
if(!strLen($Pref::Server::DeathRace_Vehicle)) $Pref::Server::DeathRace_Vehicle = "NewJeepVehicle";

$Server::MapDebug = 1;
$DeathRace::Profiles = "config/server/DeathRace/Profiles/";

if($Server::DeathRace_Loop $= "")
	$Server::DeathRace_Loop = 150;

if($Server::DeathRace_ShapeDist $= "")
	$Server::DeathRace_ShapeDist = 15;

$Server::DR_Start = 0;
$Server::DeathRace_Luck = 6;
if(!isObject(ItemBrickGroup))
	new SimSet(ItemBrickGroup);