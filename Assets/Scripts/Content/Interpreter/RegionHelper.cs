using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace KingdomsGame.Interpreter
{
    public static class RegionHelper
    {

        public static void LoadRegionBehavior(Table table)
        {
            var behavior = new Region.Behavior();

            behavior.onGameStart = new Action<Region>((region) =>
            {
                var tRegion = region.Serialize(table.OwnerScript);
                table.Get("onGameStart").Function.Call(
                    tRegion
                );
                region.LoadValuesFromTable(tRegion);
            });

            behavior.onNewDay = new Action<Region>((region) =>
            {
                var tRegion = region.Serialize(table.OwnerScript);
                table.Get("onNewDay").Function.Call(
                    tRegion
                );
                region.LoadValuesFromTable(tRegion);
            });

            behavior.onNewMonth = new Action<Region>((region) =>
            {
                var tRegion = region.Serialize(table.OwnerScript);
                table.Get("onNewMonth").Function.Call(
                    tRegion
                );
                region.LoadValuesFromTable(tRegion);
            });

            behavior.onNewYear = new Action<Region>((region) =>
            {
                var tRegion = region.Serialize(table.OwnerScript);
                table.Get("onNewYear").Function.Call(
                    tRegion
                );
                region.LoadValuesFromTable(tRegion);
            });

            Region.behavior = behavior;
        }

        public static void LoadRegionResourceDefinitions(Table table)
        {
            var resourceDefinitions = new Resource.Definitions();

            resourceDefinitions.LoadFromTable(table);

            Region.resourceDefinitions = resourceDefinitions;
        }

        public static Table Serialize(this Region region, Script script)
        {
            var table = new Table(script);

            // Resources
            table.Set("resources", DynValue.NewTable(region.resources.Serialize(script)));

            // Set functions
            table.Set("setOwner", new LuaFunction(
                (context) =>
                {
                    var args = context.args.GetArray();
                    var kingdomId = (int)args[0].Number;
                    var kingdom = Game.state.world.kingdoms[kingdomId];

                    if (kingdom != null)
                    {
                        kingdom.TakeOwnership(region);
                    }
                    return DynValue.Nil;
                }
            ));

            // Get functions
            table.Set("getOwner", new LuaFunction(
                (context) =>
                {
                    if (region.IsOwned())
                    {
                        return DynValue.Nil;
                    }
                    return DynValue.NewTable(region.GetOwner().Serialize(script));
                }
            ));


            return table;
        }


        public static void LoadValuesFromTable(this Region region, Table luaTable)
        {
            region.resources.LoadValuesFromTable(luaTable.Get("resources").Table);
        }

        public static Table Serialize(this List<Region> regions, Script script)
        {
            var table = new Table(script);
            foreach(var region in regions)
            {
                table.Append(DynValue.NewTable(region.Serialize(script)));
            }
            return table;
        }
    }
}