using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    public enum TYPE { HERO, RULER };
    public readonly Race race;
    public Characteristics characteristics;
    public Name name;
    public int age = 20;
    int birthDate = 1;
    float health = 100f;

    public Character(Name _name, Race _race, int _birthDate = 0, int _age = 0) : this()
    {
        birthDate = _birthDate > 0 ? _birthDate : birthDate;
        age = _age > 0 ? _age : age;
        race = _race;
        name = _name;
        LoadCharacteristicsDefinitions();
    }

    public Character()
    {
        race = Library.races[Library.races.Keys.ToArray()[Random.Range(0, Library.races.Keys.Count - 1)]];
        
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

    public class Name {
        string firstName;
        string lastName;
        string nameFormat = "{0} {1}";
        string preTitle = "";
        string postTitle = "";

        public Name(string firstName, string lastName, Race race)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.nameFormat = race.nameFormat;
        }

        public void SetPreTitle(string preTitle)
        {
            this.preTitle = preTitle;
        }

        public void SetPostTitle(string postTitle)
        {
            this.postTitle = postTitle;
        }

        public string GetFullName()
        {
            return preTitle+" "+string.Format(nameFormat, firstName, lastName)+" "+postTitle;
        }

        public string GetShortName()
        {
            return string.Format(nameFormat, firstName, lastName);
        }
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
