using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
        LoadRaces();
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

        //////////////////////
        ///
        ///     META INFO LOADING
        ///
        try
        {
            var meta = JsonUtility.FromJson(Disk.ReadAllText(Paths.RaceMetafile(raceFolderName)), typeof(Race.Info)) as Race.Info;
            race = new Race(meta.id, meta.name, meta.plural);
            races.Add(race.id, race);
        }
        catch (System.Exception e)
        {
            Debug.LogError("ERROR WHILE LOADING A RACE META FILE : " + raceFolderName + "\n\n" + e.ToString());
            throw;
        }


        ///
        ///     NAMES
        ///
        try
        {
            race.kingdomNames = new List<string>(Disk.ReadAllLines(Paths.RaceKingdomNames(raceFolderName)));
            race.rulerNames = new List<string>(Disk.ReadAllLines(Paths.RaceRulerNames(raceFolderName)));
        }
        catch(System.Exception e)
        {
            Debug.LogError("ERROR WHILE LOADING RACE NAMES : " + raceFolderName + "\n\n" + e.ToString());
            throw;
        }

        //
        //  CREATION RULES
        //
        try
        {
            var creationRules = Disk.ReadAllText(Paths.RaceRulerCreationRules(raceFolderName));
            race.rulerCreationRules = Interpreter.ReadRulerCreationRules(creationRules);
        }
        catch (System.Exception e)
        {
            Debug.LogError("ERROR WHILE LOADING RACE RULER CREATION RULES : " + raceFolderName + "\n\n" + e.ToString());
            throw;
        }
    }
}
