/////////////////////////////////////////////////////////
///                                                   //
///	Project Name : Center Print Menu             //
///	Type         : Support Module               //
///	Author       : Clay Hanson (ID: 15144)     //
///	Date         : 7:20 AM 10/4/2016          //
///     Updated	     : 8:59 AM 6/14/2018         //
///	Version      : 1.1                      //
///                                            //
////////////////////////////////////////////////
// Revisioned by Visolator
// Does not support multiple category options - not needed for DeathRace currently

//if($CACHE::CenterPrintMenu::LoadedVersion >= 11) return;

//$CACHE::CenterPrintMenu::LoadedVersion = 11;

if(isPackage(SupportCenterPrintMenu))
	deactivatePackage(SupportCenterPrintMenu);

package SupportCenterPrintMenu
{
	function serverCmdCancelBrick(%client)
	{
		if(!%client.CPM_UsingMenu) { Parent::serverCmdCancelBrick(%client); return; }
		
		if($CACHE::CenterPrintMenu::CanBeClosed)
			%client.displayCenterPrintMenu("", "stop");
	}
	
	function serverCmdPlantBrick(%client)
	{
		if(!%client.CPM_UsingMenu) { Parent::serverCmdPlantBrick(%client); return; }
		
		%func = %client.CPM_DataCall;
		if(isFunction(GameConnection, %func))
			%client.call(%func, %client.CPM_SelectedID);
		
		if($CACHE::CenterPrintMenu::CanBeClosedAfterDone) %client.displayCenterPrintMenu("", "stop");
		else %client.updateCenterPrintMenu();
	}
	
	function serverCmdShiftBrick(%client, %x, %y, %z)
	{
		if(!%client.CPM_UsingMenu) { Parent::serverCmdShiftBrick(%client, %x, %y, %z); return; }
		
		if(%x != 0)
		{
			%client.CPM_SelectedID = %client.CPM_SelectedID - %x;
			
			if(%client.CPM_SelectedID >= getFieldCount(%client.CPM_Data)) %client.CPM_SelectedID = 0;
			if(%client.CPM_SelectedID <= -1) %client.CPM_SelectedID = getFieldCount(%client.CPM_Data)-1;
			
			%client.updateCenterPrintMenu();
		}

		if(%y == -1) // Right - Select
		{
			%func = %client.CPM_DataCall;
			if(isFunction(GameConnection, %func))
				%client.call(%func, %client.CPM_SelectedID);
			
			if($CACHE::CenterPrintMenu::CanBeClosedAfterDone) %client.displayCenterPrintMenu("", "stop");
			else %client.updateCenterPrintMenu();
		}
	}
	
	function serverCmdSuperShiftBrick(%client, %x, %y, %z)
	{
		if(!%client.CPM_UsingMenu) { Parent::serverCmdSuperShiftBrick(%client, %x, %y, %z); return; }
		if(%x != 0)
		{
			%client.CPM_SelectedID = %client.CPM_SelectedID - %x;
			
			if(%client.CPM_SelectedID >= getFieldCount(%client.CPM_Data)) %client.CPM_SelectedID = 0;
			if(%client.CPM_SelectedID <= -1) %client.CPM_SelectedID = getFieldCount(%client.CPM_Data)-1;
			%client.updateCenterPrintMenu();
		}

		if(%y == -1) // Right - Select
		{
			%func = %client.CPM_DataCall;
			if(isFunction(GameConnection, %func))
				%client.call(%func, %client.CPM_SelectedID);
			
			if($CACHE::CenterPrintMenu::CanBeClosedAfterDone) %client.displayCenterPrintMenu("", "stop");
			else %client.updateCenterPrintMenu();
		}
	}
};
activatePackage(SupportCenterPrintMenu);

function GameConnection::updateCenterPrintMenu(%client)
{
	%client.displayCenterPrintMenu();
}

// Add %title if using for later reference
// Purposely made 3 options because I don't feel like making this fancier nor the time to do so
function GameConnection::displayCenterPrintMenu(%this, %data, %func)
{
	if(%data $= "stop") 
	{
		clearCenterPrint(%this);
		%this.CPM_UsingMenu = 0;
		%this.CPM_Data = "";
		%this.CPM_SelectedID = 0;
		%this.CPM_DataCall = "";
		return;
	}
	
	if(%data $= "")
	{
		if(%this.CPM_Data $= "") return;
		%data = %this.CPM_Data;
	}
	else
	{
		%this.CPM_DataCall = %func;

		%this.CPM_UsingMenu = 1;
		%this.CPM_Data = %data;
		%this.CPM_SelectedID = 0;
	}
	
	%options = "<" @ "font:Verdana:23" @ ">\c6Deathrace Maps (Use build keys)\r\n";
	%data = %this.CPM_Data;
	%dataFields = getFieldCount(%data);

	%lineCount = 3;
	if(%this.CPM_SelectedID > 1 && %dataFields > 3)
	{
		%options = %options NL "\c4^^ ^ ^^";
	}
	
	%start = (%dataFields > 4 && %this.CPM_SelectedID != 0 ? %this.CPM_SelectedID-1 : 0);
	
	%count = %start + %lineCount;
	for(%i = %start; %i <= %count; %i++)
	{
		%tempData = getField(%data, %i);
		
		%options = %options NL (%this.CPM_SelectedID == %i ? "<" @ "div:1" @ ">" : "") @ "\c6" @ %tempData;
	}

	if(%count < %dataFields)
		%options = %options NL "\c4vv v vv";
	
	%options = trim(%options);
	centerPrint(%this, %options);
}