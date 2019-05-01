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
    

    public readonly int id = 0;
    public string name = "Default";
    public string adjective = "of {0}";
    public Ruler.CharacteristicDefinitions rulerCreationRules;

    public List<string> kingdomsNames = new List<string>() { "DefaultKingdom" };

    public Race(int _id, string _name, string _adjective)
    {
        id = _id;
        name = _name;
        adjective = _adjective;
    }

    public Race(int _id, string _name, string _adjective, Ruler.CharacteristicDefinitions _rulerCreationRules)
    {
        id = _id;
        name = _name;
        adjective = _adjective;
        rulerCreationRules = _rulerCreationRules;
    }

    public string GetRandomKingdomName()
    {
        return kingdomsNames[Random.Range(0, kingdomsNames.Count)];
    }
}
