local rulerCreationRules = {}


local function map(characteristics, change, originalCharaName, targetCharaName, threshold)
	local characteristic = characteristics[originalCharaName]
	local target = characteristics[targetCharaName]
	
	if characteristic.GetClampedValue() > threshold + math.min(change, 0) then
		target.SetRaw(target.GetValue() + change)
	end
end

local function reverseMap(characteristics, change, originalCharaName, targetCharaName, threshold)
	local characteristic = characteristics[originalCharaName]
	local target = characteristics[targetCharaName]
	
	if characteristic.GetClampedValue() > threshold + math.min(change, 0) then
		target.SetRaw(target.GetValue() - change)
	end
end

local function genericCharaDef(name, arroganceThreshold)
	local threshold = arroganceThreshold or 5
	local c = _FUNC_NEW_CHARACTERISTIC_DEFINITION()
	
	_FUNC_CHARACTERISTIC_SET_ON_CHANGE(c, 
		function(characteristics, change)
			map(characteristics, change, name, "arrogance", threshold)
		end
	)
	
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
	
	
	local wisdom = _FUNC_NEW_CHARACTERISTIC_DEFINITION()
	
	_FUNC_CHARACTERISTIC_SET_ON_CHANGE(
		wisdom, 
		function(characteristics, change)
			reverseMap(characteristics, change, "wisdom", "arrogance", 0)
		end
	)
	charaDefs["wisdom"] = wisdom 
	
	local arrogance = _FUNC_NEW_CHARACTERISTIC_DEFINITION()
	arrogance.min = 0
	arrogance.max = 10
	arrogance.rules.isFrozen = true
	arrogance.rules.isBad = true
	charaDefs["arrogance"] = arrogance
		
	return charaDefs
end

function rulerCreationRules.initialize(race)

	local rules = race.rulerCreationRules
	
	rules.stock = 15
	rules.lifespanToStockRatio = 0.2
	rules.maxStartingAge = 50
	rules.majority = 15
	rules.maximumLifespan = 60
	
	rules.characteristicDefinitions = getCharacteristicDefinitions()
	
end

return rulerCreationRules