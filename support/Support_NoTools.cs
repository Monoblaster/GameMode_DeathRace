package ToggleTools
{
	function serverCmddropTool(%client, %toolID)
	{
		if(!%client.player.tool[%toolId].canDrop)
		{
			return;
		}

		%slot = %toolID;
		if(!isObject(%client.minigame))
		{
			Parent::serverCmddropTool(%client, %toolID);
			return;
		}

		if(!isObject(%player = %client.player))
			return;

		messageClient(%client, 'MsgItemPickup', '', %slot, 0);
		%player.tool[%slot] = 0;
		%player.resetWeaponCount();
		serverCmdUnUseTool(%client);
	}
};
activatePackage(ToggleTools);

function Player::resetWeaponCount(%this)
{
	%this.weaponCount = 0;
	%client = %this.client;
	if(!isObject(%client))
	{
		return;
	}
	
	for(%i = 0; %i < %client.getMaxTools(); %i++)
	{
		if(isObject(%item = %this.tool[%i]))
			if(%item.getClassName() $= "ItemData")
				%this.weaponCount++;
	}

	return %this.weaponCount;
}