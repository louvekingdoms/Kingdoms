using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paths
{
    // PATHS
    static readonly string basePath = Application.streamingAssetsPath;
    static readonly string racesPath = basePath+"/Races";
    static readonly string namesPath = "Names";
    static readonly string rulerPath = "Ruler";

    // FILES
    static readonly string logFile = "kingdoms.log";
    static readonly string kingdomNamesFile = "Kingdoms.txt";
    static readonly string rulerNamesFile = "Rulers.txt";
    static readonly string creationRules = "CreationRules.txt";
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
        return RaceNames(raceFolderName) + "/" + namesPath + "/" + kingdomNamesFile;
    }
    public static string RaceRulerNames(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/" + namesPath + "/" + rulerNamesFile;
    }
    public static string RaceRulerCreationRules(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/" + rulerPath + "/" + creationRules;
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
