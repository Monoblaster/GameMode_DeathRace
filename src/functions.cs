// DeathRace file

function Map_Debug(%msg)
{
	if($Server::MapDebug)
		announce("MapDebug - " @ %msg);
}

function NewJeepSmokeCheck(){} //NewJeepVehicle - For some reason doesn't exist.

function vectorFloatLength(%vec, %l)
{
	%l = mClampF(%l, 0, 50);
	return mFloatLength(getWord(%vec, 0), %l) SPC mFloatLength(getWord(%vec, 1), %l) SPC mFloatLength(getWord(%vec, 2), %l);
}

function getRandomF(%lim0, %lim1)
{
    %diff = %lim1 - %lim0;
    return getRandom() * %diff + %lim0;
}

function findclosestcolor(%x)
{
	%x = getColorF(%x);
	for(%a=0; %a<64; %a++)
	{
		%match = mabs(getword(getcoloridtable(%a),0) - getword(%x,0)) + 
			mabs(getword(getcoloridtable(%a),1) - getword(%x,1)) + mabs(getword(getcoloridtable(%a),2) - getword(%x,2))
			+ mabs(getword(getcoloridtable(%a),3) - getword(%x,3));

		if(%match < %bestmatch || %bestmatch $= "")
		{
			%bestmatch = %match;
			%bestid = %a;
		}
	}
	return %bestid;
}

function SimObject::onCameraEnterOrbit(%obj, %camera)
{
	if(isObject(%client = %camera.getControllingClient()) && %client.getClassName() $= "GameConnection" && isObject(%obj.client))
	{
		%client.spyObj = %obj;
	}
}

function SimObject::onCameraLeaveOrbit(%obj, %camera)
{
	if(isObject(%client = %camera.getControllingClient()) && %client.getClassName() $= "GameConnection")
	{
		%client.spyObj = 0;
	}
}