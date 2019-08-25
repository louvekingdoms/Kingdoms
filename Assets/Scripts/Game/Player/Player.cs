using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameLogger;

public class Player : IBrain
{
    int id = 0;
    bool isLocal = false;
    string name = "Player";
    Ruler ruler;

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

    public void Own(Ruler ruler)
    {
        this.ruler = ruler;
        ruler.brain = this;
    }

    public bool IsLocal()
    {
        return isLocal;
    }

    public int GetId()
    {
        return id;
    }
}
