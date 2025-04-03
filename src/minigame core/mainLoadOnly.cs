package CustomMinigameCore_LoadOnce
{
	function ServerLoadSaveFile_End()
   	{
      	Parent::ServerLoadSaveFile_End();
      	if(isFunction(MapSys_onMapChanged))
      		MapSys_onMapChanged();
   	}

	function miniGameCanDamage(%client, %victimObject)
	{
		if(isObject(%client) && (%canDamage = %client.Mini_CanDamage(%victimObject)) != -1)
			return %canDamage;

		return Parent::miniGameCanDamage(%client, %victimObject);
	}
};
activatePackage("CustomMinigameCore_LoadOnce");