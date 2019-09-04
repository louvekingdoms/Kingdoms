local strUtil = require("StringUtility")

local data = {}

data.subjects = {}

local subjectFolders = DISK_LIST_ELEMENTS("Subjects")

for _, v in pairs(subjectFolders) do
	if not strUtil.endsWith(v, ".meta") then 
		
		
		
	end
end

return data