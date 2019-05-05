using System;
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
    class InvalidRelationException : Exception { public InvalidRelationException(string message) : base(message) { } };
    class UnknownFunctionException : Exception { public UnknownFunctionException(string message, IEnumerable cmdList) : base(message+"\nAvailable functions:"+ string.Join("\n", cmdList)) { } };
    
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
                return new Relation(chunk.Substring(0, index - 1), chunk.Substring(index + 1, chunk.Length));
            }

            index++;
        }

        // Error?!
        throw new InvalidRelationException(chunk); 
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
        char expectedEndMarker = '@'; // Dummy
        bool isWaitingForEndMarker = true;
        string currentChunk = string.Empty;

        foreach (char chr in chunk)
        {

            // Inside an ensemble...
            if (isWaitingForEndMarker)
            {
                // Found correct end marker
                if (chr == expectedEndMarker)
                {
                    // Skipping the end marker and resuming read
                    isWaitingForEndMarker = false;
                    continue;
                }
            }

            // Detecting ensemble openings
            bool shouldSkip = false;
            foreach (char[] markers in skipSpaces)
            {
                // Stumbled on an opening marker for an ensemble or a function, we will now wait for the closing marker
                if (chr == markers[0])
                {
                    expectedEndMarker = markers[1];
                    isWaitingForEndMarker = true;
                    shouldSkip = true;
                }
            }
            if (shouldSkip) continue;

            // If we got so far, we're at a correct depth
            if (chr == separator && !isWaitingForEndMarker) {
                // Found separator, emptying chunk and adding it to the list
                chunks.Add(currentChunk);
                currentChunk = string.Empty;
                continue;
            }

            // Building current chunk
            currentChunk += chr;
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

                if (x.concernedCharacteristic.value > over){
                    x.rulerCharacteristics[characteristic].value += x.change;
                }
            }
        },
        {
            "REVERSE_MAP",(CharacteristicDefinitionRuleParameters x) => {
                var characteristic = x.arguments["CHAR"];
                var over = 0;
                if (x.arguments.ContainsKey("OVER")) over = Convert.ToInt32(x.arguments["OVER"]);

                if (x.concernedCharacteristic.value > over){
                    x.rulerCharacteristics[characteristic].value -= x.change;
                }
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
                case "START": def.start = Convert.ToInt32(relation.content); break;
                case "COST": def.cost = Convert.ToInt32(relation.content); break;
                case "RULES": def.rules = ReadCharacteristicDefinitionRules(relation.content, charName); break;
            }
        }

        return def;
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
                    ReadCharacteristicDefinitionRule(relation.content.Truncate(), charName).Invoke(charaSet, change);
                }; break;
            }
        }
        
        return rules;
    }

    // xx(a:b)
    static Action<Ruler.Characteristics, int> ReadCharacteristicDefinitionRule(string chunk, string charName)
    {
        var relation = SeparateRelation(chunk);
        if (!characteristicDefinitionsRules.ContainsKey(relation.key))
        {
            throw new UnknownFunctionException(relation.key, characteristicDefinitionsRules.Keys);
        }

        return (Ruler.Characteristics set, int change) => {
            characteristicDefinitionsRules[relation.key].Invoke(
                new CharacteristicDefinitionRuleParameters()
                {
                    concernedCharacteristic = set[charName],
                    rulerCharacteristics = set,
                    change = change,
                    arguments = ReadContext(relation.content.Truncate())
                }
            );
        };
    }
}
