local definition = {}

function definition.initialize(race)

	local def = {};
	
	-- Accumulated resources
	local population = _FUNC_NEW_RESOURCE_DEFINITION()
	population.min = 0
	population.max = INF
	population.isMutable = false
	
	local gold = _FUNC_NEW_RESOURCE_DEFINITION()
	gold.min = 0
	gold.max = INF
	gold.noModifiers = true
	
	local army = _FUNC_NEW_RESOURCE_DEFINITION()
	army.min = 0
	army.max = INF
	army.noModifiers = true
	
	-- Dynamic resources
	local wealth = _FUNC_NEW_RESOURCE_DEFINITION()
	wealth.min = -10
	wealth.max = 10
	wealth.start = 0
	
	local morale = _FUNC_NEW_RESOURCE_DEFINITION()
	morale.min = -10
	morale.max = 10
	morale.start = 0
	
	local structure = _FUNC_NEW_RESOURCE_DEFINITION()
	structure.min = -10
	structure.max = 10
	structure.start = 0
	
	local militarization = _FUNC_NEW_RESOURCE_DEFINITION()
	militarization.min = 0
	militarization.max = 100
	militarization.start = 5
	
	local reputation = _FUNC_NEW_RESOURCE_DEFINITION()
	reputation.min = -10
	reputation.max = 10
	reputation.start = 0
	
	local threat = _FUNC_NEW_RESOURCE_DEFINITION()
	threat.min = -10
	threat.max = 10
	threat.start = 0
		
	def["population"] = population
	def["gold"] = gold
	def["army"] = army
	def["wealth"] = wealth
	def["morale"] = morale
	def["structure"] = structure
	def["militarization"] = population
	def["reputation"] = reputation
	def["threat"] = threat
	
	race.resourceDefinitions = def
end

return definition