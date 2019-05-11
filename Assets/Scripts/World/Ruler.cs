using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Ruler
{
    public readonly string name;
    public readonly Race race;
    public Characteristics characteristics;
    int age = 20;
    int birthDate = 1;
    float health = 100f;

    public Ruler(string _name, Race _race, int _birthDate=0, int _age=0) : this()
    {
        birthDate = _birthDate > 0 ? _birthDate : birthDate;
        age = _age > 0 ? _age : age;
        race = _race;
        name = _name;
        LoadCharacteristicsDefinitions();
    }

    public Ruler()
    {
        race = Library.races[Library.races.Keys.ToArray()[Random.Range(0, Library.races.Keys.Count - 1)]];
        name = race.GetRandomRulerName();

        birthDate = Random.Range(1, Rules.set[RULE.YEAR_LENGTH].GetInt());
        age = Random.Range(
            race.rulerCreationRules.majority,
            Mathf.RoundToInt(Rules.set[RULE.LIFESPAN_MULTIPLIER].GetFloat() * race.rulerCreationRules.maximumLifespan)
        );

        LoadCharacteristicsDefinitions();
    }
    
    void LoadCharacteristicsDefinitions()
    {
        characteristics = new Characteristics();

        foreach(var def in race.rulerCreationRules.characteristicDefinitions.Keys)
        {
            characteristics.Add(def, new Characteristic(race.rulerCreationRules.characteristicDefinitions[def]));
        }
    }

    public int GetAge()
    {
        return age;
    }

    public class CreationRules
    {
        public CharacteristicDefinitions characteristicDefinitions;
        public int maximumLifespan = 60;
        public int majority = 16;
    }

    // Varying characteristic
    public class Characteristic
    {
        int value;
        public readonly CharacteristicDefinition definition;

        public Characteristic(CharacteristicDefinition _definition)
        {
            definition = _definition;
            value = definition.min;
        }

        public void Increase(Characteristics otherChars, int amount=1)
        {
            var oldVal = value;
            value = Mathf.Clamp(value + amount, definition.min, definition.max);
            definition.rules.onChange(otherChars, value - oldVal);
        }

        public void Decrease(Characteristics otherChars, int amount = 1)
        {
            var oldVal = value;
            value = Mathf.Clamp(value - amount, definition.min, definition.max);
            definition.rules.onChange(otherChars, value - oldVal);
        }

        public void SetRaw(int val)
        {
            value = val;
        }

        public int GetValue()
        {
            return value;
        }

        public int GetClampedValue()
        {
            return Mathf.Clamp(value, definition.min, definition.max);
        }
    }

    // "set-in-stone" definition, rules, chara behavior
    public class CharacteristicDefinition
    {
        public int min = 1;
        public int max = 10;
        public int cost = 1;
        public Rules rules;

        public class Rules
        {
            // <current characteristics, concerned chara, amount of change>
            public System.Action<Characteristics, int> onChange;
            public bool isFrozen = false;   // The player cannot change this variable value
            public bool isBad = false; // The characteristic is considered to be a negative trait
        }
    }

    // static set of varying characteristics
    public class Characteristics : Dictionary<string, Characteristic> { }

    // static definitions set
    public class CharacteristicDefinitions : Dictionary<string, CharacteristicDefinition> { }

}
