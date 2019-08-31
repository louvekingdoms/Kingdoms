local names = require("Data/Names")
local rulerCreationRules = require("Character/RulerCreationRules")
local kingdomBehavior = require("Kingdom/Behavior")
local resourceDefinitions = require("Kingdom/Definition")
local baseInfo = require("BaseInfo")
local content = require("Content")

baseInfo.load(_RACE)
content.load(_RACE)

names.initialize(_RACE)
rulerCreationRules.initialize(_RACE)
resourceDefinitions.initialize(_RACE)
kingdomBehavior.initialize(_RACE)

--TODO : List of traveler types
