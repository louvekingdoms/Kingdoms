using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Race
{
    public override string ToString()
    {
        return "[RACE:{0}.{1}]".Format(name, id);
    }

    public int id = 0;
    public string name;
    public string plural = "{0}s";
    public string characterNameFormat = "{0} {1}";
    public string rulerTitle;
    public bool isPlayable = true;
    public Ruler.CreationRules rulerCreationRules;
    public Kingdom.Behavior kingdomBehavior;
    public Names names = new Names();
    
    public string GetRandomKingdomName()
    {
        return names.kingdoms.PickRandom(Game.state.random);
    }

    public Character.Name GetRandomCharacterName()
    {
        var first = names.first.PickRandom(Game.state.random);
        var family = names.family.PickRandom(Game.state.random);
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
