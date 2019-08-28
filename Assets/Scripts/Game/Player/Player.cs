using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameLogger;

public class Player : Brain
{
    int id = 0;
    bool isLocal = false;
    string name = "Player";

    public Player() : this(Game.players.GrabFreeID()){}

    public Player(int id){
        this.id = id;
        logger.Info("Created player with id " + id);
    }

    public static Player LoadLocalPlayer(int id)
    {
        return new Player(id)
        {
            name = Environment.UserName,
            isLocal = true
        };
    }


    public override bool IsLocal()
    {
        return isLocal;
    }

    public override int GetId()
    {
        return id;
    }

    public override bool CanSeeSecretsOf(Ruler ruler)
    {
        if (Game.flags.IS_OMNISCIENT) return true;

        if (ruler.brain == this)
        {
            return true;
        }
        return false;
    }
}
