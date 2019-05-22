using System;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public static class Interpreter
{

    #region SUPERGLOBALS
    static Context context = new Context();
    static Helpers<float> numberHelpers = new Helpers<float>();
    #endregion

    #region SETTINGS
    static char relationMarker = ':';
    static char separator = ',';
    static char commentMarker = '*';
    static char[] ensembleMarkers = new char[] { '[', ']' };
    static char[] functionMarkers = new char[] { '(', ')' };
    static string[] litteralStringMarkers = new string[] { "<<", ">>"};
    #endregion

    #region UTILITIES
    class UnsolvedDepthException : Exception { public UnsolvedDepthException(string message) : base(message) { } };
    class InvalidRelationException : Exception { public InvalidRelationException(string message) : base(message) { } };
    class UnknownFunctionException : Exception { public UnknownFunctionException(string message, List<string> cmdList) : base(message + "\nAvailable functions:" + string.Join("\n", cmdList)) { } };
    class UnknownKeyException : Exception { public UnknownKeyException(string message, List<string> cmdList) : base(message + "\nAvailable keys:" + string.Join("\n", cmdList)) { } };
    class InvalidBoolException : Exception { public InvalidBoolException(string message) : base(message) { } };
    class MissingContextElementException : Exception { public MissingContextElementException(string message) : base(message) { } };

    class Context : Dictionary<string, string> {
        public static Context Merge(Context a, Context b)
        {
            var ctx = a;
            if (a == null) return b;
            if (b == null) return a;

            foreach(var element in b.Keys) {
                ctx[element] = b[element];
            }

            return ctx;
        }
    }

    class Helpers<T> : Dictionary<string, Func<T>> { }

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

    class NamedAction<T>
    {
        public string key;
        public Action<T, string> effect;
        public NamedAction(string key, Action<T, string> effect)
        {
            this.key = key;
            this.effect = effect;
        }
    }

    class ActionTable<T> : Dictionary<string, Action<T, string>> {
        public ActionTable(params NamedAction<T>[] actions)
        {
            foreach(var action in actions) {
                Add(action.key, action.effect);
            }
        }
    }

    class NamedFunction<T>
    {
        public string key;
        public Func<Context, T> effect;
        public NamedFunction(string key, Func<Context, T> effect)
        {
            this.key = key;
            this.effect = effect;
        }
    }

    class FunctionTable<T> : Dictionary<string, Func<Context, T>>
    {
        public FunctionTable(params NamedFunction<T>[] functions)
        {
            foreach (var function in functions) {
                Add(function.key, function.effect);
            }
        }
    }

    static void print(object a) { Debug.Log(a); }
    static void echo(object a) { print(a); }
    static void show_debug_message(object a) { print(a); }
    static void log(object a) { print(a); }

    static void Require(this Context ctx, params string[] requirements)
    {
        foreach(var str in requirements) {
            if (!ctx.ContainsKey(str)) {
                throw new MissingContextElementException(str);
            }
        }
    }

    static string Sanitize(this string chunk)
    {
        //Regex regex = new Regex(@"[\s](?=[^" + litteralStringMarkers[1]+ "]*?(?:"+ litteralStringMarkers[0]+ "|$))");
        Regex regex = new Regex(@"\s*");
        Regex comment = new Regex(@"\"+commentMarker+@"(.*?)\"+ commentMarker + @"");



        chunk = regex.Replace(chunk, "");
        chunk = comment.Replace(chunk, "");

        return chunk.Replace("\n", "").Replace("	", "");
    }

    static string Truncate(this string ensemble)
    {
        return ensemble.Substring(1, ensemble.Length - 2);
    }

    static Relation SeparateRelation(this string chunk)
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

    static Relation SeparateFunctionCall(this string chunk)
    {
        int index = 0;
        int start = 0;
        int end = 0;
        bool foundStart = false;
        foreach (char chr in chunk)
        {
            if (!foundStart && chr == functionMarkers[0])
            {
                start = index;
                foundStart = true;
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
    }
    
    static Context ReadContext(this string chunk)
    {
        var context = new Context();
        var index = 1; // Used for unnamed args*
        foreach(var block in chunk.ExplodeChunk(new char[][] {ensembleMarkers, functionMarkers }))
        {
            Relation relation;
            try {
                relation = SeparateRelation(block);
            }
            catch (InvalidRelationException e) {
                // most probably just a list element
                Logger.Debug("You can ignore this message safely (list detection of interpreter) : " + e.Message);
                relation = new Relation(index.ToString(), block);
                index++;
            }
            context[relation.key] = relation.content;
        }
        return context;
    }

    static void ReadNumberHelpers(this string chunk)
    {
        foreach(var line in chunk.ExplodeChunk(new char[][] { ensembleMarkers, functionMarkers })) {
            var relation = line.SeparateRelation();
            Func<float> action = delegate {
                return ToNumber(relation.content);
            };
            var name = relation.key;

            numberHelpers[name] = action;
        }
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
            throw new UnsolvedDepthException("Out of depth! Are you closing all your ensembles?\nOpened ensembles:"+string.Join(" ", expectedEndMarkers)+"\nExceeding data:\n" + currentChunk);
        }
        return chunks;
    }

    static void LoadRelations<T>(this string chunk, ActionTable<T> actionTable, T subject)
    {
        var chunks = chunk.Sanitize().ExplodeChunk(new char[][] { ensembleMarkers, functionMarkers });
        foreach (var definition in chunks) {
            var relation = SeparateRelation(definition);
            var key = relation.key.ToUpper();
            if (!actionTable.ContainsKey(key)) { throw new UnknownKeyException(key, actionTable.Keys.ToList()); }
            actionTable[key].Invoke(subject, relation.content);
        }
    }

    static bool StringToBool(string chunk)
    {
        switch (chunk.ToUpper()) {
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

    static string ReadLitteralString(string chunk)
    {
        foreach (char character in litteralStringMarkers[0]) {
            chunk = chunk.Truncate();
        }
        return chunk;
    }

    // Give it either a number or a chunk that is supposed to be a data function returning an int
    static float ToNumber(this string chunk)
    {
        float result;
        if (float.TryParse(chunk, out result)) { return Convert.ToSingle(chunk); }
        
        // Keywords?
        switch (chunk.ToUpper()) {
            case "INF": return Mathf.Infinity;
            case "NINF":
            case "-INF":
                return Mathf.NegativeInfinity;
        }

        // Maybe a helper ?
        try {
            var rel = SeparateFunctionCall(chunk);

            if (numberHelpers.ContainsKey(rel.key)) {
                return numberHelpers[rel.key].Invoke();
            }
        }
        catch (InvalidRelationException e) {
            Logger.Debug("You can ignore this error safely (interpreter trying to cast number as helper) : " + e.Message);
        }
        // Data function most probably
        var relation = SeparateFunctionCall(chunk);
        return numberDataFunctions[relation.key].Invoke(
            Context.Merge(context, ReadContext(relation.content))
        );
        
    }

    static int ToInt(this float f)
    {
        return Mathf.RoundToInt(f);
    }

    #endregion

    #region COMMANDS

    #region CREATION RULES

    #region RULER
    class CharacteristicDefinitionRuleParameters
    {
        public Character.Characteristic concernedCharacteristic;
        public Character.Characteristics rulerCharacteristics;
        public Context arguments = new Context();
        public int change = 0;
    }
    static Dictionary<string, Action<CharacteristicDefinitionRuleParameters>> characteristicDefinitionsRules = new Dictionary<string, Action<CharacteristicDefinitionRuleParameters>>()
    {
        {
            "MAP",(CharacteristicDefinitionRuleParameters x) => {
                var characteristic = x.arguments["CHAR"];
                var over = 0;
                if (x.arguments.ContainsKey("OVER")) over = ToNumber(x.arguments["OVER"]).ToInt();

                if (x.concernedCharacteristic.GetClampedValue() > over + Mathf.Min(x.change, 0)){
                    x.rulerCharacteristics[characteristic].SetRaw(x.rulerCharacteristics[characteristic].GetValue() + x.change);
                }
            }
        },
        {
            "REVERSE_MAP",(CharacteristicDefinitionRuleParameters x) => {
                var characteristic = x.arguments["CHAR"];
                var over = 0;
                if (x.arguments.ContainsKey("OVER")) over = ToNumber(x.arguments["OVER"]).ToInt();

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

    static ActionTable<Ruler.CreationRules> rulerCreationRulesElements = new ActionTable<Ruler.CreationRules>(
        new NamedAction<Ruler.CreationRules>("CHARACTERISTICS", (rules, content) => { rules.characteristicDefinitions = ReadCharacteristicDefinitions(content); }),
        new NamedAction<Ruler.CreationRules>("STOCK", (rules, content) => { rules.stock = ToNumber(content).ToInt(); }),
        new NamedAction<Ruler.CreationRules>("MAJORITY", (rules, content) => { rules.majority = ToNumber(content).ToInt(); }),
        new NamedAction<Ruler.CreationRules>("BASE_LIFESPAN", (rules, content) => { rules.maximumLifespan = ToNumber(content).ToInt(); }),
        new NamedAction<Ruler.CreationRules>("LIFESPAN_TO_STOCK_RATIO", (rules, content) => { rules.lifespanToStockRatio = Convert.ToSingle(content, CultureInfo.InvariantCulture); }),
        new NamedAction<Ruler.CreationRules>("MAX_STARTING_AGE", (rules, content) => { rules.maxStartingAge = ToNumber(content).ToInt(); })
    );

    class CCDRAndCharaName { public Character.CharacteristicDefinition.Rules rules = new Character.CharacteristicDefinition.Rules(); public string name; }
    static ActionTable<CCDRAndCharaName> characteristicDefinitionRulesElements = new ActionTable<CCDRAndCharaName>(
        new NamedAction<CCDRAndCharaName>("IS_BAD", (rulesAndChara, content) => { rulesAndChara.rules.isBad = StringToBool(content); }),
        new NamedAction<CCDRAndCharaName>("IS_FROZEN", (rulesAndChara, content) => { rulesAndChara.rules.isFrozen = StringToBool(content); }),
        new NamedAction<CCDRAndCharaName>("ON_CHANGE", (rulesAndChara, content) => {
            var deleg = ReadCharacteristicDefinitionRule(content, rulesAndChara.name);
            rulesAndChara.rules.onChange += (Character.Characteristics charaSet, int change) => {
                deleg.Invoke(charaSet, change);
            };
        })
    );

    class CCDAndCharaName { public Character.CharacteristicDefinition def = new Character.CharacteristicDefinition(); public string name; }
    static ActionTable<CCDAndCharaName> characteristicDefinitionElements = new ActionTable<CCDAndCharaName>(
        new NamedAction<CCDAndCharaName>("MIN", (defAndChara, content) => { defAndChara.def.min = ToNumber(content).ToInt(); }),
        new NamedAction<CCDAndCharaName>("MAX", (defAndChara, content) => { defAndChara.def.max = ToNumber(content).ToInt(); }),
        new NamedAction<CCDAndCharaName>("COST", (defAndChara, content) => { defAndChara.def.cost = ToNumber(content).ToInt(); }),
        new NamedAction<CCDAndCharaName>("RULES", (defAndChara, content) => { defAndChara.def.rules = ReadCharacteristicDefinitionRules(content.Truncate(), defAndChara.name); })
    );

    #endregion

    #region KINGDOM & REGION

    static ActionTable<Kingdom.Behavior> kingdomBehaviorElements = new ActionTable<Kingdom.Behavior>(
        new NamedAction<Kingdom.Behavior>("HELPERS", (rules, content) => { ReadNumberHelpers(content.Truncate()); }),
        new NamedAction<Kingdom.Behavior>("RESOURCES", (rules, content) => { rules.resourceDefinitions = ReadResourceDefinitions(content); }),
        new NamedAction<Kingdom.Behavior>("ON_NEW_DAY", (rules, content) => { rules.onNewDay = (kingdom) => { context["KINGDOM"] = kingdom.id.ToString();  ReadActions(content.Truncate()).Invoke(); }; }),
        new NamedAction<Kingdom.Behavior>("ON_NEW_MONTH", (rules, content) => { rules.onNewDay = (kingdom) => { context["KINGDOM"] = kingdom.id.ToString(); print("Reading actions " + content.Truncate()); ReadActions(content.Truncate()).Invoke(); }; })
    );

    static ActionTable<Region.Behavior> regionBehaviorElements = new ActionTable<Region.Behavior>(
        new NamedAction<Region.Behavior>("HELPERS", (rules, content) => { ReadNumberHelpers(content.Truncate()); }),
        new NamedAction<Region.Behavior>("RESOURCES", (rules, content) => { rules.resourceDefinitions = ReadResourceDefinitions(content); }),
        new NamedAction<Region.Behavior>("ON_NEW_DAY", (rules, content) => { rules.onNewDay = (region) => { context["REGION"] = region.id.ToString(); ReadActions(content.Truncate()).Invoke(); }; }),
        new NamedAction<Region.Behavior>("ON_NEW_MONTH", (rules, content) => { rules.onNewMonth = (region) => { context["REGION"] = region.id.ToString(); ReadActions(content.Truncate()).Invoke(); }; })
    );    

    static ActionTable<ResourceDefinition> resourceDefinitionElements = new ActionTable<ResourceDefinition>(
        new NamedAction<ResourceDefinition>("MIN", (def, content) => { def.min = ToNumber(content); }),
        new NamedAction<ResourceDefinition>("MAX", (def, content) => { def.max = ToNumber(content); }),
        new NamedAction<ResourceDefinition>("START", (def, content) => { def.start = ToNumber(content); }),
        new NamedAction<ResourceDefinition>("RULES", (def, content) => {
            content.Truncate().LoadRelations(resourceDefinitionElements, def);
        }),
        // Rules
        new NamedAction<ResourceDefinition>("NO_MODIFIERS", (def, content) => { def.noModifiers = StringToBool(content); }),
        new NamedAction<ResourceDefinition>("IS_MUTABLE", (def, content) => { def.isMutable = StringToBool(content); })
    );

    #endregion

    #endregion

    #region RACE INFO

    static ActionTable<Race> raceInfoElements = new ActionTable<Race>(
        new NamedAction<Race>("ID", (race, content) => { race.id = ToNumber(content).ToInt(); }),
        new NamedAction<Race>("NAME", (race, content) => { race.name = content; }),
        new NamedAction<Race>("CHARACTER_NAME_FORMAT", (race, content) => { race.characterNameFormat = ReadLitteralString(content); }),
        new NamedAction<Race>("PLURAL", (race, content) => { race.plural = content; }),
        new NamedAction<Race>("IS_PLAYABLE", (race, content) => { race.isPlayable = StringToBool(content); }),
        new NamedAction<Race>("RULER_TITLE", (race, content) => { race.rulerTitle = content; })
    );

    static ActionTable<Race> raceNamesElements = new ActionTable<Race>(
        new NamedAction<Race>("FIRST_NAMES", (race, content) => { race.names.first = content.Truncate().Split(separator).ToList(); }),
        new NamedAction<Race>("FAMILY_NAMES", (race, content) => { race.names.family = content.Truncate().Split(separator).ToList(); }),
        new NamedAction<Race>("KINGDOM_NAMES", (race, content) => { race.names.kingdoms = content.Truncate().Split(separator).ToList(); })
    );

    #endregion

    #region ACTIONS

    static ActionTable<World> actionElements = new ActionTable<World>(
        new NamedAction<World>("SET_RESOURCE_VALUE", (world, content) => {
            var ctx = Context.Merge(context, ReadContext(content));
            ctx.Require("KINGDOM", "VALUE", "RSC");
            var kingdomId = ctx["KINGDOM"].ToNumber();
            var kingdom = Game.world.kingdoms.Find(o => o.id == kingdomId);
            var value = ctx["VALUE"].ToNumber();

            kingdom.resources[ctx["RSC"]].SetRaw(value);
        }),
        new NamedAction<World>("INCREMENT_RESOURCE_VALUE", (world, content) => {
            var ctx = Context.Merge(context, ReadContext(content));
            ctx.Require("KINGDOM", "AMOUNT", "RSC");
            var kingdomId = ctx["KINGDOM"].ToNumber();
            var kingdom = Game.world.kingdoms.Find(o => o.id == kingdomId);
            var value = ctx["AMOUNT"].ToNumber();

            kingdom.resources[ctx["RSC"]].Increase(kingdom.resources, value);
        })
    );

    #endregion

    #region DATA FUNCTIONS

    static FunctionTable<float> numberDataFunctions = new FunctionTable<float>(
        // Arithmetic
        new NamedFunction<float>("SUM", ctx => {
            List<float> elements = new List<float>();
            for (int i = 1; i < ctx.Count + 1; i++) {
                elements.Add(ctx[i.ToString()].ToNumber());
            }
            return elements.Sum();
        }),
        new NamedFunction<float>("DIVIDE", ctx => {
            var value = ctx["1"].ToNumber();
            for (int i = 2; i < ctx.Count + 1; i++) {
                value /= ctx[i.ToString()].ToNumber();
            }
            return value;
        }),
        new NamedFunction<float>("MULTIPLY", ctx => {
            float value = ctx["1"].ToNumber();
            for (int i = 2; i < ctx.Count + 1; i++) {
                value *= ctx[i.ToString()].ToNumber();
            }
            return value;
        }),
        new NamedFunction<float>("STEP", ctx => {
            float a = ctx["REFERENCE"].ToNumber();
            float b = ctx["OVER"].ToNumber();
            if (a >= b) return 1f;
            else return 0f;
        }),
        new NamedFunction<float>("NEGATE", ctx => {
            return -ctx["VALUE"].ToNumber();
        }),

        // Resources
        new NamedFunction<float>("GET_RESOURCE_VALUE", ctx => {
            var kingdomId = ctx["KINGDOM"].ToNumber();
            var kingdom = Game.world.kingdoms.Find(o => o.id == kingdomId);
            return kingdom.resources[ctx["RSC"]].GetValue();
        }),
        new NamedFunction<float>("GET_RESOURCE_MAX", ctx => {
            var kingdomId = ctx["KINGDOM"].ToNumber();
            var kingdom = Game.world.kingdoms.Find(o => o.id == kingdomId);
            return kingdom.resources[ctx["RSC"]].definition.max;
        }),
        new NamedFunction<float>("GET_RESOURCE_MIN", ctx => {
            var kingdomId = ctx["KINGDOM"].ToNumber();
            var kingdom = Game.world.kingdoms.Find(o => o.id == kingdomId);
            return kingdom.resources[ctx["RSC"]].definition.min;
        }),

        // Game situation
        new NamedFunction<float>("GET_NUMBER_OF_OWNED_REGIONS", ctx => {
            var kingdomId = ctx["KINGDOM"].ToNumber();
            var kingdom = Game.world.kingdoms.Find(o => o.id == kingdomId);
            return kingdom.GetTerritory().Count;
        }),
        new NamedFunction<float>("GET_NUMBER_OF_REGIONS", ctx => {
            return Game.world.map.regions.Count;
        }),
        new NamedFunction<float>("GET_SUM_OF_ALL_OWNED_REGIONS_RESOURCE_VALUE", ctx => {
            var kingdomId = ctx["KINGDOM"].ToNumber();
            var kingdom = Game.world.kingdoms.Find(o => o.id == kingdomId);
            var sum = 0f;
            foreach(var region in kingdom.GetTerritory()) {
                sum += region.resources[ctx["RSC"]].GetValue();
            }
            return sum;
        })

    );
    #endregion

    #endregion

    static Dictionary<string, List<string>> ReadDataLists(string chunk)
    {
        var dataTable = new Dictionary<string, List<string>>();
        var list = new List<string>();
        var data = chunk.Sanitize().ExplodeChunk(new char[][] { ensembleMarkers, functionMarkers });
        foreach (var category in data) {
            var relation = category.SeparateRelation();
            dataTable.Add(relation.key, relation.content.Truncate().Split(separator).ToList());
        }

        return dataTable;
    }

    public static Race LoadRaceNames(Race race, string chunk)
    {
        var data = ReadDataLists(chunk);
        LoadRelations(chunk, raceNamesElements, race);
        return race;

    }

    public static Race ReadRaceInfo(string raceInfo)
    {
        var race = new Race();
        raceInfo.LoadRelations(raceInfoElements, race);
        return race;
    }

    public static Kingdom.Behavior ReadKingdomBehavior(string chunk)
    {
        var rules = new Kingdom.Behavior();
        chunk.LoadRelations(kingdomBehaviorElements, rules);
        return rules;
    }

    public static Region.Behavior ReadRegionBehavior(string chunk)
    {
        var rules = new Region.Behavior();
        chunk.LoadRelations(regionBehaviorElements, rules);
        return rules;
    }

    public static ResourceDefinitions ReadResourceDefinitions(string resources)
    {
        var definitions = new ResourceDefinitions();
        var saneChunk = resources.Truncate();
        foreach (var chunk in saneChunk.ExplodeChunk(new char[][] { ensembleMarkers, functionMarkers })) {
            var relation = SeparateRelation(chunk);
            definitions.Add(relation.key, ReadResourceDefinition(relation.content.Truncate(), relation.key));
        }

        return definitions;
    }

    public static ResourceDefinition ReadResourceDefinition(string chunk, string rscName)
    {
        var def = new ResourceDefinition();
        chunk.LoadRelations(resourceDefinitionElements, def);
        return def;
    }


    public static Ruler.CreationRules ReadRulerCreationRules(string creationRules)
    {
        var rules = new Ruler.CreationRules();
        creationRules.LoadRelations(rulerCreationRulesElements, rules);
        return rules;
    }

    // charisma: [xxxx], martial [xxxxx]
    static Character.CharacteristicDefinitions ReadCharacteristicDefinitions(string definitionsChunk)
    {
        var definitions = new Character.CharacteristicDefinitions();
        var saneChunk = definitionsChunk.Truncate();
        foreach (var chunk in saneChunk.ExplodeChunk(new char[][] { ensembleMarkers, functionMarkers }))
        {
            var relation = SeparateRelation(chunk);
            definitions.Add(relation.key, ReadCharacteristicDefinition(relation.content.Truncate(), relation.key));
        }
        return definitions;
    }

    // RULES:[xx], MIN:xx, MAX:xx
    static Character.CharacteristicDefinition ReadCharacteristicDefinition (string definition, string charName)
    {
        var defAndName = new CCDAndCharaName() { name = charName };
        definition.LoadRelations(characteristicDefinitionElements, defAndName);
        return defAndName.def;
    }

    // RULES:[ON_OVER_HALF: xx(xxx), ON_CHANGE: xx(xxx)]
    static Character.CharacteristicDefinition.Rules ReadCharacteristicDefinitionRules (string chunk, string charName)
    {
        var def = new CCDRAndCharaName() { name = charName };
        chunk.LoadRelations(characteristicDefinitionRulesElements, def);
        return def.rules;
    }

    // xx(a:b)
    static Action<Character.Characteristics, int> ReadCharacteristicDefinitionRule(string chunk, string charName)
    {
        var relation = SeparateFunctionCall(chunk);
        if (!characteristicDefinitionsRules.ContainsKey(relation.key))
        {
            throw new UnknownFunctionException(relation.key, characteristicDefinitionsRules.Keys.ToList());
        }

        return (Character.Characteristics set, int change) => {
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
    
    static Action ReadActions(string chunk)
    {
        List<Action> actions = new List<Action>();
        foreach (var instruction in chunk.ExplodeChunk(new char[][] { ensembleMarkers, functionMarkers })) {
            var call = SeparateFunctionCall(instruction);
            if (!actionElements.ContainsKey(call.key)) throw new UnknownFunctionException(call.key, actionElements.Keys.ToList());
            actions.Add(delegate { actionElements[call.key](Game.world, call.content); });
        }

        return delegate {
            foreach (var action in actions) {
                action.Invoke();
            }
        };
    }
}
