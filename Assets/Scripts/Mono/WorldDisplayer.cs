using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class WorldDisplayer : MonoBehaviour
{

    public Color backgroundColor = Color.black;
    public Color foregroundColor = Color.white;
    public Color capitalColor = Color.yellow;
    public float jitterForce = 1;
    public float jitterCutEvery = 3;
    public float brushSize = 1f;
    public float capitalSize = 1.5f;
    public DisplayMode mode;

    public TextMeshProUGUI debugText;

    public enum DisplayMode { REGION, TERRITORY, KINGDOM};

    RawImage image;
    bool shouldExtraDraw = false;
    Texture2D tx;


    private void Start()
    {
        tx = new Texture2D(1, 1);
        image = GetComponent<RawImage>();
    }

    // Here is a very simple way to display the result using a simple bresenham line algorithm
    // Just attach this script to a quad
    public void DrawMap(Map map, int resolution)
    {
        if (image == null) { return; }

        tx.Resize(resolution, resolution);
        shouldExtraDraw = !shouldExtraDraw;


        // Fill background
        var pixels = tx.GetPixels();
        for (var i = 0; i < pixels.Length; ++i)
        {
            pixels[i] = backgroundColor;
        }
        tx.SetPixels(pixels);


        // Regions
        int draws = 0;
        if (mode == DisplayMode.REGION)
        {
            draws += DrawRegions(map.regions);
        }

        if (mode == DisplayMode.KINGDOM)
        {
            draws += DrawRegions(map.regions);
            draws += DrawKingdoms(map.world.kingdoms);
        }
       

        tx.Apply();
        image.texture = tx;

        debugText.text = "Regions : " + map.regions.Count + "\n" +
            "Drawn edges : " + draws + "\n" +
            "Kingdoms : " + map.world.kingdoms.Count;

    }

    int DrawKingdoms(List<Kingdom> kingdoms)
    {
        int draws = 0;
        foreach (Kingdom kingdom in kingdoms)
        {
            List<Edge> frontiers = kingdom.GetFrontiers();

            foreach (Region region in kingdom.GetTerritory())
            {
                foreach (Site site in region.sites)
                {
                    StarFillSite(kingdom.GetColor(), site);

                    foreach(Edge edge in site.Edges)
                    {
                        if (frontiers.Contains(edge))
                        {
                            DrawEdge(edge, kingdom.GetColor());
                        }
                        else
                        {
                            DrawEdge(edge, Color.black);
                            DrawEdge(edge, kingdom.GetColor(), true, brushSize/2f);
                        }
                    }
                }
            }

            // Capital
            Region mainland = kingdom.GetMainland();
            Vector2f capitalCoords = new Vector2f((int)mainland.sites[mainland.capital].Coord.x, (int)mainland.sites[mainland.capital].Coord.y);
            DrawCircle(capitalCoords, tx, backgroundColor, brushSize * capitalSize);
            DrawCircle(capitalCoords, tx, capitalColor, brushSize * capitalSize-2);

        }

        return draws;
    }


    // Filling by tracing rays to the center
    void StarFillSite(Color c, Site site)
    {
        foreach(Edge edge in site.Edges)
        {
            var jittered = JitterLine(edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT], 2, 0);
            foreach (KeyValuePair<Vector2f, Vector2f> pair in jittered)
            {
                DrawLine(
                    pair.Key,
                    site.Coord,
                    tx,
                    c
                );
            }
        }
    }

    int DrawRegions(List<Region> regions)
    {
        List<Edge> drawnEdges = new List<Edge>();
        int draws = 0;
        foreach (Region region in regions)
        {
            // Boundaries
            foreach (Edge edge in region.GetOuterEdges())
            {
                if (drawnEdges.Contains(edge))
                {
                    continue;
                }

                DrawEdge(edge, foregroundColor);
                drawnEdges.Add(edge);
                draws++;
            }
        }

        return draws;
    }

    void DrawEdge(Edge edge, Color c, bool isDotted=false, float dotWeight=1f)
    {
        // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
        if (edge.ClippedEnds == null) return;
        float weight = brushSize;
        if (isDotted)
        {
            weight = dotWeight;
        }
        DrawJitteredLine(edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT], tx, c, weight, isDotted);
    }

    void DrawJitteredLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, float brushSize, bool isDotted=false)
    {
        bool dot = false;
        var jittered = JitterLine(p0, p1, jitterCutEvery, jitterForce);

        if (isDotted)
        {// Straight
            jittered = JitterLine(p0, p1, jitterCutEvery/4f, 0);
        }

        foreach (KeyValuePair<Vector2f, Vector2f> line in jittered)
        {
            dot = !dot;
            if (!isDotted || dot) { 
                DrawLine(line.Key, line.Value, tx, c, brushSize);
            }
        }
    }


    // Bresenham line algorithm
    static void DrawLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, float brushSize=1f)
    {
        int x0 = (int)p0.x;
        int y0 = (int)p0.y;
        int x1 = (int)p1.x;
        int y1 = (int)p1.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (brushSize > 0)
            {
                DrawCircle(new Vector2f(x0, y0), tx, c, brushSize/2);
            }
            else
            {
                tx.SetPixel(x0, y0, c);
            }

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    static void DrawCircle(Vector2f p0, Texture2D tx, Color c, float r=1f)
    {
        int radius = Mathf.RoundToInt(r);
        for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                if (x * x + y * y <= radius * radius)
                    tx.SetPixel(Mathf.RoundToInt(p0.x + x), Mathf.RoundToInt(p0.y + y), c);
        
    }


    static List<KeyValuePair<Vector2f, Vector2f>> JitterLine(Vector2f origin, Vector2f destination, float jitterCutEvery, float jitterForce)
    {
        System.Random localRandom = new System.Random(1);

        var segments = new List<KeyValuePair<Vector2f, Vector2f>>();
        var segmentAmount = Mathf.CeilToInt(Vector2f.DistanceSquare(origin, destination)/ jitterCutEvery);

        List<Vector2> newPositions = new List<Vector2>();

        // Breaking down into multiple points
        for (int i = 0; i < segmentAmount + 1; i++)
        {
            Vector2 point =
                Vector2.Lerp(
                    new Vector2(origin.x, origin.y),
                    new Vector2(destination.x, destination.y),
                    ((float)i) / segmentAmount
                );

            newPositions.Add(point);
        }

        // Recomposing segments - but with jitter
        Vector2 lastPosition = new Vector2();
        bool isFirstLoop = true;
        foreach (Vector2 position in newPositions)
        {
            if (isFirstLoop)
            {
                lastPosition = new Vector2(origin.x, origin.y);
                isFirstLoop = false;
                continue;
            }

            Vector2 newPosition = position;

            newPosition += new Vector2((float)localRandom.NextDouble() * 2f - 1, (float)localRandom.NextDouble() * 2f - 1) * jitterForce;
            
            segments.Add(new KeyValuePair<Vector2f, Vector2f>(
                new Vector2f(lastPosition.x, lastPosition.y),
                new Vector2f(newPosition.x, newPosition.y)
            ));
            lastPosition = newPosition;
        }

        return segments;
    }
}
