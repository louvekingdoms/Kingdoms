using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Library
{
    static public Dictionary<int, Race> races = new Dictionary<int, Race>();

    static public void Load()
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
            var meta = JsonUtility.FromJson(File.ReadAllText(Paths.RaceMetafile(raceFolderName)), typeof(Race.Info)) as Race.Info;
            race = new Race(meta.id, meta.name, meta.adjective);
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
            var kingdoms = new List<string>(File.ReadAllLines(Paths.RaceKingdomsNames(raceFolderName)));
            race.kingdomsNames = kingdoms;
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
            var creationRules = new List<string>(File.ReadAllLines(Paths.RaceRulerCreationRules(raceFolderName)));
            race.kingdomsNames = kingdoms;
        }
        catch (System.Exception e)
        {
            Debug.LogError("ERROR WHILE LOADING RACE RULER CREATION RULES : " + raceFolderName + "\n\n" + e.ToString());
            throw;
        }
    }
}
