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
        public int max = 5;
        public int start = 1;
        public int cost = 1;

        public class Rule
        {
            public System.Action<Characteristics> onOverflow;
            public System.Action<Characteristics> onIncrement;
            public System.Action<Characteristics> onDecrement;
        }
    }

    // static set of varying characteristics
    public class Characteristics : List<Characteristic> { }

    // static definitions set
    public class CharacteristicDefinitions : Dictionary<string, CharacteristicDefinition> { }

}
