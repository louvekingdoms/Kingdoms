using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreFilledLibrary : Library
{
    static public Dictionary<int, Race> races = new Dictionary<int, Race>();

    PreFilledLibrary()
    {
        races.Add(1, new Race()
        {
            isPlayable=true,
            id=1,
            name="Hoomans",
            names=new Race.Names()
            {
                first = new List<string>() { "John"},
                family = new List<string>() { "Gardopee" },
                kingdoms = new List<string>() { "Xanadu" },
            }
        }
        );
    }
}
