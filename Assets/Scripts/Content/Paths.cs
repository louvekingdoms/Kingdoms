using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Paths
{
    // PATHS
    static readonly string basePath = UnityEngine.Application.streamingAssetsPath;
    static readonly string gamePath = Path.Combine(basePath, @"Game");
    static readonly string commonPath = Path.Combine(basePath, @"Common");

    static readonly string racesPath = Path.Combine(basePath, @"Races");

    // FILES
    static readonly string logFile = @"kingdoms.log";
    static readonly string mainFile = @"Main.lua";
    static readonly string regionFile = @"RegionConstructor.lua";

    // ACCESS
    public static string Races()
    {
        return racesPath;
    }

    public static string RaceMainFile(string raceFolderName)
    {
        return Path.Combine(RaceNames(raceFolderName), mainFile);
    }
    public static string RaceNames(string raceFolderName)
    {
        return Path.Combine(racesPath, raceFolderName);
    }
    public static string RegionDefinitionsFile()
    {
        return Path.Combine(gamePath, regionFile);
    }
    public static string LogPath()
    {
        return Path.Combine(UnityEngine.Application.persistentDataPath, "logs");
    }
    public static string LogFile()
    {
        return Path.Combine(LogPath(), logFile);
    }

    public static string CommonLuaPath()
    {
        return commonPath;
    }
}
