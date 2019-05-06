using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class Disk
{
    static public string ReadAllText(string filePath)
    {
        return string.Join("\n", ReadAllLines(filePath));
    }

    static public List<string> ReadAllLines(string filePath)
    {
        return new List<string>(File.ReadAllLines(filePath));
    }
}
