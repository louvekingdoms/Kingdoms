local tblUtil = require("TableUtility")

local behavior = {}

local territory, totalNumberOfRegions

-- Utils
local function getStructureModifier(kingdom)
	return 
		0.5 + 
		(kingdom.resources["structure"].value - kingdom.resources["structure"].definition.min) / (kingdom.resources["structure"].definition.max - kingdom.resources["structure"].definition.min)
end

local function getPercentageOfMapControl()
	return (#territory) / (#totalNumberOfRegions)
end


local function getArmyRegenerationSpeed(kingdom)
	return getStructureModifier(kingdom)*(kingdom.resources["wealth"].value - kingdom.resources["wealth"].definition.min)
end

local function getSumOfAllOwnedRegionsResource(resource)
	local total = 0
	for _,region in pairs(territory) do
		total = total + region.resources[resource].value
	end
	return total
end


-- Events
local function onNewDay(kingdom, date)

	territory = kingdom.getTerritory()
	totalNumberOfRegions = kingdom.getMap().getRegions()

-- test
	kingdom.resources["morale"].value = kingdom.resources["morale"].value+1
	
	-- Population refresh
	kingdom.resources["population"].value = getSumOfAllOwnedRegionsResource("population")
	
	-- Threat refresh
	local threat = 
		kingdom.resources["threat"].definition.min +
		(
			getPercentageOfMapControl() + 
			kingdom.resources["militarization"].value /	kingdom.resources["militarization"].definition.max
		) * (
			kingdom.resources["threat"].definition.max - kingdom.resources["threat"].definition.min
		)
			
	kingdom.resources["threat"].value = threat
	
	--* Morale *
	--* Structure *
	--* Militarization *
end


local function onNewMonth(kingdom, date)
	
	-- 		Gold 
	--[[
		Increases by ( (Total regions wealth value) x  ( wealth  -min_wealth) 
		example : 4.2 x (3 - (-10)) => 4.2 x 13 => 54.6 => 54 (rounded)
	--]]
	local additionalGold = 
		getSumOfAllOwnedRegionsResource("wealth") * 
		(kingdom.resources["wealth"].value - kingdom.resources["wealth"].definition.min) *
		getStructureModifier(kingdom)
		
	kingdom.resources["gold"].value = kingdom.resources["gold"].value + (additionalGold)
	
	--		Army
	local pop = kingdom.resources["population"].value
	local army = kingdom.resources["army"].value
	local armyObjective = pop/kingdom.resources["militarization"].value
	
	if army < armyObjective then 
		kingdom.resources["army"].value = army + math.min(getArmyRegenerationSpeed(kingdom), armyObjective-army)
	end
end


local function onNewYear(kingdom, date)



end


function behavior.initialize(race)
	local b = race.kingdomBehavior
	
	b.onNewDay = onNewDay;
	b.onNewMonth = onNewMonth;
	b.onNewYear = onNewYear;
	
end

return behavior;




