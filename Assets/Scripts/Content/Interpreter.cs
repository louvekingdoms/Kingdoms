using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using KingdomsSharedCode.Generic;
using Logger = KingdomsSharedCode.Generic.Logger;

public static class Interpreter
{
    public static Script CreateScript()
    {
        UserData.RegisterType<Race>();

        Script script = new Script(CoreModules.Preset_HardSandbox);
        
        script.Options.ScriptLoader = new ScriptLoader()
        {
            ModulePaths = new string[] { "?_module.lua" }
        };
        script.Options.DebugPrint = (string o) => { Logger.Info("<LUA> "+o); };

        script.SetAliases();

        return script;
    }

    static void SetAliases(this Script script)
    {
        script.Globals.Set("YES", DynValue.True);
        script.Globals.Set("NO", DynValue.False);
    }

    #region Deserialization
    public static Race InitializeRace(string raceData)
    {
        var race = new Race();
        var script = CreateScript();

        script.Globals["_RACE"] = race;

        var data = script.DoString(raceData);
        return race;
    }
    #endregion
}
