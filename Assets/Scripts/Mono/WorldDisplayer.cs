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
    public int regionResolution = 256;
    public DisplayMode mode;
    public enum DisplayMode { REGION, TERRITORY, KINGDOM };

    public TextMeshProUGUI debugText;
    public GameObject regionsLayer;
    public GameObject kingdomsLayer;
    public GameObject tagsLayer;
    public GameObject siteImageExample;
    public GameObject siteTextExample;


    bool shouldExtraDraw = false;
    Dictionary<Site, GameObject> sitesGameObjects = new Dictionary<Site, GameObject>();
    Dictionary<Site, TextMeshProUGUI> sitesTags = new Dictionary<Site, TextMeshProUGUI>();
    float scaleFactor;

    /*
    private void Start()
    {
        scaleFactor = transform.parent.GetComponent<RectTransform>().sizeDelta.x + GetComponent<RectTransform>().offsetMax.y - GetComponent<RectTransform>().offsetMin.y;
        sitesChildren = new Dictionary<Site, RawImage>();
        sitesTags = new Dictionary<Site, TextMeshProUGUI>();
    }

    // Here is a very simple way to display the result using a simple bresenham line algorithm
    // Just attach this script to a quad
    */
    public void DrawMap(Map map)
    {
        shouldExtraDraw = !shouldExtraDraw;
        
        
        // Regions
        if (mode == DisplayMode.REGION)
        {
            DrawRegions(map.regions);
        }

        /*
        if (mode == DisplayMode.KINGDOM)
        {
            DrawKingdoms(map.world.kingdoms);
            DrawRegions(map.regions);
            DrawKingdomsTags(map.world.kingdoms);
        }
        */
        
        debugText.text = "Regions : " + map.regions.Count + "\n" +
            "Kingdoms : " + map.world.kingdoms.Count;

    }
    /*

    void DrawKingdomsTags(List<Kingdom> kingdoms)
    {
        //sitesTags
        foreach (Kingdom kingdom in kingdoms)
        {
            DrawKingdomTag(kingdom);
        }
        tagsLayer.transform.SetSiblingIndex(regionsLayer.transform.parent.childCount - 1);
    }

    void DrawKingdomTag(Kingdom kingdom)
    {

        Region mainland = kingdom.GetMainland();
        Site capitalSite = mainland.sites[mainland.capital];

        var img = GetSiteTag(capitalSite);

        // Capital name
        var text = img.GetComponentInChildren<TextMeshProUGUI>();
        
        if (text != null)
        {
            text.text = kingdom.name;
        }
    }

    void DrawKingdoms(List<Kingdom> kingdoms)
    {
        foreach (Kingdom kingdom in kingdoms)
        {
            DrawKingdom(kingdom);
        }
        kingdomsLayer.transform.SetSiblingIndex(regionsLayer.transform.parent.childCount - 1);
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
                /*
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
        var tag = GetSiteTag(capitalSite);
        tag.text = kingdom.name;

        tex.Apply();
    }

        */
    void DrawRegion(Region region)
    {

        var allowedEdges = region.GetFrontiers();
        var r = GetComponent<RectTransform>().rect;
        // Frontiers
        foreach (Site site in region.sites)
        {
            var img = GetSiteObject(site, regionsLayer.transform);
            var spr = img.GetComponent<Image>().sprite;
            DrawSite(spr, site, Color.white);
            img.GetComponent<RectTransform>().anchoredPosition = ToVector2(site.Coord) * new Vector2(r.width, r.height) - new Vector2(r.width/2, r.height/2);
        }
    }

    /*
    TextMeshProUGUI GetSiteTag(Site site)
    {
        float factor = (scaleFactor / resolution);
        if (!sitesTags.ContainsKey(site))
        {
            GameObject siteO = Instantiate(siteTextExample, siteTextExample.transform.parent);
            siteO.SetActive(true);
            var rt = siteO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(site.Coord.x * factor, site.Coord.y * factor);
            sitesTags[site] = siteO.GetComponentInChildren<TextMeshProUGUI>();
        }

        return sitesTags[site];
    }
    */
    GameObject GetSiteObject(Site site, Transform parent)
    {
        if (sitesGameObjects.ContainsKey(site)) {
            return sitesGameObjects[site];
        }
        else {
            var gameO = new GameObject();
            gameO.transform.parent = parent;
            var sr = gameO.AddComponent<Image>(); // add a sprite renderer
            var texture = new Texture2D(regionResolution+1, regionResolution+1); // create a texture larger than your maximum polygon size
            var color = new Color(0f, 1f, 0f, 1f);
                                                                                       
            List<Color> cols = new List<Color>();// create an array and fill the texture with your color
            for (int i = 0; i < (texture.width * texture.height); i++)
                cols.Add(color);
            texture.SetPixels(cols.ToArray());
            texture.Apply();

            sr.color = color; //you can also add that color to the sprite renderer

            sr.sprite = Sprite.Create(texture, new Rect(0, 0, regionResolution, regionResolution), Vector2.zero, 1); //create a sprite with the texture we just created and colored in

            return gameO;
        }


        /*
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

            if (parent == null) parent = siteImageExample.transform.parent;

            GameObject siteO = Instantiate(siteImageExample, parent);
            siteO.SetActive(true);
            RectTransform rt = siteO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(square[1].x - square[0].x, square[1].y - square[0].y) * factor;
            rt.anchoredPosition = new Vector2(site.Coord.x * factor, site.Coord.y * factor);
            sitesChildren[site] = siteO.GetComponent<RawImage>();
            tex = new Texture2D(Mathf.RoundToInt(square[1].x - square[0].x), Mathf.RoundToInt(square[1].y - square[0].y), TextureFormat.RGBA32, true);
            tex.mipMapBias = 0f;
            tex.filterMode = FilterMode.Point;
            tex.Apply();
        }
        else
        {
            tex = (Texture2D)sitesChildren[site].texture;
        }

        ri = sitesChildren[site];
        ri.texture = tex;

        return ri;
        */
    }
    /*
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
    */
    void DrawRegions(List<Region> regions)
    {
        //List<Edge> drawnEdges = new List<Edge>();
        foreach (Region region in regions)
        {

            DrawRegion(region);
            /*
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
            */
        }
        regionsLayer.transform.SetSiblingIndex(regionsLayer.transform.parent.childCount - 1);
    }

    public static Vector2 ToVector2(Vector2f f)
    {
        return new Vector2(f.x, f.y);
    }

    // Outlines the site
    void DrawSite(Sprite sprite, Site site, Color c, float scale=1f)
    {
        // generate vertices list
        List<Vector2> points = new List<Vector2>();

        /*
        foreach(Edge edge in site.Edges) {
            if (edge.ClippedEnds == null) continue;
            var left = edge.ClippedEnds[LR.LEFT];
            var right = edge.ClippedEnds[LR.RIGHT];
            site.
            points.Add(ToVector2(left));
            points.Add(ToVector2(right));
        }
        */

        List<ushort> triangles = new List<ushort>();
        for (int i = 1; i < points.Count - 1; i++) {
            triangles.Add(0);
            triangles.Add((ushort)(i));
            triangles.Add((ushort)(i + 1));
        }
        triangles.Add(0);
        triangles.Add((ushort)(points.Count - 1));
        triangles.Add((ushort)(1));

        points = new List<Vector2>() { new Vector2(1, 1), new Vector2(.05f, 3.3f), new Vector2(1, 2), new Vector2(1.95f, 1.3f), new Vector2(1.58f, 0.2f), new Vector2(.4f, .2f) };


        //convert coordinates to local space

        Vector2[] localv = new Vector2[points.Count];
        for (int i = 0; i < points.Count; i++) {
            localv[i] = points[i] - ToVector2(site.Coord) + new Vector2(regionResolution / 2, regionResolution / 2);
            localv[i] /= scale;
        }

        var stringData = "";
        foreach(var point in points) {
            stringData += point+"\n";
        }
        stringData += "-----\n";
        foreach (var s in triangles) {
            stringData += s + "\n";
        }
        sprite.name = stringData;
        sprite.OverrideGeometry(localv, triangles.ToArray()); // set the vertices and triangles        

        /*
        RectTransform rt = siteO.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(site.Coord.x * factor, site.Coord.y * factor);
        */
        /*
        Vector2[] square =
            new Vector2[2] {
                new Vector2(siteTextureMargin, siteTextureMargin), //-X -Y
                new Vector2(tex.width-siteTextureMargin, tex.height-siteTextureMargin) // X Y
            };

        var center = Vector2.Lerp(square[0], square[1], 0.5f);

        foreach (Edge edge in site.Edges) {
            if (edge.ClippedEnds == null) continue;
            if (ignoreEdges != null && ignoreEdges.Contains(edge)) continue;

            var position = site.Coord;

            var left = edge.ClippedEnds[LR.LEFT] - position;
            var right = edge.ClippedEnds[LR.RIGHT] - position;
            left = new Vector2f(left.x * factor, left.y * factor);
            right = new Vector2f(right.x * factor, right.y * factor);

            DrawLine(left + new Vector2f(center.x, center.y), right + new Vector2f(center.x, center.y), tex, c, brushSize);
        }
        */


    }
}
