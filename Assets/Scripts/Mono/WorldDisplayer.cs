using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class WorldDisplayer : MonoBehaviour
{

    public Color backgroundColor = Color.black;
    public Color foregroundColor = Color.white;
    public Color capitalColor = Color.yellow;
    public float jitterForce = 1;
    public float jitterCutEvery = 3;
    public float brushSize = 1f;
    public float capitalSize = 1.5f;
    public float siteTextureMargin = 50f;
    public DisplayMode mode;

    public TextMeshProUGUI debugText;
    public GameObject childExample;

    public enum DisplayMode { REGION, TERRITORY, KINGDOM};

    RawImage image;
    bool shouldExtraDraw = false;
    int resolution = 1;
    Texture2D bgTex;
    Dictionary<Site, RawImage> sitesChildren;
    float scaleFactor;


    private void Start()
    {
        bgTex = new Texture2D(resolution, resolution);
        image = GetComponent<RawImage>();
        scaleFactor = transform.parent.GetComponent<RectTransform>().sizeDelta.x + GetComponent<RectTransform>().offsetMax.y - GetComponent<RectTransform>().offsetMin.y;
        sitesChildren = new Dictionary<Site, RawImage>();
    }

    // Here is a very simple way to display the result using a simple bresenham line algorithm
    // Just attach this script to a quad
    public void DrawMap(Map map, int _resolution)
    {
        if (image == null) { return; }

        resolution = _resolution;
        bgTex.Resize(resolution, resolution);
        shouldExtraDraw = !shouldExtraDraw;
        

        // Fill background
        var pixels = bgTex.GetPixels();
        for (var i = 0; i < pixels.Length; ++i)
        {
            pixels[i] = backgroundColor;
        }
        bgTex.SetPixels(pixels);


        // Regions
        if (mode == DisplayMode.REGION)
        {
            DrawRegions(map.regions);
        }

        if (mode == DisplayMode.KINGDOM)
        {
            DrawRegions(map.regions);
            DrawKingdoms(map.world.kingdoms);
        }


        bgTex.Apply();
        image.texture = bgTex;

        debugText.text = "Regions : " + map.regions.Count + "\n" +
            "Kingdoms : " + map.world.kingdoms.Count;

    }

    void DrawKingdoms(List<Kingdom> kingdoms)
    {
        foreach (Kingdom kingdom in kingdoms)
        {
            List<Edge> frontiers = kingdom.GetFrontiers();

            foreach (Region region in kingdom.GetTerritory())
            {
                foreach (Site site in region.sites)
                {
                    print("Filling " + site + " for kingdom " + kingdom.name);
                    FillSite(kingdom.GetColor(), site);
                    /*
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
                    */
                }
            }

            // Capital
            /*
            Region mainland = kingdom.GetMainland();
            Vector2f capitalCoords = new Vector2f((int)mainland.sites[mainland.capital].Coord.x, (int)mainland.sites[mainland.capital].Coord.y);
            DrawCircle(capitalCoords, tx, backgroundColor, brushSize * capitalSize);
            DrawCircle(capitalCoords, tx, capitalColor, brushSize * capitalSize-2);
            */

        }
    }


    // Filling by tracing rays to the center
    void FillSite(Color c, Site site)
    {
        // 1) Trapping the site in a square
        Vector2[] square =
            new Vector2[2] { 
                new Vector2(site.Coord.x, site.Coord.y), //-X -Y
                new Vector2(site.Coord.x, site.Coord.y) // X Y
            };

        foreach (Edge edge in site.Edges)
        {
            List<Vector2f> points = new List<Vector2f>() { edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT] };
            foreach(Vector2f point in points)
            {
                square[0].x = Mathf.Min(square[0].x, point.x);
                square[0].y = Mathf.Min(square[0].y, point.y);
                square[1].x = Mathf.Max(square[1].x, point.x);
                square[1].y = Mathf.Max(square[1].y, point.y);
            }
        }
        
        // Applying margin
        square[0].x = square[0].x - siteTextureMargin;
        square[0].y = square[0].y - siteTextureMargin;
        square[1].x = square[1].x + siteTextureMargin;
        square[1].y = square[1].y + siteTextureMargin;

        RawImage ri;
        Texture2D tex;
        
        if (!sitesChildren.ContainsKey(site))
        {
            float factor = (scaleFactor / resolution);
            GameObject siteO = Instantiate(childExample, transform);
            siteO.SetActive(true);
            RectTransform rt = siteO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(square[1].x - square[0].x, square[1].y - square[0].y)* factor;
            rt.anchoredPosition = new Vector2(site.Coord.x * factor, site.Coord.y * factor);
            sitesChildren[site] = siteO.GetComponent<RawImage>();
            tex = new Texture2D(Mathf.RoundToInt(square[1].x - square[0].x), Mathf.RoundToInt(square[1].y - square[0].y));
        }
        else
        {
            tex = (Texture2D)sitesChildren[site].texture;
        }
        ri = sitesChildren[site];
        
        // Fill background
        var pixels = tex.GetPixels();
        for (var i = 0; i < pixels.Length; ++i)
        {
            pixels[i] = new Color(1, 0, 1, 0);
        }
        tex.SetPixels(pixels);

        DrawSite(tex, site, c);
        
        // Fill the site
        tex.Apply();
        for (int i = 0; i < tex.height; i++)
        {
            DrawScanline(new Vector2f(0f, i), new Vector2f((float)tex.width, i), tex, c);
        }

        tex.Apply();
        ri.texture = tex;
        /*
        Vector2Int startingCoord = new Vector2Int(Mathf.RoundToInt(site.Coord.x), Mathf.RoundToInt(site.Coord.y));

        FloodFill(startingCoord, t, c);
        */

    }
    /*
    void FillSite(Color c, Site site, Color t)
    {
        Vector2Int startingCoord = new Vector2Int(Mathf.RoundToInt(site.Coord.x), Mathf.RoundToInt(site.Coord.y));

        FloodFill(startingCoord, t, c);
        
    }
    */

    static void FillRectangle(Vector2[] square, Texture2D tx, Color c)
    {

        for (float i = square[0].y; i < square[1].y; i++)
        {
            DrawLine(new Vector2f(square[0].x, Mathf.RoundToInt(i)), new Vector2f(square[1].x, Mathf.RoundToInt(i)), tx, c);
        }
    }
    

    void DrawRegions(List<Region> regions)
    {
        List<Edge> drawnEdges = new List<Edge>();
        foreach (Region region in regions)
        {
            // Boundaries
            foreach (Edge edge in region.GetOuterEdges())
            {
                if (drawnEdges.Contains(edge))
                {
                    continue;
                }

                DrawEdge(edge, bgTex, foregroundColor);
                drawnEdges.Add(edge);
            }
        }
    }

    void DrawSite(Texture2D tex, Site site, Color c)
    {
        Vector2[] square =
            new Vector2[2] {
                new Vector2(siteTextureMargin, siteTextureMargin), //-X -Y
                new Vector2(tex.width-siteTextureMargin, tex.height-siteTextureMargin) // X Y
            };
        
        var center = Vector2.Lerp(square[0], square[1], 0.5f);

        foreach (Edge edge in site.Edges)
        {
            if (edge.ClippedEnds == null) continue;

            var position = site.Coord;

            var left = edge.ClippedEnds[LR.LEFT] - position + new Vector2f(center.x, center.y);
            var right = edge.ClippedEnds[LR.RIGHT] - position + new Vector2f(center.x, center.y);

            DrawLine(left, right, tex, c);
        }
    }

    void DrawEdge(Edge edge, Texture2D tx, Color c, bool isDotted=false, float dotWeight=1f)
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

    static void DrawScanline(Vector2f p0, Vector2f p1, Texture2D tx, Color c)
    {
        bool shouldFill = false;

        var pixels = GetPixelsOnLine(p0, p1);
        foreach(var pixel in pixels)
        {
            var iPixel = new Vector2Int(Mathf.RoundToInt(pixel.x), Mathf.RoundToInt(pixel.y));
            var currentColor = tx.GetPixel(iPixel.x, iPixel.y);

            if (shouldFill)
            {
                if (currentColor.a > 0)
                {
                    shouldFill = false;
                }
                else
                {
                    tx.SetPixel(iPixel.x, iPixel.y, c);
                }
            }
            else
            {
                if (currentColor.a > 0)
                {
                    //print("SHOULD FILL");
                    shouldFill = true;
                }
            }
        }
    }


    // Bresenham line algorithm
    static void DrawLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, float brushSize=1f)
    {
        var pixels = GetPixelsOnLine(p0, p1);

        foreach (Vector2f px in pixels)
        {
            if (brushSize > 0)
            {
                DrawCircle(new Vector2f(px.x, px.y), tx, c, brushSize / 2);
            }
            else
            {
                tx.SetPixel(Mathf.RoundToInt(px.x), Mathf.RoundToInt(px.y), c);
            }
        }
    }

    static List<Vector2f> GetPixelsOnLine(Vector2f p0, Vector2f p1)
    {
        List<Vector2f> pixels = new List<Vector2f>();

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
            pixels.Add(new Vector2f(x0, y0));

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

        return pixels;
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
