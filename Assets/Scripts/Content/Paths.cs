using System.Collections;
using System.Collections.Generic;

public class Paths
{
    // PATHS
    static readonly string basePath = UnityEngine.Application.streamingAssetsPath;
    static readonly string racesPath = basePath+"/Races";
    static readonly string gamePath = basePath + "/Game";
    static readonly string kingdomPath = "Kingdom";
    static readonly string characterPath = "Character";

    // FILES
    static readonly string logFile = "kingdoms.log";
    static readonly string namesFile = "Names.lua";
    static readonly string rulerCreationRules = "RulerCreationRules.lua";
    static readonly string behavior = "Behavior.lua";
    static readonly string metafile = "Info.lua";
    static readonly string regionFile = "Region.lua";

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
    public static string RaceCharacterNames(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/" + characterPath + "/" + namesFile;
    }
    public static string RaceRulerCreationRules(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/" + characterPath + "/" + rulerCreationRules;
    }
    public static string RaceKingdomGameBehavior(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/" + kingdomPath + "/" + behavior;
    }
    public static string RegionDefinitionsFile()
    {
        return gamePath + "/" + regionFile;
    }
    public static string LogPath()
    {
        return UnityEngine.Application.persistentDataPath + "/" + "logs";
    }
    public static string LogFile()
    {
        return LogPath() + "/" + logFile;
    }
}
