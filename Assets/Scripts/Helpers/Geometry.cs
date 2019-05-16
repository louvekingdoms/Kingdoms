using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

public static class Geometry
{
    public class Segment { public Vector2 a; public Vector2 b; }
    public class Polygon : List<Segment> { }
    
    public static List<Vector2> Points(this Polygon polygon)
    {
        var points = new List<Vector2>();
        foreach (var seg in polygon) {
            points.Add(seg.a);
            points.Add(seg.b);
        }
        return points;
    }

    public static Vector2 Dimensions(this Site site)
    {
        var bounds = site.ToPolygon().Bounds();
        var dim = bounds[1] - bounds[0];
        return dim;
    }

    public static Vector2[] Bounds(this Polygon polygon)
    {
        var lowest = new Vector2(Mathf.Infinity, Mathf.Infinity);
        var highest = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);

        foreach (var segment in polygon) {
            var points = new List<Vector2>() { segment.a, segment.b };
            foreach (var point in points) {
                if (lowest.x > point.x) lowest.x = point.x;
                if (lowest.y > point.y) lowest.y = point.y;
                if (highest.x < point.x) highest.x = point.x;
                if (highest.y < point.y) highest.y = point.y;
            }
        }

        return new Vector2[] { lowest, highest };
    }

    public static void Move(this Polygon segments, Vector2 offset)
    {
        var newPoly = new Polygon();
        foreach (var seg in segments) {
            newPoly.Add(new Segment() {
                a = seg.a + offset,
                b = seg.b + offset
            });
        }
        segments = newPoly;
    }

    public static void Scale(this Polygon segments, float factor)
    {
        var newPoly = new Polygon();
        foreach (var seg in segments) {
            newPoly.Add(new Segment() {
                a = seg.a * factor,
                b = seg.b * factor
            });
        }
        segments = newPoly;
    }

    public static Polygon ToPolygon(this Site site)
    {
        return site.Edges.ToSegments();
    }

    public static bool IsPointInBounds(this Vector2 point, Vector2[] bounds)
    {
        return point.x > bounds[0].x
            && point.x < bounds[1].x
            && point.x > bounds[0].y
            && point.x < bounds[1].y;
    }

    public static Polygon ToSegments(this List<Edge> edges)
    {
        var segs = new Polygon();
        foreach (Edge edge in edges) {
            if (edge.ClippedEnds == null) continue;
            var a = edge.ClippedEnds[LR.LEFT].ToVector2();
            var b = edge.ClippedEnds[LR.RIGHT].ToVector2();
            segs.Add(new Segment { a = a, b = b });
        }
        return segs;
    }

    public static Segment ToSegment(this Edge edge)
    {
        var a = edge.ClippedEnds[LR.LEFT].ToVector2();
        var b = edge.ClippedEnds[LR.RIGHT].ToVector2();
        return new Segment { a = a, b = b };
    }

    public static bool IsInTriangle(this Vector2 point, Vector2[] triangle)
    {
        for (var i = 0; i < triangle.Length; i++) {
            var a = triangle[i];
            var b = triangle[(i + 1) % triangle.Length];
            var c = triangle[(i + 2) % triangle.Length];

            var side = Mathf.Sign((b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x));
            var cSide = Mathf.Sign((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x));

            if (side != cSide) return false;
        }

        return true;
    }
    
    public static Vector2 ToVector2(this Vector2f f)
    {
        return new Vector2(f.x, f.y);
    }

    public static bool ApproximativelyEquals(float a, float b, int by = 100)
    {
        return (
            Mathf.Round(a * by) / by
            ==
            Mathf.Round(b * by) / by
        );
    }
}
