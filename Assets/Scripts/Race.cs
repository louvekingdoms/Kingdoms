using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Race
{
    public override string ToString()
    {
        return "[RACE:"+name+"]";
    }

    public int id = 0;
    public string name;
    public string plural = "{0}s";
    public string characterNameFormat = "{0} {1}";
    public string rulerTitle;
    public bool isPlayable = true;
    public Ruler.CreationRules rulerCreationRules;
    public Kingdom.Rules kingdomRules;
    public Names names = new Names();
    
    public string GetRandomKingdomName()
    {
        return names.kingdoms.RandomElement();
    }

    public Character.Name GetRandomCharacterName()
    {
        var first = names.first.RandomElement();
        var family = names.family.RandomElement();
        return new Character.Name(first, family, this);
    }

    public string GetRandomHeroName()
    {
        //FIXME
        return string.Empty;
    }

    public class Names {
        public List<string> kingdoms = new List<string>();
        public List<string> first = new List<string>();
        public List<string> family = new List<string>();
    }

    public string GetPlural()
    {
        return string.Format(plural, name);
    }
}
