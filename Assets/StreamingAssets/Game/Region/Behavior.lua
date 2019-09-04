

local behavior = {}

local function getStructurePercent(region)
	if not region.owner then 
		return 0.1
	end
	
	return 
		(region.owner.resources["structure"].value - region.owner.resources["structure"].definition.min) /
		(region.owner.resources["structure"].definition.max - region.owner.resources["structure"].definition.min)		
end

local populationGrowthAmplitude = 0.4

-- Events
local function onNewDay(region)

end


local function onNewMonth(region)
	local popIncrease = getStructurePercent(region) * populationGrowthAmplitude
	region.resources["population"].value = region.resources["population"].value + popIncrease
end


local function onNewYear(region)



end


local function onGameStart(region)
	region.resources["population"].value = GET_RULE(RULE_STARTING_POPULATION_PER_REGION).GetInt()
end


function behavior.initialize(region)
	local b = region.behavior
	b.onGameStart = onGameStart
	b.onNewDay = onNewDay
	b.onNewMonth = onNewMonth
	b.onNewYear = onNewYear
end

return behavior