$Server::PrePlayerEmitterVersion = 2;

if($Server::PlayerEmitter)
	if($Server::PlayerEmitterVersion > $Server::PrePlayerEmitterVersion)
		return;

package MountEmitters
{
	function Armor::onRemove(%this, %obj)
	{
		if(isFunction(%obj.getClassName(), DeleteEmitters))
			%obj.DeleteEmitters();
		return Parent::onRemove(%this, %obj);
	}
};
activatePackage(MountEmitters);

$Server::PlayerEmitter = 1;
$Server::PlayerEmitterVersion = $Server::PrePlayerEmitterVersion;
//Particle attaching support
function ParticleEmitterNode::enterAttachLoop(%this, %obj)
{
   	//this is working off the assumption that you wont enter the emitter in more than once
   	//we make sure the object exists when we point to the object from the emitter
   	//while also making sure that the currently attached object exists
   	if(%this.attached != %obj)
      	%this.attached = %obj;

   	if(!isObject(%this.attached))
   	{
   		%this.delete();
   		return;
   	}

   	//setTransform moves the emitter to the position of the object
   	%this.setTransform(vectorAdd(%obj.position, %this.offset) @ " 1 0 0 0");

   	if(isObject(%client = %obj.client) && getWordCount(%col = %client.chestColor) == 4)
   		%this.setColor(%col);
   	else if(getWordCount(%col = %obj.chestColor) == 4)
   		%this.setColor(%col);

   	//this updates the emitters position, this is the part I'd imagine would be hard to find
   	%this.inspectPostApply();

   	//we repeat the function in 150 milliseconds, creating the loop
   	%this.schedule(50, "enterAttachLoop", %obj);
}

function ShapeBase::mountEmitter(%this, %data, %offset, %time)
{
	if(!isObject(%data)) return -1;
	if(%this.getClassName() !$= "AIPlayer" && %this.getClassName() !$= "Player") return;
	%data = nameToID(%data);
	for(%i = 0;%i < 10;%i ++)
	{
		if(isObject(%this.emitter[%i]))
		{
			if(%this.emitter[%i].emitter == %data)
				return;

			%count++;
		}
		else
		{
			%slot = %i;
			break;
		}
	}
	if(%slot $= "")
		return; //Broken crap

	%scale = %this.getScale();
	%this.emitterCount = %count;
	%emitter = new ParticleEmitterNode()
	{
		dataBlock = GenericEmitterNode;
		position = %this.getPosition();
	   	scale = %scale;
	   	rotation = "1 0 0 0";
	   	velocity = 1;
	   	spherePlacement = 0;
	   	offset = %offset;
	};
	if(!isObject(pEmitterGroup))
		new SimSet(pEmitterGroup);
	pEmitterGroup.add(%emitter);

	%this.emitter[(%this.emitterCount++)-1] = %emitter;
	%emitter.setEmitterDatablock(%data);
	%emitter.enterAttachLoop(%this);
	if(%time > 0) %this.schedule(%time * 1000, unMountEmitter, %slot);
}
registerOutputEvent(Player,"mountEmitter","datablock ParticleEmitterData" TAB "vector 50" TAB "int 0 9999 5");
registerOutputEvent(Bot,"mountEmitter","datablock ParticleEmitterData" TAB "vector 50" TAB "int 0 9999 5");

function ShapeBase::unMountEmitter(%this,%slot)
{
	if(%this.getClassName() !$= "AIPlayer" && %this.getClassName() !$= "Player") return;
	if(isObject(%em=%this.emitter[%slot]) && %slot >= 0 && %slot <= 10)
	{
		%this.emitter[%slot] = 0;
		%em.delete();
	}
}
registerOutputEvent(Player,"unMountEmitter","int 0 10 5");
registerOutputEvent(Bot,"unMountEmitter","int 0 10 5");

function ShapeBase::isMountedEmitter(%this, %emitterDatablock)
{
	if(%this.getClassName() !$= "AIPlayer" && %this.getClassName() !$= "Player")
		return 0;
	
	for(%i = 0;%i < 10;%i ++)
	{
		if(isObject(%this.emitter[%i]))
		{
			if(%this.emitter[%i].emitter == %emitterDatablock)
				return 1;
		}
	}

	return 0;
}

function ShapeBase::getMountedEmitter(%this, %slot)
{
	if(%this.getClassName() !$= "AIPlayer" && %this.getClassName() !$= "Player") return;
	if(!isObject(%this.emitter[%slot])) return -1;
	return %this.emitter[%slot].emitter;
}

function ShapeBase::DumpEmitters(%this)
{
	if(%this.getClassName() !$= "AIPlayer" && %this.getClassName() !$= "Player") return;
	for(%i=0;%i<10;%i++)
	{
		if(!isObject(%this.emitter[%i])) %table = %table @ "\c4<nothing> " @ "\c7" @ %i @ " | ";
		else %table = %table @ "\c5" @ %this.emitter[%i] @ "\c7" @ %i @ " | ";
	}
	return %table;
}

function ShapeBase::DeleteEmitters(%this)
{
	if(%this.getClassName() !$= "AIPlayer" && %this.getClassName() !$= "Player") return;
	%this.emitterCount = 0;
	for(%i=0;%i<10;%i++)
	{
		if(isObject(%em=%this.emitter[%i]))
			%em.delete();
		%this.emitter[%i] = 0;
	}
}
registerOutputEvent(Player,"DeleteEmitters");
registerOutputEvent(Bot,"DeleteEmitters");