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
    static readonly string mainFile = "Main.lua";
    static readonly string regionFile = "Region.lua";

    // ACCESS
    public static string Races()
    {
        return racesPath;
    }

    public static string RaceMainFile(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/"+ mainFile;
    }
    public static string RaceNames(string raceFolderName)
    {
        return racesPath + "/" + raceFolderName;
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
