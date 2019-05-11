using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Interpreter
{

    #region SETTINGS
    static char relationMarker = ':';
    static char separator = ',';
    static char[] ensembleMarkers = new char[] { '[', ']' };
    static char[] functionMarkers = new char[] { '(', ')' };
    #endregion

    #region UTILITIES
    class UnsolvedDepthException : Exception { public UnsolvedDepthException(string message) : base(message) { } };
    class InvalidRelationException : Exception { public InvalidRelationException(string message) : base(message) { } };
    class UnknownFunctionException : Exception { public UnknownFunctionException(string message, List<string> cmdList) : base(message+"\nAvailable functions:"+ string.Join("\n", cmdList)) { } };
    class InvalidBoolException : Exception { public InvalidBoolException(string message) : base(message) { } };

    class Context : Dictionary<string, string> {}
    class Relation
    {
        public string key;
        public string content;
        public Relation(string _k, string _content)
        {
            key = _k;
            content = _content;
        }
    }

    static string Sanitize(this string chunk)
    {
        return chunk.Replace(" ", "").Replace("\n", "").Replace("\r", "") ;
    }

    static string Truncate(this string ensemble)
    {
        return ensemble.Substring(1, ensemble.Length - 2);
    }

    static Relation SeparateRelation(string chunk)
    {
        int index = 0;
        foreach (char chr in chunk)
        {
            if (chr == relationMarker)
            {
                var key = chunk.Substring(0, index);
                var content = chunk.Substring(index + 1, chunk.Length-(index+1));
                return new Relation(key, content);
            }

            index++;
        }

        // Error?!
        throw new InvalidRelationException(chunk); 
    }

    static Relation SeparateFunctionCall(string chunk)
    {
        int index = 0;
        int start = 0;
        int end = 0;
        foreach (char chr in chunk)
        {
            if (chr == functionMarkers[0])
            {
                start = index;
            }
            if (start > 0 && chr == functionMarkers[1])
            {
                end = index;
            }

            index++;
        }
        if (start == end || start == 0 || end == 0)
        {
            throw new InvalidRelationException(chunk);
        }

        var key = chunk.Substring(0, start);
        var content = chunk.Substring(start + 1, chunk.Length - (start + 2));
        return new Relation(key, content);
        /*
            */
    }
    
    static Context ReadContext(this string chunk)
    {
        var context = new Context();
        foreach(var block in chunk.ExplodeChunk(new char[][] {ensembleMarkers, functionMarkers }))
        {
            var relation = SeparateRelation(block);
            context[relation.key] = relation.content;
        }
        return context;
    }

    static List<string> ExplodeChunk(this string chunk, char[][] skipSpaces)
    {
        List<string> chunks = new List<string>();
        List<char> expectedEndMarkers = new List<char>();
        string currentChunk = string.Empty;

        for (int i = 0; i < chunk.Length; i++)
        {
            var chr = chunk[i];
            // Inside an ensemble...
            if (expectedEndMarkers.Count > 0) 
            {
                // Found correct end marker
                if (chr == expectedEndMarkers[expectedEndMarkers.Count-1])
                {
                    // Skipping the end marker and resuming read
                    expectedEndMarkers.RemoveAt(expectedEndMarkers.Count - 1);
                }
            }

            // Detecting ensemble openings
            foreach (char[] markers in skipSpaces)
            {
                // Stumbled on an opening marker for an ensemble or a function, we will now wait for the closing marker
                if (chr == markers[0])
                {
                    expectedEndMarkers.Add(markers[1]);
                }
            }

            // If we're at a correct depth
            if ((i == chunk.Length-1 || chr == separator) && expectedEndMarkers.Count == 0) {
                if (i == chunk.Length - 1)
                {
                    // EOF
                    currentChunk += chr;
                }
                // Found separator, emptying chunk and adding it to the list
                chunks.Add(currentChunk);
                currentChunk = string.Empty;
                continue;
            }

            // Building current chunk
            currentChunk += chr;
        }

        if (currentChunk.Length > 0)
        {
            throw new UnsolvedDepthException("Out of depth! Are you closing all your ensembles?\nExceeding data:\n" + currentChunk);
        }
        return chunks;
    }
    #endregion

    #region COMMANDS

    #region CREATION RULES

    class CharacteristicDefinitionRuleParameters
    {
        public Ruler.Characteristic concernedCharacteristic;
        public Ruler.Characteristics rulerCharacteristics;
        public Context arguments = new Context();
        public int change = 0;
    }
    static Dictionary<string, Action<CharacteristicDefinitionRuleParameters>> characteristicDefinitionsRules = new Dictionary<string, Action<CharacteristicDefinitionRuleParameters>>()
    {
        {
            "MAP",(CharacteristicDefinitionRuleParameters x) => {
                var characteristic = x.arguments["CHAR"];
                var over = 0;
                if (x.arguments.ContainsKey("OVER")) over = Convert.ToInt32(x.arguments["OVER"]);

                if (x.concernedCharacteristic.GetClampedValue() > over + Mathf.Min(x.change, 0)){
                    x.rulerCharacteristics[characteristic].SetRaw(x.rulerCharacteristics[characteristic].GetValue() + x.change);
                }
            }
        },
        {
            "REVERSE_MAP",(CharacteristicDefinitionRuleParameters x) => {
                var characteristic = x.arguments["CHAR"];
                var over = 0;
                if (x.arguments.ContainsKey("OVER")) over = Convert.ToInt32(x.arguments["OVER"]);

                if (x.concernedCharacteristic.GetClampedValue() > over + Mathf.Min(x.change, 0)){
                    x.rulerCharacteristics[characteristic].SetRaw(x.rulerCharacteristics[characteristic].GetValue() - x.change);
                }
            }
        },
        {
            "FREEZE",(CharacteristicDefinitionRuleParameters x) => {
                x.concernedCharacteristic.SetRaw(x.concernedCharacteristic.GetValue() - x.change);
            }
        }
    };

    #endregion

    #endregion

    public static Ruler.CreationRules ReadRulerCreationRules(string creationRules)
    {
        var rules = new Ruler.CreationRules();
        string chunk = Sanitize(creationRules);
        var chunks = ExplodeChunk(chunk, new char[][] { ensembleMarkers, functionMarkers });
        
        foreach (var definition in chunks) {
            var relation = SeparateRelation(definition);
            switch (relation.key)
            {
                case "CHARACTERISTICS":
                    rules.characteristicDefinitions = ReadCharacteristicDefinitions(relation.content);
                    break;
            }
        }

        return rules;
    }

    // charisma: [xxxx], martial [xxxxx]
    static Ruler.CharacteristicDefinitions ReadCharacteristicDefinitions(string definitionsChunk)
    {
        var definitions = new Ruler.CharacteristicDefinitions();
        var saneChunk = definitionsChunk.Truncate();
        foreach (var chunk in ExplodeChunk(saneChunk, new char[][] { ensembleMarkers, functionMarkers }))
        {
            var relation = SeparateRelation(chunk);
            definitions.Add(relation.key, ReadCharacteristicDefinition(relation.content, relation.key));
        }
        return definitions;
    }

    // [RULES:[xx], MIN:xx, MAX:xx]
    static Ruler.CharacteristicDefinition ReadCharacteristicDefinition (string definition, string charName)
    {
        var def = new Ruler.CharacteristicDefinition();
        var saneChunk = definition.Truncate();
        var exploded = ExplodeChunk(saneChunk, new char[][] { ensembleMarkers, functionMarkers });
        foreach (var setting in exploded) {
            var relation = SeparateRelation(setting);
            switch (relation.key.ToUpper()) {

                case "MIN": def.min = Convert.ToInt32(relation.content); break;

                case "MAX": def.max = Convert.ToInt32(relation.content); break;

                case "COST": def.cost = Convert.ToInt32(relation.content); break;

                case "RULES":
                    def.rules = ReadCharacteristicDefinitionRules(relation.content, charName);
                    break;

            }
        }

        return def;
    }

    static bool StringToBool(string chunk)
    {
        switch (chunk.ToUpper())
        {
            case "YES":
            case "TRUE":
            case "1":
                return true;

            case "NO":
            case "FALSE":
            case "0":
                return false;
        }
        throw new InvalidBoolException(chunk + " is not a valid bool");
    }

    // RULES:[ON_OVER_HALF: xx(xxx), ON_CHANGE: xx(xxx)]
    static Ruler.CharacteristicDefinition.Rules ReadCharacteristicDefinitionRules (string chunk, string charName)
    {
        var rules = new Ruler.CharacteristicDefinition.Rules();
        var exploded = ExplodeChunk(chunk.Truncate(), new char[][] { ensembleMarkers, functionMarkers });
        foreach (var setting in exploded)
        {
            var relation = SeparateRelation(setting);
            switch (relation.key.ToUpper()){
                case "ON_CHANGE": rules.onChange += (Ruler.Characteristics charaSet, int change) => {
                    ReadCharacteristicDefinitionRule(relation.content, charName).Invoke(charaSet, change);
                }; break;

                case "IS_BAD":
                    rules.isBad = StringToBool(relation.content);
                    break;

                case "IS_FROZEN":
                    rules.isFrozen = StringToBool(relation.content);
                    break;
            }
        }
        
        return rules;
    }

    // xx(a:b)
    static Action<Ruler.Characteristics, int> ReadCharacteristicDefinitionRule(string chunk, string charName)
    {
        var relation = SeparateFunctionCall(chunk);
        if (!characteristicDefinitionsRules.ContainsKey(relation.key))
        {
            throw new UnknownFunctionException(relation.key, characteristicDefinitionsRules.Keys.ToList());
        }

        return (Ruler.Characteristics set, int change) => {
            characteristicDefinitionsRules[relation.key].Invoke(
                new CharacteristicDefinitionRuleParameters()
                {
                    concernedCharacteristic = set[charName],
                    rulerCharacteristics = set,
                    change = change,
                    arguments = ReadContext(relation.content)
                }
            );
        };
    }
}
