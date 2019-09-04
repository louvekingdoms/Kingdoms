using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace KingdomsGame.Interpreter
{
    public static class RaceHelper
    {
        public static Race MakeRaceFrom(this Table table)
        {
            var race = new Race();
            race.id = (int)table.Get("id").Number;
            race.name = table.Get("name").String;
            race.characterNameFormat = table.Get("characterNameFormat").String;
            race.plural = table.Get("plural").String;
            race.isPlayable = table.Get("isPlayable").Boolean;

            // Names
            race.names = new Race.Names();
            race.names.family = new List<string>(table.Get("names").Table.Get("family").Table.Values.Select(o => o.String).ToArray());
            race.names.first = new List<string>(table.Get("names").Table.Get("first").Table.Values.Select(o => o.String).ToArray());
            race.names.kingdoms = new List<string>(table.Get("names").Table.Get("kingdoms").Table.Values.Select(o => o.String).ToArray());

            // Images
            foreach (var portraitUrl in table.Get("portraits").Table.Values)
            {
                race.portraits.Add(new Image(portraitUrl.String));
            }
            race.pawn = new Image(table.Get("pawnImage").String);

            // Ruler rules
            var rules = table.Get("rulerCreationRules").Table;
            race.rulerCreationRules = new Ruler.CreationRules()
            {
                lifespanToStockRatio = (float)rules.Get("lifespanToStockRatio").Number,
                majority = (int)rules.Get("majority").Number,
                maximumLifespan = (int)rules.Get("maximumLifespan").Number,
                maxStartingAge = (int)rules.Get("maxStartingAge").Number,
                stock = (int)rules.Get("stock").Number
            };

            race.rulerCreationRules.characteristicDefinitions = CharacteristicsHelper.MakeCharacteristicDefinitionsFrom(rules.Get("characteristicDefinitions").Table);

            var behavior = new Kingdom.Behavior();
            behavior.LoadFromTable(table.Get("kingdomBehavior").Table);
            race.kingdomBehavior = behavior;

            race.resourceDefinitions = new Resource.Definitions();
            race.resourceDefinitions.LoadFromTable(table.Get("resourceDefinitions").Table);

            //TODO: Implement
            //race.genders = table.Get("genders").Number;

            return race;
        }
    }
}