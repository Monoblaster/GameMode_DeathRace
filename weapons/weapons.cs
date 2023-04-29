//lets you make t+t weapons using objects fields to generate the required datablocks and defining weapon behaviour
$Weapons::Count = 0;

//creates datablock and deletes the used scriptObject
function Weapons_Create(%data)
{
	%name = "Weapons_" @ $Weapons::Count;
	%command = ""
	@	"datablock ItemData( " @ %name @ "Item )"
	@	"{"
	@	"	className = %data.itemClass;"
	@	"	image = %name @ \"Image\";"

	@	"	shapeFile = %data.shapeFile;"
	@	"	iconName = %data.iconName;"
	@	"	doColorShift = %data.doColorShift;"
	@	"	colorShiftColor = %data.colorShiftColor;"
	@	"	emap = true;"

	@	"	mass = %data.mass;"
	@	"	density = %data.density;"
	@	"	elasticity = %data.elasticity;"
	@	"	friction = %data.friction;"

	@	"	uiName = %data.uiName;"
	@	"	canDrop = true;"
	@	"};"

	@	"datablock ShapeBaseImageData( " @ %name @ "Image )"
	@	"{"
	@	"	className = %data.imageClass;"
	@	"	item = %name @ \"Item\";"

	@	"	shapeFile = %data.shapeFile;"
	@	"	doColorShift = %data.doColorShift;"
	@	"	colorShiftColor = %data.colorShiftColor;"
	@	"	emap = true;"

	@	"	mountPoint = 0;"
	@	"	offset = %data.offset;"
	@	"	eyeOffset = %data.eyeOffset;"
	@	"	rotation = eulerToMatrix(%data.rotation);"
	@	"};";
	eval(%command);
	$Weapons::Item[%data.uiName] = "Weapons_" @ $Weapons::Count @ "Item";
	$Weapons::Image[%data.uiName] = "Weapons_" @ $Weapons::Count @ "Image";
	$Weapons::Name[$Weapons::Count] = %data.uiName;

	$Weapons::Count++;
	%data.delete();
}

//state script
//equip cooldown > wait for fire
//wait for fire > ammo check
//ammo check > (no ammo > do dry fire) (ammo > do fire)
//

function Weapons_findItemByName(%name)
{
	return $Weapons::Item[%name];
}

function Weapons_findItemByID(%num)
{
	return $Weapons::Item[$Weapons::Name[%num]];
}


function WeaponData::onAdd(%o)
{
	%o.shapeFile = expandFilename(%o.shapeFile);
	%o.iconName = expandFilename(%o.iconName);
}

$currGun = new ScriptObject()
{
	class = "WeaponData";

	itemClass = "Weapon";
	imageClass = "WeaponImage";

	shapeFile = "./assets/pistol_.dts";
	iconName = "./assets/pistol";
	doColorShift = true;
	colorShiftColor = "0.73 0.73 0.73 1.000";

	fireSound = "./Pistol_fire.1.wav";
	emptySound = "./Pistol_click.wav";

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;

	uiName = "Pistol";
};
Weapons_Create($currGun);
