using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paths
{
    // PATHS
    static readonly string basePath = Application.streamingAssetsPath;
    static readonly string racesPath = basePath+"/Races";
    static readonly string kingdomPath = "Kingdom";
    static readonly string rulerPath = "Ruler";

    // FILES
    static readonly string logFile = "kingdoms.log";
    static readonly string namesFile = "Names.txt";
    static readonly string creationRules = "CreationRules.txt";
    static readonly string gameRules = "GameRules.txt";
    static readonly string metafile = "Info.json";

    // ACCESS
    public static string Races()
    {
        return racesPath;
    }

    public static string RaceMetafile(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/"+ metafile;
    }
    public static string RaceNames(string raceFolderName)
    {
        return racesPath + "/" + raceFolderName;
    }
    public static string RaceKingdomNames(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/" + kingdomPath + "/" + namesFile;
    }
    public static string RaceRulerNames(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/" + rulerPath + "/" + namesFile;
    }
    public static string RaceRulerCreationRules(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/" + rulerPath + "/" + creationRules;
    }
    public static string RaceKingdomGameRules(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/" + kingdomPath + "/" + gameRules;
    }
    public static string LogPath()
    {
        return Application.persistentDataPath + "/" + "logs";
    }
    public static string LogFile()
    {
        return LogPath() + "/" + logFile;
    }
}
