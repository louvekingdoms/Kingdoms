using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Subjects : List<Subject>
{
    public new Subject this[int i] => Find(o => o.id == i);
}