using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using static Character;

namespace KingdomsGame.Interpreter
{
    public static class CharacteristicsHelper
    {

        public static CharacteristicDefinitions MakeCharacteristicDefinitionsFrom(this Table table)
        {

            var characteristicDefinitions = new CharacteristicDefinitions();
            foreach (var charaDef in table.Pairs)
            {
                var key = charaDef.Key.String;
                var val = charaDef.Value.Table;

                var def = new CharacteristicDefinition();

                def.cost = (int)val.Get("cost").Number;
                def.max = (int)val.Get("max").Number;
                def.min = (int)val.Get("min").Number;

                var rules = val.Get("rules").Table;
                def.rules = new CharacteristicDefinition.Rules();

                def.rules.isBad = rules.Get("isBad").Boolean;
                def.rules.isFrozen = rules.Get("isFrozen").Boolean;
                def.rules.onChange = new Action<Characteristics, int>((characteristics, changeAmount) =>
                {
                    var tCharas = characteristics.Serialize(table.OwnerScript);
                    rules.Get("onChange").Function.Call(
                        tCharas,
                        changeAmount
                    );

                    characteristics.LoadValuesFromTable(tCharas);
                });
            }

            return characteristicDefinitions;
        }

        public static Table Serialize(this Characteristics characteristics, Script script)
        {
            var table = new Table(script);
            foreach (var key in characteristics.Keys)
            {
                var chara = characteristics[key];
                var charaTable = new Table(script);
                charaTable.Set(DynValue.NewString("rawValue"), DynValue.NewNumber(chara.GetValue()));
                charaTable.Set(DynValue.NewString("clampedValue"), DynValue.NewNumber(chara.GetClampedValue()));

                table.Set(
                    DynValue.NewString(key),
                    DynValue.NewTable(charaTable)
                );
            }
            return table;
        }

        public static void LoadValuesFromTable(this Characteristics characteristics, Table luaCharacteristics)
        {
            var defs = new Characteristics();
            foreach (var key in luaCharacteristics.Keys)
            {
                var data = luaCharacteristics.Get(key).Table;
                var chara = defs[key.String];
                chara.SetRaw((int)data.Get("rawValue").Number);
            }
        }
    }
}