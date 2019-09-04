using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace KingdomsGame.Interpreter
{
    public static class MapHelper
    {
        public static Table Serialize(this Map map, Script script)
        {
            var table = new Table(script);

            table.Set("getRegions", new LuaFunction(
                context =>
                {
                    return DynValue.NewTable(map.regions.Serialize(script));
                }
            ));

            return table;
        }
    }
}