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
    static readonly string kingdomsNamesFile = "Kingdoms.txt";
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
    public static string RaceKingdomsNames(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/" + namesPath + "/" + kingdomsNamesFile;
    }
    public static string RaceRulerCreationRules(string raceFolderName)
    {
        return RaceNames(raceFolderName) + "/" + rulerPath + "/" + creationRules;
    }
}
