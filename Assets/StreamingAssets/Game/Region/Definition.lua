local definition = {}

function definition.initialize(region)
	local def = {};
	
	-- Accumulated resources
	local population = NEW_RESOURCE_DEFINITION()
	population.min = 0
	population.max = INF
	
	-- Dynamic resources
	local wealth = NEW_RESOURCE_DEFINITION()
	wealth.min = -10
	wealth.max = 10
	wealth.start = 0
	
	local civilization = NEW_RESOURCE_DEFINITION()
	civilization.min = 0
	civilization.max = 100
	civilization.start = 0
	
	local defense = NEW_RESOURCE_DEFINITION()
	defense.min = 0
	defense.max = 100
	defense.start = 0
		
	def["population"] = population
	def["wealth"] = wealth
	def["civilization"] = civilization
	def["defense"] = defense
	
	region.resourceDefinitions = def
end

return definition