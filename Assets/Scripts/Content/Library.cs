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
        LoadRegionBehavior();
        LoadRaces();
    }

    static void LoadRegionBehavior()
    {
        Region.behavior = Interpreter.ReadRegionBehavior(Disk.ReadAllText(Paths.RegionDefinitionsFile()));
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
        ///     META AND CULTURAL INFO LOADING
        ///
        try
        {
            var meta = Disk.ReadAllText(Paths.RaceMetafile(raceFolderName));
            race = Interpreter.ReadRaceInfo(meta);
            races.Add(race.id, race);
        }
        catch (System.Exception e)
        {
            var msg = "ERROR WHILE LOADING A RACE META FILE : " + raceFolderName + "\n\n" + e.ToString();
            Logger.Error(msg);
            Debug.LogError(msg);
            throw;
        }


        ///
        ///     NAMES
        ///
        try
        {
            race = Interpreter.LoadRaceNames(race, Disk.ReadAllText(Paths.RaceKingdomNames(raceFolderName)));
            race = Interpreter.LoadRaceNames(race, Disk.ReadAllText(Paths.RaceCharacterNames(raceFolderName)));
        }
        catch(System.Exception e) {
            var msg = "ERROR WHILE LOADING RACE NAMES : " + raceFolderName + "\n\n" + e.ToString();
            Logger.Error(msg);
            Debug.LogError(msg);
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
        catch (System.Exception e) {
            var msg = "ERROR WHILE LOADING RACE CREATION RULES : " + raceFolderName + "\n\n" + e.ToString();
            Logger.Error(msg);
            Debug.LogError(msg);
            throw;
        }

        ///
        /// KINGDOM RULES
        ///
        try {
            var textRules = Disk.ReadAllText(Paths.RaceKingdomGameBehavior(raceFolderName));
            race.kingdomBehavior = Interpreter.ReadKingdomBehavior(textRules);
        }
        catch (System.Exception e) {
            Debug.LogError("ERROR WHILE LOADING RACE KINGDOM CREATION RULES : " + raceFolderName + "\n\n" + e.ToString());
            throw;
        }

    }
}
