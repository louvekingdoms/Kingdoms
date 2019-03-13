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
    public RawImage regionsLayer;
    public GameObject childExample;

    public enum DisplayMode { REGION, TERRITORY, KINGDOM};

    bool shouldExtraDraw = false;
    int resolution = 1;
    Texture2D bgTex;
    Dictionary<Site, RawImage> sitesChildren;
    float scaleFactor;


    private void Start()
    {
        bgTex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        bgTex.mipMapBias = 0f;
        bgTex.filterMode = FilterMode.Point;
        scaleFactor = transform.parent.GetComponent<RectTransform>().sizeDelta.x + GetComponent<RectTransform>().offsetMax.y - GetComponent<RectTransform>().offsetMin.y;
        sitesChildren = new Dictionary<Site, RawImage>();
    }

    // Here is a very simple way to display the result using a simple bresenham line algorithm
    // Just attach this script to a quad
    public void DrawMap(Map map, int _resolution)
    {
        if (regionsLayer == null) { return; }

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
            DrawKingdoms(map.world.kingdoms);
            DrawRegions(map.regions);
        }


        bgTex.Apply();
        regionsLayer.texture = bgTex;

        debugText.text = "Regions : " + map.regions.Count + "\n" +
            "Kingdoms : " + map.world.kingdoms.Count;

    }

    void DrawKingdoms(List<Kingdom> kingdoms)
    {
        foreach (Kingdom kingdom in kingdoms)
        {

            DrawKingdom(kingdom);

        }
    }


    void DrawKingdom(Kingdom kingdom)
    {

        // Frontiers
        List <Edge> frontiers = kingdom.GetFrontiers();
        foreach (Region region in kingdom.GetTerritory())
        {
            foreach (Site site in region.sites)
            {
                var image = GetSiteImage(site);
                FillSite(kingdom.GetColor(), site);
                /*
                Texture2D tx = image.mainTexture;
                foreach (Edge edge in site.Edges)
                {
                    if (frontiers.Contains(edge))
                    {
                        DrawEdge(edge, tx, foregroundColor);
                    }
                    else
                    {
                        DrawEdge(edge, tx, Color.black);
                        DrawEdge(edge, tx, kingdom.GetColor(), true, brushSize / 2f);
                    }
                }
                */
            }
        }

        // Capital
        Region mainland = kingdom.GetMainland();
        Site capitalSite = mainland.sites[mainland.capital];

        var img = GetSiteImage(capitalSite);
        Texture2D tex = (Texture2D)img.mainTexture;

        Vector2f capitalCoords = new Vector2f(tex.width/2, tex.height/2);
        DrawCircle(capitalCoords, tex, backgroundColor, brushSize * capitalSize);
        DrawCircle(capitalCoords, tex, capitalColor, brushSize * capitalSize-2);

        // Capital name
        var text = img.GetComponentInChildren<TextMeshProUGUI>();
        
        if (text != null)
        {
            text.text = kingdom.name;
        }

        tex.Apply();
    }

    RawImage GetSiteImage(Site site)
    {
        Texture2D tex;

        // 1) Trapping the site in a square
        Vector2[] square =
            new Vector2[2] {
                new Vector2(site.Coord.x, site.Coord.y), //-X -Y
                new Vector2(site.Coord.x, site.Coord.y) // X Y
            };

        foreach (Edge edge in site.Edges)
        {
            if (edge.ClippedEnds == null)
            {
                continue;
            }
            List<Vector2f> points = new List<Vector2f>() { edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT] };
            foreach (Vector2f point in points)
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

        if (!sitesChildren.ContainsKey(site))
        {
            float factor = (scaleFactor / resolution);
            GameObject siteO = Instantiate(childExample, transform);
            siteO.SetActive(true);
            RectTransform rt = siteO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(square[1].x - square[0].x, square[1].y - square[0].y) * factor;
            rt.anchoredPosition = new Vector2(site.Coord.x * factor, site.Coord.y * factor);
            sitesChildren[site] = siteO.GetComponent<RawImage>();
            tex = new Texture2D(Mathf.RoundToInt(square[1].x - square[0].x), Mathf.RoundToInt(square[1].y - square[0].y));
            tex.mipMapBias = 0f;
            tex.filterMode = FilterMode.Point;
        }
        else
        {
            tex = (Texture2D)sitesChildren[site].texture;
        }

        ri = sitesChildren[site];
        ri.texture = tex;

        return ri;
    }

    // Filling by tracing rays to the center
    void FillSite(Color c, Site site)
    {

        RawImage ri = GetSiteImage(site);
        Texture2D tex = (Texture2D)ri.mainTexture;

        // Fill background
        var pixels = tex.GetPixels();
        for (var i = 0; i < pixels.Length; ++i)
        {
            pixels[i] = new Color(1, 0, 1, 0);
        }
        tex.SetPixels(pixels);

        DrawSite(tex, site, c);

        // Fill the site
        Vector2[] squareCoords =
            new Vector2[2] {
                new Vector2(siteTextureMargin, siteTextureMargin), //-X -Y
                new Vector2(tex.width-siteTextureMargin, tex.height-siteTextureMargin) // X Y
            };

        for (int i = 0; i < 11; i++)
        {
            DrawSite(tex, site, c, 0.1f*i, 2f);
        }

        DrawSite(tex, site, backgroundColor, 1, 1);

        tex.Apply();

    }

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
        regionsLayer.transform.SetSiblingIndex(regionsLayer.transform.parent.childCount - 1);
    }

    void DrawSite(Texture2D tex, Site site, Color c, float factor=1f, float brushSize=1f)
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

            var left = edge.ClippedEnds[LR.LEFT] - position;
            var right = edge.ClippedEnds[LR.RIGHT] - position;
            left = new Vector2f(left.x * factor, left.y * factor);
            right = new Vector2f(right.x * factor, right.y * factor);

            DrawLine(left + new Vector2f(center.x, center.y), right + new Vector2f(center.x, center.y), tex, c, brushSize);
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
