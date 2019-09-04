using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingdomsGame.Interpreter
{
    public static class DateHelper
    {
        public static DynValue Serialize(this Clock.Date date, Script script)
        {
            return DynValue.NewString(date.ToString());
        }
    }
}
