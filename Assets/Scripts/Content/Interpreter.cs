using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Interpreter
{
    class InvalidRelationException : Exception { public InvalidRelationException(string message) : base(message) {  } };

    static char relationMarker = ':';
    static char separator = ',';
    static char[] ensembleMarkers = new char[] { '[', ']' };
    static char[] functionMarkers = new char[] { '(', ')' };

    public class Relation
    {
        public string key;
        public string content;
        public Relation(string _k, string _content)
        {
            key = _k;
            content = _content;
        }
    }

    public static string Sanitize(string chunk)
    {
        return chunk.Replace(" ", "").Replace("\n", "").Replace("\r", "") ;
    }

    public static Relation SeparateRelation(string chunk)
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

    public static List<string> ExplodeChunk(string chunk, char[][] skipSpaces)
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
        
    }

    public static Ruler.CharacteristicDefinitions ReadCharacteristicDefinitions(string definitionsChunk)
    {
        var saneChunk = definitionsChunk.Substring(1, definitionsChunk.Length - 2);
        foreach (var chunk in ExplodeChunk(saneChunk, new char[][] { ensembleMarkers, functionMarkers }))
        {

        }
    }

    public static Ruler.CharacteristicDefinition ReadCharacteristicDefinition (string definition)
    {

    }
}
