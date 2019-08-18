using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Logger = KingdomsSharedCode.Generic.Logger;

[Serializable]
public class Library
{
    static public Dictionary<int, Race> races = new Dictionary<int, Race>();

    static public void Initialize()
    {
        Logger.Info("Initializing library");
        Clear();
        Load();
    }

    static void Clear()
    {
        races.Clear();
    }

    static void Load()
    {
        LoadRegionBehavior();
        LoadRaces();
    }

    static void LoadRegionBehavior()
    {
        Interpreter.LoadRegion(Paths.RegionDefinitionsFile());
    }

    static void LoadRaces()
    {
        var path = Paths.Races();
        foreach (string dirName in Directory.GetDirectories(path))
        {
            var name = Path.GetFileNameWithoutExtension(dirName);
            LoadRaceFromFolder(name);
        }
    }

    static void LoadRaceFromFolder(string raceFolderName)
    {
        Race race;
        try
        {
            race = Interpreter.LoadRace(Paths.RaceMainFile(raceFolderName));
            Logger.Debug(race.rulerCreationRules.characteristicDefinitions.Count.ToString());
            races.Add(race.id, race);
        }
        catch (MoonSharp.Interpreter.InterpreterException e)
        {
            var msg = "ERROR while loading race data: " + raceFolderName + "\n\n" + e.ToString();
            Logger.Error(msg);
            throw;
        }
    }
}
