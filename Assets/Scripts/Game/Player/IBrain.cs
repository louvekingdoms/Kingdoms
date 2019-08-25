using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IBrain
{
    bool IsLocal();
    int GetId();
    void Own(Ruler ruler);
}
