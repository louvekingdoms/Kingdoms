local names = require("Data/Names")
local rulerCreationRules = require("Character/RulerCreationRules")
local kingdomBehavior = require("Kingdom/Behavior")
local resourceDefinitions = require("Kingdom/Definition")

_RACE.id=0
_RACE.name="Numan"
_RACE.characterNameFormat="{0} of {1}"
_RACE.plural="{0}s"
_RACE.isPlayable=true
_RACE.rulerTitle="Keen {0}"

names.initialize(_RACE)
rulerCreationRules.initialize(_RACE)
resourceDefinitions.initialize(_RACE)
kingdomBehavior.initialize(_RACE)