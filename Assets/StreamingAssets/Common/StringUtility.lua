local strUtility = {}

function strUtility.endsWith(str, ending)
   return ending == "" or str:sub(-#ending) == ending
end

return strUtility