local rulerCreationRules = {}


local function map(characteristics, change, originalCharaName, targetCharaName, threshold)
	local characteristic = characteristics[originalCharaName]
	local target = characteristics[targetCharaName]
	
	if characteristic.clampedValue > threshold + math.min(change, 0) then
		target.rawValue = target.rawValue + change
	end
end

local function reverseMap(characteristics, change, originalCharaName, targetCharaName, threshold)
	local characteristic = characteristics[originalCharaName]
	local target = characteristics[targetCharaName]
	
	if characteristic.clampedValue > threshold + math.min(change, 0) then
		target.rawValue = (target.clampedValue - change)
	end
end

local function genericCharaDef(name, arroganceThreshold)
	local threshold = arroganceThreshold or 5
	local c = require("Default/CharacteristicDefinition")()
	c.onChange = function(characteristics, change)
		map(characteristics, change, name, "arrogance", threshold)
	end
	
	return c	
end

local function getCharacteristicDefinitions()
	
	charaDefs = {}
	
	charaDefs["command"] = genericCharaDef("command")
	charaDefs["martial"] = genericCharaDef("martial")
	charaDefs["diplomacy"] = genericCharaDef("diplomacy")
	charaDefs["administration"] = genericCharaDef("administration")
	charaDefs["charisma"] = genericCharaDef("charisma")
	charaDefs["luck"] = genericCharaDef("luck")
	
	
	local wisdom = require("Default/CharacteristicDefinition")()
	wisdom.onChange = function(characteristics, change)
		reverseMap(characteristics, change, "wisdom", "arrogance", 0)
	end
	
	charaDefs["wisdom"] = wisdom 
	
	local arrogance = require("Default/CharacteristicDefinition")()
	arrogance.min = 0
	arrogance.max = 10
	arrogance.rules.isFrozen = true
	arrogance.rules.isBad = true
	charaDefs["arrogance"] = arrogance
		
	return charaDefs
end

function rulerCreationRules.initialize(race)

	local rules = require("Default/RulerCreationRules")()
	
	rules.characteristicDefinitions = getCharacteristicDefinitions()
	
	race.rulerCreationRules = rules
	
end

return rulerCreationRules