using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace KingdomsGame.Interpreter
{
    public static class KingdomHelper
    {
        public static Table Serialize(this Kingdom kingdom, Script script)
        {
            var table = new Table(script);

            table.Set("color", DynValue.NewString(kingdom.color.ToHex()));
            table.Set("demonym", DynValue.NewString(kingdom.demonym));
            table.Set("name", DynValue.NewString(kingdom.name));
            table.Set("mainland", DynValue.NewNumber(kingdom.mainland));

            // Resources
            table.Set("resources", DynValue.NewTable(kingdom.resources.Serialize(script)));

            // Getters
            table.Set("getTerritory", new LuaFunction(
                (context) => {
                    return DynValue.NewTable(kingdom.GetTerritory().Serialize(script));
                }
            ));
            table.Set("getMap", new LuaFunction(
                (context) => {
                    return DynValue.NewTable(kingdom.map.Serialize(script));
                }
            ));

            // Setters

            return table;
        }

        public static void LoadValuesFromTable(this Kingdom kingdom, Table table)
        {
            kingdom.color = new Color(table.Get("color").String);
            kingdom.demonym = table.Get("demonym").String;
            kingdom.mainland = (int)table.Get("mainland").Number;
            kingdom.name = table.Get("name").String;

            kingdom.resources.LoadValuesFromTable(table.Get("resources").Table);

        }

        public static void LoadFromTable(this Kingdom.Behavior behavior, Table table)
        {
            // Events in lua actually have an additional parameter to each event - "date", 
            // which is "today's date" in form of a table object

            behavior.onNewDay = new Action<Kingdom>((kingdom) =>
            {
                var tKingdom = kingdom.Serialize(table.OwnerScript);
                table.Get("onNewDay").Function.Call(
                    tKingdom,
                    DateHelper.Serialize(Game.clock.GetDate(), table.OwnerScript)
                );
                kingdom.LoadValuesFromTable(tKingdom);
            });

            behavior.onNewMonth = new Action<Kingdom>((kingdom) =>
            {
                var tKingdom = kingdom.Serialize(table.OwnerScript);
                table.Get("onNewMonth").Function.Call(
                    tKingdom,
                    DateHelper.Serialize(Game.clock.GetDate(), table.OwnerScript)
                );
                kingdom.LoadValuesFromTable(tKingdom);
            });

            behavior.onNewYear = new Action<Kingdom>((kingdom) =>
            {
                var tKingdom = kingdom.Serialize(table.OwnerScript);
                table.Get("onNewYear").Function.Call(
                    tKingdom,
                    DateHelper.Serialize(Game.clock.GetDate(), table.OwnerScript)
                );
                kingdom.LoadValuesFromTable(tKingdom);
            });

        }


    }
}