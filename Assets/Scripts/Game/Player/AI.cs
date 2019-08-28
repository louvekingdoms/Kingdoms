using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AI : Brain
{
    readonly int id = 1;

    public AI()
    {
        id = Game.players.GrabFreeID();
    }

    public override int GetId()
    {
        return id;
    }

    public override bool IsLocal()
    {
        throw new NotImplementedException();
    }
}

