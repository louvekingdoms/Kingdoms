local baseInfo = {}

function baseInfo.load(race)
	race.id=0
	race.name="Numan"
	race.characterNameFormat="{0} of {1}"
	race.plural="{0}s"
	race.isPlayable=true
	race.rulerTitle="Keen {0}"
end

return baseInfo