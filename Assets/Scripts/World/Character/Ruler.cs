using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Ruler : Character
{
    public class NoKingdomException : System.Exception { public NoKingdomException(string message) : base(message) { Logger.Error(message); } }

    public Ruler(Name _name, Race _race, int _birthDate = 0, int _age = 0) : base (_name, _race, _birthDate, _age)
    {
        name.preTitle = race.rulerTitle;
    }

    Ruler(Race _race) : base(_race) {}

    public static Ruler CreateRuler()
    {
        var chara = CreateCharacter();
        var r = new Ruler(chara.race);
        r.name = chara.name;
        r.name.preTitle = chara.race.rulerTitle;
        r.age = chara.age;
        r.birthDate = chara.birthDate;

        return r;
    }

    public new class CreationRules : Character.CreationRules
    {
        public int stock = 15;
        public float lifespanToStockRatio = 0.8f;
        public int maxStartingAge;
    }

    public Kingdom GetOwnedKingdom(Map map)
    {
        foreach(var kingdom in map.world.kingdoms) {
            if (kingdom.ruler == this) {
                return kingdom;
            }
        }

        throw new NoKingdomException("The ruler "+name+":"+GetHashCode()+" owns no kingdom! This should not happen");
    }
}
