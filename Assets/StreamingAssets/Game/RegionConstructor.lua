
-- Initializes standard behavior and resource definitions for every region that will ever be created by the game

local definition = require("Region/Definition")
local behavior = require("Region/Behavior")

local regionConstructor = require("Default/RegionGlobalParameters")()

definition.initialize(regionConstructor)
behavior.initialize(regionConstructor)

return regionConstructor;