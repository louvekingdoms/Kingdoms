using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static T RandomElement<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}
