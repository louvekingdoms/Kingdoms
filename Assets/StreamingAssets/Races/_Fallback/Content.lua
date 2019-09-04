local strUtil = require("StringUtility")

local allowedExtensions = {".png", ".jpg"}
local content = {}

function content.load(race)
	loadPortraits(race)
	loadPawn(race)
end

function loadPortraits(race)
	local list = DISK_LIST_ELEMENTS("Data/Pictures/Faces")
	for _, v in pairs(list) do
		for _, ext in pairs(allowedExtensions) do
			if strUtil.endsWith(v, ext) then 
				table.insert(race.portraits, v)
			end
		end
	end
end

function loadPawn(race)
	race.pawnImage = PATH.."/Data/Pictures/Pawn_32x32.png"
end

return content