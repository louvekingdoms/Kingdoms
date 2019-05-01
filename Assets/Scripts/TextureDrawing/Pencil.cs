using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Pencil
{
    static Color pencilColor = Color.white;

    public static void SetPencilColor(this Texture2D tex, Color c)
    {
        pencilColor = c;
    }

    public static void Point(this Texture2D tex, Vector2 UVOrigin){
        tex.Circle(UVOrigin, 1f);
    }

    public static void Circle(this Texture2D tex, Vector2 UVOrigin, float radius)
    {
        var origin = tex.UVToRasterCoordinate(UVOrigin);
        var iRadius = Mathf.RoundToInt(radius);

        for (int y = -iRadius; y <= iRadius; y++)
            for (int x = -iRadius; x <= iRadius; x++)
                if (x * x + y * y <= iRadius * iRadius + iRadius * 0.8)
                    tex.SetPixel(origin.x + x, origin.y + y, pencilColor);

    }

    public static void Line(this Texture2D tex, Vector2 UVP1, Vector2 UVP2, float width=1f)
    {
        var p1 = tex.UVToRasterCoordinate(UVP1);
        var p2 = tex.UVToRasterCoordinate(UVP2);

        // Dark magic found on the internet
        int w = p2.x - p1.x;
        int h = p2.y - p1.y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);
        if (!(longest > shortest)) {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++) {
            tex.SetPixel(p1.x, p1.y, pencilColor);
            numerator += shortest;
            if (!(numerator < longest)) {
                numerator -= longest;
                p1 += new Vector2Int(dx1, dy1);
            }
            else {
                p1 += new Vector2Int(dx2, dy2);
            }
        }
    }

    public static void Polygon(this Texture2D tex, List<Vector2> UVs, float width = 1f)
    {
        for (int i = 1; i < UVs.Count; i++) {
            tex.Line(UVs[i - 1], UVs[i], width);
        }
        tex.Line(UVs[UVs.Count-1], UVs[0], width);
    }

    static Vector2Int UVToRasterCoordinate(this Texture2D tex, Vector2 UV)
    {
        UV.CheckValue();
        return new Vector2Int(
            Mathf.RoundToInt(UV.x * tex.width),
            Mathf.RoundToInt(UV.y * tex.height)
        );
    }

    static void CheckValue(this Vector2 UV)
    {
        if (UV.x < 0f || UV.y < 0f || UV.magnitude > Mathf.Sqrt(2f)) {
            throw(new System.Exception("Invalid UV (out of bounds?) : "+ UV +"->"+ UV.magnitude));
        }
    }
}
