function Inventory_Create()
{
	return new scriptObject()
	{
		class = "Inventory";
	};
}

function Inventory::Set(%inv,%slot,%db)
{
	%inv.tool[%slot] = %db;

	return %inv;
}

function Inventory::Get(%inv,%slot)
{
	return %inv.tool[%slot];
}

function Inventory::Display(%inv,%client,%writeBlank,%offset)
{
	if(!isObject(%client) || %client.getClassName() !$= "GameConnection")
	{
		return %inv;
	}

	for(%i = 0; %i < 20; %i++)
	{
		%tool = %inv.tool[%i + %offset];
		if(!isObject(%tool) && !%writeBlank)
		{
			continue;
		}

		if(isObject(%tool))
		{
			%tool = %tool.getId();
		}

		messageClient(%client,'MsgItemPickup',"",%i,%tool,1);
	}
}