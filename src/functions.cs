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

function ShapeBase::GetBaseMount(%obj)
{
	%lastMount = %obj;
	while(isObject(%newMount = %lastMount.getObjectMount()))
	{
		%lastMount = %newMount;
	}

	return %lastMount;
}

function ShapeBase::GetTopMount(%obj)
{
	%newMount = %obj;
	%lastMount = %obj;
	while(isObject(%newMount = %newMount.getMountedObject(0)))
	{
		%lastMount = %newMount;
	}

	return %lastMount;
}

function ShapeBase::GetMountedObjects(%obj)
{
	%found = "";
	%unsearched = %obj.getid();
	while(getWordCount(%unsearched) > 0)
	{
		%curr = getWord(%unsearched,0);
		%unsearched = removeWord(%unsearched,0);
		%count = %curr.getMountedObjectCount();
		for(%i = 0; %i < %count; %i++)
		{
			%currMounted = %curr.getMountedObject(%i).getid();
			%unsearched = %unsearched SPC %currMounted;
			%found = %found SPC %currMounted;
		}
		%unsearched = trim(%unsearched);
	}
	return trim(%found);
}

function ShapeBase::MessageClients(%obj,%msg)
{
	%mounted = %obj.getMountedObjects() @ %obj;
	%count = getWordCount(%mounted);
	for(%i = 0; %i < %count; %i++)
	{
		%player = getWord(%mounted,%i);
		%client = %player.client;
		if(isObject(%client))
		{
			%client.chatMessage(%msg);
		}
	}
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