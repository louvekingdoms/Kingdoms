using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AI : IBrain
{
    readonly int id = 1;
    Ruler ruler;

    public AI()
    {
        id = Game.players.GrabFreeID();
    }

    public int GetId()
    {
        return id;
    }

    public bool IsLocal()
    {
        throw new NotImplementedException();
    }

    public void Own(Ruler ruler)
    {
        this.ruler = ruler;
        ruler.brain = this;
    }
}

