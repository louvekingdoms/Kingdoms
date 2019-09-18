local definition = {}

local newResource = require("Default/ResourceDefinition")

function definition.initialize(race)

	local def = {};
	
	-- Accumulated resources
	local population = newResource()
	population.min = 0
	population.max = INF
	population.isMutable = false
	
	local gold = newResource()
	gold.min = 0
	gold.max = INF
	gold.noModifiers = true
	
	local army = newResource()
	army.min = 0
	army.max = INF
	army.noModifiers = true
	
	-- Dynamic resources
	local wealth = newResource()
	wealth.min = 0
	wealth.max = INF
	wealth.isMutable = false
	
	local morale = newResource()
	morale.min = -10
	morale.max = 10
	morale.start = 0
	
	local structure = newResource()
	structure.min = -10
	structure.max = 10
	structure.start = 0
	
	local militarization = newResource()
	militarization.min = 0
	militarization.max = 100
	militarization.start = 5
	
	local reputation = newResource()
	reputation.min = -10
	reputation.max = 10
	reputation.start = 0
	
	local threat = newResource()
	threat.min = -10
	threat.max = 10
	threat.start = 0
		
	def["population"] = population
	def["gold"] = gold
	def["army"] = army
	def["wealth"] = wealth
	def["morale"] = morale
	def["structure"] = structure
	def["militarization"] = militarization
	def["reputation"] = reputation
	def["threat"] = threat
	
	race.resourceDefinitions = def
end

return definition