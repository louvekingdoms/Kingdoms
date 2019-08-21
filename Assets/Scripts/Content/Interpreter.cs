using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using KingdomsSharedCode.Generic;
using static GameLogger;
using System.IO;
using System;
using System.Linq;

public static class Interpreter
{
    static Interpreter()
    {
        UserData.RegisterType<IRuleValue>();

        UserData.RegisterType<Race>();
        UserData.RegisterType<Race.Names>();
        UserData.RegisterType<Character.CharacteristicDefinition>();
        UserData.RegisterType<Character.CharacteristicDefinition.Rules>();
        UserData.RegisterType<Region>();
        UserData.RegisterType<Region.Behavior>(); 
        UserData.RegisterType<Resource>();
        UserData.RegisterType<Resources>();
        UserData.RegisterType<Resource.Definition>();
        UserData.RegisterType<Kingdom>();
        UserData.RegisterType<Kingdom.Behavior>();
        UserData.RegisterType<Ruler.CreationRules>();
        UserData.RegisterType<Map>();

        UserData.RegisterType<RegionWrapper>();

        #region Translation Tables
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Character.CharacteristicDefinitions),
            (DynValue dynaVal) => {
                var table = dynaVal.Table;
                var defs = new Character.CharacteristicDefinitions();
                foreach (var key in table.Keys)
                {
                    var data = table[key] as Character.CharacteristicDefinition;
                    defs[key.String] = data;
                }
                return defs;
            }
        );

        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Resource.Definitions),
            (DynValue dynaVal) => {
                var table = dynaVal.Table;
                var defs = new Resource.Definitions();
                foreach (var key in table.Keys)
                {
                    var data = table[key] as Resource.Definition;
                    defs[key.String] = data;
                }
                return defs;
            }
        );
        #endregion
    }

    public static Script CreateScript(string basePath)
    {
        Script script = new Script(CoreModules.Preset_HardSandbox | CoreModules.LoadMethods);
        
        script.Options.ScriptLoader = new ScriptLoader(basePath);
        script.Options.DebugPrint = (string o) => { logger.Info("<LUA> "+o); };


        script.SetAliases();
        script.SetRulesMacros();
        script.SetFunctions();

        return script;
    }

    #region Superglobal

    static void SetAliases(this Script script)
    {
        script.Globals.Set("YES", DynValue.True);
        script.Globals.Set("NO", DynValue.False);
        script.Globals.Set("INF", DynValue.NewNumber(double.PositiveInfinity));
        script.Globals.Set("NINF", DynValue.NewNumber(double.NegativeInfinity));
    }

    static void SetRulesMacros(this Script script)
    {
        var rules = Enum.GetValues(typeof(RULE)).Cast<RULE>();
        foreach(var rule in rules)
        {
            script.Globals.Set("_RULE_"+rule.ToString(), DynValue.NewString(rule.ToString()));
        }

        script.Globals["_FUNC_GET_RULE"] = (Func<string, IRuleValue>)((ruleIndex) => {
            return Rules.set[(RULE)Enum.Parse(typeof(RULE), ruleIndex)];
        });
    }

    static void SetFunctions(this Script script)
    {
        script.SetCharacteristicSetFunctions();
        script.SetKingdomBehaviorFunctions();
        script.SetRegionBehaviorFunctions();
    }

    static void SetCharacteristicSetFunctions(this Script script)
    {
        script.Globals["_FUNC_NEW_RESOURCE_DEFINITION"] = (Func<Resource.Definition>)(delegate { return new Resource.Definition(); });
        script.Globals["_FUNC_NEW_CHARACTERISTIC_DEFINITION"] = (Func<Character.CharacteristicDefinition>)(delegate { return new Character.CharacteristicDefinition(); });

        script.Globals["_FUNC_CHARACTERISTIC_SET_ON_CHANGE"] = (Action<Character.CharacteristicDefinition, Closure>)((charDef, action) => {
            charDef.rules.onChange = new Action<Character.Characteristics, int>((characteristics, changeAmount) => {
                action.Call(
                    DynValue.FromObject(script, characteristics),
                    DynValue.NewNumber(changeAmount)
                );
            });
        });
    }

    static void SetKingdomBehaviorFunctions(this Script script)
    {
        script.Globals["_FUNC_KINGDOM_SET_ON_NEW_DAY"] = (Action<Kingdom.Behavior, Closure>)((kingdomBehavior, action) => {
            kingdomBehavior.onNewDay = new Action<Kingdom>((kingdom) => {
                action.Call(
                    DynValue.FromObject(script, kingdom)
                );
            });
        });

        script.Globals["_FUNC_KINGDOM_SET_ON_NEW_MONTH"] = (Action<Kingdom.Behavior, Closure>)((kingdomBehavior, action) => {
            kingdomBehavior.onNewMonth = new Action<Kingdom>((kingdom) => {
                action.Call(
                    DynValue.FromObject(script, kingdom)
                );
            });
        });

        script.Globals["_FUNC_KINGDOM_SET_ON_NEW_YEAR"] = (Action<Kingdom.Behavior, Closure>)((kingdomBehavior, action) => {
            kingdomBehavior.onNewYear = new Action<Kingdom>((kingdom) => {
                action.Call(
                    DynValue.FromObject(script, kingdom)
                );
            });
        });
    }

    static void SetRegionBehaviorFunctions(this Script script)
    {
        script.Globals["_FUNC_REGION_SET_ON_NEW_DAY"] = (Action<Region.Behavior, Closure>)((regionBehavior, action) => {
            regionBehavior.onNewDay = new Action<Region>((region) => {
                action.Call(
                    DynValue.FromObject(script, region)
                );
            });
        });
        script.Globals["_FUNC_REGION_SET_ON_NEW_MONTH"] = (Action<Region.Behavior, Closure>)((regionBehavior, action) => {
            regionBehavior.onNewMonth = new Action<Region>((region) => {
                action.Call(
                    DynValue.FromObject(script, region)
                );
            });
        });
        script.Globals["_FUNC_REGION_SET_ON_NEW_YEAR"] = (Action<Region.Behavior, Closure>)((regionBehavior, action) => {
            regionBehavior.onNewYear = new Action<Region>((region) => {
                action.Call(
                    DynValue.FromObject(script, region)
                );
            });
        });
        script.Globals["_FUNC_REGION_SET_ON_GAME_START"] = (Action<Region.Behavior, Closure>)((regionBehavior, action) => {
            regionBehavior.onGameStart = new Action<Region>((region) => {
                action.Call(
                    DynValue.FromObject(script, region)
                );
            });
        });
    }
    #endregion

    #region Deserialization
    public static Race LoadRace(string racePath)
    {
        var race = new Race();
        var content = Disk.ReadAllText(racePath);
        var script = CreateScript(Path.GetDirectoryName(racePath));

        script.Globals["_RACE"] = race;

        script.DoString(content);

        return race;
    }
    public static void LoadRegion(string regionPath)
    {
        var race = new Race();
        var content = Disk.ReadAllText(regionPath);
        var script = CreateScript(Path.GetDirectoryName(regionPath));

        var wrapper = new RegionWrapper();
        script.Globals["_REGION_WRAPPER"] = wrapper;
        script.DoString(content);

        Region.behavior = wrapper.behavior;
        Region.resourceDefinitions = wrapper.resourceDefinitions;
    }


    #endregion

    #region Wrappers
    class RegionWrapper
    {
        public Region.Behavior behavior = new Region.Behavior();
        public Resource.Definitions resourceDefinitions;
    }

    #endregion
}
