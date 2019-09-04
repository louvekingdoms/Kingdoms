using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace KingdomsGame.Interpreter
{
    public static class ResourceHelper
    {
        public static Table Serialize(this Resource resource, Script script)
        {
            var resourceTable = new Table(script);

            resourceTable.Set("value", DynValue.NewNumber(resource.GetValue()));
            resourceTable.Set("definition", DynValue.NewTable(resource.definition.Serialize(script)));

            return resourceTable;
        }

        public static Table Serialize(this Resources resources, Script script)
        {
            var tResources = new Table(script);

            foreach (var key in resources.Keys)
            {
                var resource = resources[key];
                var resourceTable = resource.Serialize(script);

                tResources.Set(
                    DynValue.NewString(key),
                    DynValue.NewTable(resourceTable)
                );
            }

            return tResources;
        }

        public static Table Serialize(this Resource.Definition resourceDefinition, Script script)
        {
            var defTable = new Table(script);

            defTable.Set("min", DynValue.NewNumber(resourceDefinition.min));
            defTable.Set("max", DynValue.NewNumber(resourceDefinition.max));
            defTable.Set("start", DynValue.NewNumber(resourceDefinition.start));
            defTable.Set("noModifiers", DynValue.NewBoolean(resourceDefinition.noModifiers));
            defTable.Set("isMutable", DynValue.NewBoolean(resourceDefinition.isMutable));

            return defTable;
        }

        public static void LoadValuesFromTable(this Resources resources, Table luaTable)
        {
            foreach (var key in luaTable.Keys)
            {
                var tResource = luaTable.Get(key).Table;
                var resource = resources[key.String];
                resource.LoadValuesFromTable(tResource);
            }
        }

        public static void LoadValuesFromTable(this Resource resource, Table luaTable)
        {
            var target = luaTable.Get("value").Number.RoundToInt();
            resource.Set(target);
        }

        public static void LoadFromTable(this Resource.Definitions resourceDefinitions, Table luaTable)
        {
            foreach (var key in luaTable.Keys)
            {
                var tResourceDef = luaTable.Get(key).Table;
                var def = new Resource.Definition();
                def.LoadFromTable(tResourceDef);
                resourceDefinitions[key.String] = def;
            }
        }

        public static void LoadFromTable(this Resource.Definition resourceDefinition, Table luaTable)
        {
            resourceDefinition.min = luaTable.Get("min").Number.RoundToInt();
            resourceDefinition.max = luaTable.Get("max").Number.RoundToInt();
            resourceDefinition.start = luaTable.Get("start").Number.RoundToInt();
            resourceDefinition.noModifiers = luaTable.Get("noModifiers").Boolean;
            resourceDefinition.isMutable = luaTable.Get("isMutable").Boolean;
        }
    }
}