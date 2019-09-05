using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingdomsGame.Interpreter
{
    public static class SubjectHelper
    {
        public static Subject MakeSubjectFrom(this Table table)
        {
            var subject = new Subject(
                (int)table.Get("id").Number,
                table.Get("image").String
            );

            return subject;
        }
        
        public static Subjects MakeSubjectsFrom(this Table table)
        {
            var sub = new Subjects();
            foreach(var tSub in table.Values)
            {
                sub.Add(tSub.Table.MakeSubjectFrom());
            }
            return sub;
        }
    }
}
