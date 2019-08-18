using System.Collections;
using System.Collections.Generic;
using System;
using KingdomsSharedCode.JSON;

[Serializable]
public class GameState
{
    [NonSerialized] public Random random;
    public int seed;
    public World world;
    public string librarySum;
    public Ruleset ruleset = Rules.set;

    public string Sum()
    {
        List<byte> bytes = new List<byte>();

        // Manual regrouping of data and then putting all the bytes one after each other

        return Safety.Hash(bytes.ToArray());
    }


}
