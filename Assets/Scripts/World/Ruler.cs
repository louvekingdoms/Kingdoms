using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ruler
{
    string name;
    Race race;
    Characteristics characteristics;
    
    public class CreationRules
    {
        public CharacteristicDefinitions characteristicDefinitions;
    }

    // Varying characteristic
    public class Characteristic
    {
        public int value;
        public readonly CharacteristicDefinition definition;

        public Characteristic(CharacteristicDefinition _definition)
        {
            definition = _definition;
            value = definition.start;
        }
    }

    // "set-in-stone" definition, rules, chara behavior
    public class CharacteristicDefinition
    {
        public int min = 1;
        public int max = 10;
        public int start = 1;
        public int cost = 1;
        public Rules rules;

        public class Rules
        {
            // <current characteristics, concerned chara, amount of change>
            public System.Action<Characteristics, int> onChange;
            public bool isFrozen = false;   // The player cannot change this variable value
        }
    }

    // static set of varying characteristics
    public class Characteristics : Dictionary<string, Characteristic> { }

    // static definitions set
    public class CharacteristicDefinitions : Dictionary<string, CharacteristicDefinition> { }

}
