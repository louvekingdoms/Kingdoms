using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Race
{
    // Used for disk operations
    public class Info
    {
        public int id;
        public string name;
        public string adjective;
    }

    public override string ToString()
    {
        return "[RACE:"+name+"]";
    }

    public readonly int id = 0;

    public string name = "Default";
    public string adjective = "of {0}";
    public Ruler.CreationRules rulerCreationRules;

    public List<string> kingdomNames = new List<string>() { "DefaultKingdom" };
    public List<string> rulerNames = new List<string>() { "DefaultRuler" };

    public Race(int _id, string _name, string _adjective)
    {
        id = _id;
        name = _name;
        adjective = _adjective;
    }

    public Race(int _id, string _name, string _adjective, Ruler.CreationRules _rulerCreationRules)
    {
        id = _id;
        name = _name;
        adjective = _adjective;
        rulerCreationRules = _rulerCreationRules;
    }

    public string GetRandomKingdomName()
    {
        return kingdomNames[Random.Range(0, kingdomNames.Count)];
    }

    public string GetRandomRulerName()
    {
        return rulerNames[Random.Range(0, rulerNames.Count)];
    }
}
