using System.Collections;
using System.Collections.Generic;
using System;

public static class KMaths
{
    public static int FloorToInt(this float a)
    {
        return (int)Math.Floor(a);
    }
    public static int RoundToInt(this float a)
    {
        return (int)Math.Round(a);
    }
    public static int FloorToInt(this double a)
    {
        return (int)Math.Floor(a);
    }
    public static int RoundToInt(this double a)
    {
        return (int)Math.Round(a);
    }
    public static float Clamp(float a, float min, float max)
    {
        return Math.Min(Math.Max(a, min), max);
    }
    public static int Clamp(int a, int min, int max)
    {
        return Math.Min(Math.Max(a, min), max);
    }
    public static float Clamp01(float a)
    {
        return Clamp(a, 0f, 1f);
    }
    public static int Clamp01(int a)
    {
        return Clamp(a, 0, 1);
    }
}
