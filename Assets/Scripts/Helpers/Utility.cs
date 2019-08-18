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

    public static Color ColorFromHSV(float h, float s, float v)
    {
        int hi = (int)Math.Floor(h / 60.0) % 6;
        float f = (h / 60.0f) - (float)Math.Floor(h / 60.0f);

        float p = v * (1.0f - s);
        float q = v * (1.0f - (f * s));
        float t = v * (1.0f - ((1.0f - f) * s));

        byte a = (byte)KMaths.FloorToInt(p * 255);
        byte b = (byte)KMaths.FloorToInt(q * 255);
        byte c = (byte)KMaths.FloorToInt(t * 255);
        byte d = (byte)KMaths.FloorToInt(v * 255);

        Color ret;

        switch (hi)
        {
            case 0:
                ret = Color.FromArgb(d, c, a);
                break;
            case 1:
                ret = Color.FromArgb(c, d, a);
                break;
            case 2:
                ret = Color.FromArgb(a, d, c);
                break;
            case 3:
                ret = Color.FromArgb(a, c, d);
                break;
            case 4:
                ret = Color.FromArgb(c, a, d);
                break;
            case 5:
                ret = Color.FromArgb(d, a, c);
                break;
            default:
                ret = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
                break;
        }
        return ret;
    }
}
