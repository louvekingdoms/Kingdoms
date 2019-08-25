using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameLogger;

public class Players : List<Player>
{
    Random random;

    public Players() : base()
    {
        random = new Random();
        Add(Player.LoadLocalPlayer(1));
    }

    public int GrabFreeID()
    {
        int id = random.Next(0, ushort.MaxValue-1);
        while (IsIDTaken(id))
        {
            id = random.Next(0, ushort.MaxValue - 1);
        }
        return id;
    }

    public new Player this[int i] => Find(o => o.GetId() == i);

    bool IsIDTaken(int id)
    {
        return Find(o => o.GetId() == id) != null;
    }

    public Player localPlayer { get { return Find(o => o.IsLocal()); } }
}
