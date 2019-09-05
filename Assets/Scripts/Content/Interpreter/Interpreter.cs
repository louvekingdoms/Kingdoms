using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using KingdomsSharedCode.Generic;
using static GameLogger;
using System.IO;
using System;
using System.Linq;
using MoonSharp.Interpreter.Loaders;
using KingdomsGame.Interpreter;

public static class Interpreter
{
    static Interpreter()
    {
        UserData.RegisterType<IRuleValue>();

        //TODO: Replace all of those with Custom interpreter wrappers
        // and load them on REQUIRE()

        //UserData.RegisterType<Race.Names>();
    }

    public static Script CreateScript(string baseDirPath)
    {
        Script script = new Script(CoreModules.Preset_HardSandbox | CoreModules.LoadMethods);
        
        script.Options.ScriptLoader = new ScriptLoader(baseDirPath);

        script.Options.DebugPrint = (string o) => { logger.Info("<LUA> "+o); };

        script.Globals["PATH"] = baseDirPath;

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
        script.Globals.Set("INF", DynValue.NewNumber(int.MaxValue));
        script.Globals.Set("NINF", DynValue.NewNumber(int.MinValue));
    }

    static void SetRulesMacros(this Script script)
    {
        var rules = Enum.GetValues(typeof(RULE)).Cast<RULE>();
        foreach(var rule in rules)
        {
            script.Globals.Set("RULE_"+rule.ToString(), DynValue.NewString(rule.ToString()));
        }

        script.Globals["GET_RULE"] = (Func<string, IRuleValue>)((ruleIndex) => {
            return Rules.set[(RULE)Enum.Parse(typeof(RULE), ruleIndex)];
        });
    }

    static void SetFunctions(this Script script)
    {
        script.SetHelperFunctions();
    }

    static void SetHelperFunctions(this Script script)
    {
        script.Globals["DISK_LIST_ELEMENTS"] = (Func<string, string[]>)((url) => {
            var realUrl = Path.Combine(script.Globals.Get("PATH").String , url.RemoveTraversalCharacters());
            return Directory.GetFileSystemEntries(realUrl);
        });
    }
    #endregion

    #region Deserialization
    public static Race LoadRace(string racePath)
    {
        try
        {
            var content = Disk.ReadAllText(racePath);
            var script = CreateScript(Path.GetDirectoryName(racePath));

            var raceTable = script.DoString(content);
               
            return RaceHelper.MakeRaceFrom(raceTable.Table);
        }
        catch (InterpreterException e)
        {
            logger.Error(e.DecoratedMessage);
            throw new Exception(e.DecoratedMessage);
        }
    }

    public static void LoadRegion(string regionPath)
    {
        try
        {
            var content = Disk.ReadAllText(regionPath);
            var script = CreateScript(Path.GetDirectoryName(regionPath));

            var data = script.DoString(content).Table;

            RegionHelper.LoadRegionBehavior(data.Get("behavior").Table);
            RegionHelper.LoadRegionResourceDefinitions(data.Get("resourceDefinitions").Table);
        }
        catch(InterpreterException e)
        {
            logger.Error(e.DecoratedMessage);
            throw new Exception(e.DecoratedMessage);
        }
    }


    public static void LoadTravelers(string travelersPath)
    {
        try
        {
            var content = Disk.ReadAllText(travelersPath);
            var script = CreateScript(Path.GetDirectoryName(travelersPath));

            var data = script.DoString(content).Table;
            
            Library.subjects = data.Get("subjects").Table.MakeSubjectsFrom();
        }
        catch (InterpreterException e)
        {
            logger.Error(e.DecoratedMessage);
            throw new Exception(e.DecoratedMessage);
        }
    }

    #endregion


    public static List<string> ToStringList(Table table)
    {
        var list = new List<string>();
        foreach(var val in table.Values)
        {
            list.Add(val.String);
        }
        return list;
    }
}
