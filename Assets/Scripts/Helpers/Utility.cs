using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static T RandomElement<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static float Sum(this IEnumerable<float> list)
    {
        var sum = 0f;
        foreach(var element in list) {
            sum += element;
        }
        return sum;
    }
}
