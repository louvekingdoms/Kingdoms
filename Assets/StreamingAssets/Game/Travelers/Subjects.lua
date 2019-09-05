local strUtil = require("StringUtility")
local newSubject = require("Default/Subject")

local subjects = {}

local subjectFolders = DISK_LIST_ELEMENTS("Travelers/Subjects")

for i, v in pairs(subjectFolders) do
	if not strUtil.endsWith(v, ".meta") then 
		local subject = newSubject()
		subject.image = v.."/Icon_32x32.png"
		subject.id = i
		table.insert(subjects, subject)
	end
end

return subjects