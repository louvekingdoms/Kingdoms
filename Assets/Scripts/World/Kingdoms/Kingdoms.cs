using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Kingdoms : List<Kingdom>
{
    public new Kingdom this[int i] => Find(o => o.id == i);
}