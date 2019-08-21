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

    byte[] lastSum = null;

    public byte[] Sum()
    {
        List<byte> bytes = new List<byte>();

        // Manual regrouping of data and then putting all the bytes one after each other
        // TODO

        lastSum = Safety.Hash(bytes.ToArray());
        return GetLastSum();
    }

    public byte[] GetLastSum()
    {
        return new List<byte>(lastSum).ToArray();
    }
}
