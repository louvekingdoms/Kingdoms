using System.Collections;
using System.Collections.Generic;
using System;
using System.Drawing;

public static class Utility
{
    public static T PickRandom<T>(this List<T> list, Random r)
    {
        return list[r.Range(0, list.Count)];
    }

    public static float Sum(this IEnumerable<float> list)
    {
        var sum = 0f;
        foreach(var element in list) {
            sum += element;
        }
        return sum;
    }

    public static int Range(this Random r, int a, int b)
    {
        return r.Next(a, b);
    }

    public static float Range(this Random r, float a, float b)
    {
        return (float)(a + r.NextDouble() * (b - a));
    }

    public static float NextFloat(this Random r)
    {
        return (float)r.NextDouble();
    }

    public static string Format(this string str, params object[] values)
    {
        return string.Format(str, values);
    }
}
