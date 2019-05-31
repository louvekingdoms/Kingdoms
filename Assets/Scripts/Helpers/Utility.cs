using System.Collections;
using System.Collections.Generic;

public static class Utility
{
    public static T RandomElement<T>(this List<T> list)
    {
        return list[Game.random.Range(0, list.Count)];
    }

    public static float Sum(this IEnumerable<float> list)
    {
        var sum = 0f;
        foreach(var element in list) {
            sum += element;
        }
        return sum;
    }

    public static int Range(this System.Random r, int min=0, int max=1)
    {
        return (int)System.Math.Floor(r.NextDouble() * (max - min) + min);
    }

    public static float NextFloat(this System.Random r)
    {
        return r.NextDouble().ToFloat();
    }

    public static float ToFloat(this double a)
    {
        return (float)a;
    }
}
