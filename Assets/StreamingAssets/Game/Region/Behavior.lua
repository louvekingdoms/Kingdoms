local behavior = {}

local function getStructurePercent(region)
	if not region.owner then 
		return 0.1
	end
	
	return 
		(region.owner.resources.Get("structure") - region.owner.resources.GetDefinition("structure").min) /
		(region.owner.resources.GetDefinition("structure").max - region.owner.resources.GetDefinition("structure").min)		
end

local populationGrowthAmplitude = 0.4

-- Events
local function onNewDay(region)

end


local function onNewMonth(region)
	local popIncrease = getStructurePercent(region) * populationGrowthAmplitude
	region.resources["population"].Increase(popIncrease)
end


local function onNewYear(region)



end


local function onGameStart(region)
	region.resources.SetRaw("population", _FUNC_GET_RULE(_RULE_STARTING_POPULATION_PER_REGION).GetInt())
end


function behavior.initialize(region)

	local b = region.behavior
	_FUNC_REGION_SET_ON_GAME_START(b, onGameStart)
	_FUNC_REGION_SET_ON_NEW_DAY(b, onNewDay)
	_FUNC_REGION_SET_ON_NEW_MONTH(b, onNewMonth)
	_FUNC_REGION_SET_ON_NEW_YEAR(b, onNewYear)		
end

return behavior