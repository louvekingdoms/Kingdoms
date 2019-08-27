local behavior = {}


-- Utils
local function getStructureModifier(kingdom)
	return 
		0.5 + 
		(kingdom.resources.GetValue("structure") - kingdom.resources.GetDefinition("structure").min) / (kingdom.resources.GetDefinition("structure").max - kingdom.resources.GetDefinition("structure").min)
end

local function getPercentageOfMapControl(kingdom)
	return (#kingdom.GetTerritory()) / (#kingdom.map.regions)
end


local function getArmyRegenerationSpeed(kingdom)
	return getStructureModifier(kingdom)*(kingdom.resources.GetValue("wealth") - kingdom.resources.GetDefinition("wealth").min)
end

local function getSumOfAllOwnedRegionsResource(kingdom, resource)
	local territory = kingdom.GetTerritory()
	local total = 0
	for _,region in pairs(territory) do
		total = total + region.resources.GetValue(resource)
	end
	return total
end


-- Events
local function onNewDay(kingdom)

	kingdom.resources.SetRaw("morale", kingdom.resources.GetValue("morale")+1)
	
	-- Population refresh
	kingdom.resources.SetRaw("population", getSumOfAllOwnedRegionsResource(kingdom, "population"))
	
	-- Threat refresh
	local threat = 
		kingdom.resources.GetDefinition("threat").min +
		(
			getPercentageOfMapControl(kingdom) + kingdom.resources.GetValue("militarization") /
			kingdom.resources.GetDefinition("militarization").max
		) * (
			kingdom.resources.GetDefinition("threat").max - kingdom.resources.GetDefinition("threat").min
		) 
		
	kingdom.resources.SetRaw("threat", threat)
	
	--* Morale *
	--* Structure *
	--* Militarization *
end


local function onNewMonth(kingdom)
	
	-- 		Gold 
	--[[
		Increases by ( (Total regions wealth value) x  ( wealth  -min_wealth) 
		example : 4.2 x (3 - (-10)) => 4.2 x 13 => 54.6 => 54 (rounded)
	--]]
	local additionalGold = 
		getSumOfAllOwnedRegionsResource(kingdom, "wealth") * 
		(kingdom.resources.GetValue("wealth") - kingdom.resources.GetDefinition("wealth").min) *
		getStructureModifier(kingdom)
		
	kingdom.resources["gold"].Increase(additionalGold)
	
	--		Army
	local pop = kingdom.resources.GetValue("population")
	local army = kingdom.resources.GetValue("army")
	local armyObjective = pop/kingdom.resources.GetValue("militarization")
	
	if army < armyObjective then 
		kingdom.resources["gold"].Increase(math.min(getArmyRegenerationSpeed(kingdom), armyObjective-army))
	end
end


local function onNewYear(kingdom)



end


function behavior.initialize(race)
	local b = race.kingdomBehavior
	
	KINGDOM_SET_ON_NEW_DAY(b, onNewDay)
	KINGDOM_SET_ON_NEW_MONTH(b, onNewMonth)
	KINGDOM_SET_ON_NEW_YEAR(b, onNewYear)
end

return behavior;




