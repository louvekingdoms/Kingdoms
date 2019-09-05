using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Relation
{
    List<RelationModifier> modifiers;
    int rawValue;


    public int GetValue()
    {
        int total = rawValue;
        foreach(var mod in modifiers)
        {
            total += mod.GetImpact();
        }
        return total;
    }
}