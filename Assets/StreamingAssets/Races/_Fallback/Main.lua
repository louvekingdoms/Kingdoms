local names = require("Data/Names")
local rulerCreationRules = require("Character/RulerCreationRules")
local kingdomBehavior = require("Kingdom/Behavior")
local resourceDefinitions = require("Kingdom/Definition")
local baseInfo = require("BaseInfo")
local content = require("Content")

local race = require("Default/Race")

baseInfo.load(race)
content.load(race)

names.initialize(race)
rulerCreationRules.initialize(race)

kingdomBehavior.initialize(race)
resourceDefinitions.initialize(race)

return race

--TODO : List of traveler types allowed for this race
