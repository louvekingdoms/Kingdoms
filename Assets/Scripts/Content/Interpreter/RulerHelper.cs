using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace KingdomsGame.Interpreter
{
    public static class RulerHelper
    {
        public static Table Serialize(this Ruler ruler, Script script)
        {
            var table = new Table(script);
            
            return table;
        }


    }
}