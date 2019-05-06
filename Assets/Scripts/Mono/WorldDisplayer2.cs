using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using csDelaunay;

public class WorldDisplayer2 : MonoBehaviour
{
    public RawImage regionsLayer;
    [Range(0f, 1024f)] public int regionDPI = 1024;
    public DisplayMode mode;
    [Range(0f, 100f)] public float mapSize = 10f;
    [Range(0f, 1000f)] public float regionScale = 1f;
    public Material strokeMaterial;
    public enum DisplayMode { REGION, TERRITORY, KINGDOM };

    public int drawRegion;

    Dictionary<Site, GameObject> sitesGameObjects = new Dictionary<Site, GameObject>();

    public void DrawMap(Map map)
    {
        UpdateLayerTexture();

        // Regions
        if (mode == DisplayMode.REGION)
        {
            DrawRegions(map.regions);
        }
        
        // Kingdoms
        if (mode == DisplayMode.KINGDOM)
        {
            DrawKingdoms(map.world.kingdoms);
            DrawRegions(map.regions);
            //DrawKingdomsTags(map.world.kingdoms);
        }
    }

    void UpdateLayerTexture()
    {
        if (regionsLayer.texture) Destroy(regionsLayer.texture);

        var size = regionsLayer.transform.parent.GetComponent<RectTransform>().sizeDelta.x+regionsLayer.GetComponent<RectTransform>().sizeDelta.x;
        var sizei = Mathf.RoundToInt(size);
        
        regionsLayer.texture = new Texture2D(sizei, sizei);
    }

    void DrawRegions(List<Region> regions)
    {
        var i = 0;
        foreach (var region in regions)
        {
            foreach (var site in region.sites) {
                i++;
                //if (i > -1 && i != drawRegion) continue;
                DrawSite(site, Color.red);
            }
        }
    }
    
    void DrawKingdoms(List<Kingdom> kingdoms)
    {
        /*
        foreach (var region in regions)
        {
            foreach (var site in region.sites)
            {
                DrawSite(site, Color.black);
            }
        }
        */
    }

    void DrawSite(Site site, Color color, float scale=1f)
    {
        var position = ToVector2(site.Coord);

        var tex = ((Texture2D)regionsLayer.mainTexture);

        tex.SetPencilColor(Color.green);

        var UVs = new List<Pencil.Segment>();

        var segs = SiteSegments(site);
        foreach (var seg in segs) {
            UVs.Add(seg);
        }

        tex.SetPencilColor(color);
        tex.Lines(UVs, 2f);

        tex.Apply();


        /*
        var image = g.GetComponent<RawImage>();
        var size = regionsLayer.transform.parent.GetComponent<RectTransform>().sizeDelta.x;

        var dim = SiteDimensions(site);
        var bounds = SiteBounds(site);

        var tex = ((Texture2D)image.mainTexture);
        tex.SetPencilColor(Color.yellow);
        tex.Circle(new Vector2(0.5f, 0.5f), 1f);

        tex.SetPencilColor(color);
        var UVs = new List<Vector2>();
        foreach(var point in SitePoints(site)) {
            UVs.Add((point - bounds[0]) / dim);
        }
        tex.Polygon(UVs, 3f);
        tex.Apply();
        
        // Set position and size
        var rect = g.GetComponent<RectTransform>();
        rect.anchoredPosition = position*size-new Vector2(size/2, size/2);
        rect.sizeDelta = (dim)* regionScale;
        */
    }

    /*
    void ShapeSprite(List<Vector2> points, Vector2 position, Sprite sprite)
    {
        List<ushort> triangles = new List<ushort>();
        for (int i = 1; i < points.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add((ushort)(i));
            triangles.Add((ushort)(i + 1));
        }
        triangles.Add(0);
        triangles.Add((ushort)(points.Count - 1));
        triangles.Add((ushort)(1));

        sprite.OverrideGeometry(points.ToArray(), triangles.ToArray()); // set the vertices and triangles

    }
    */

    public static Vector2 ToVector2(Vector2f f)
    {
        return new Vector2(f.x, f.y);
    }
    
    Vector2 SiteDimensions(Site site)
    {
        var bounds = SiteBounds(site);
        var dim = bounds[1] - bounds[0];
        return dim;
    }

    Vector2[] SiteBounds(Site site)
    {
        var lowest = new Vector2(Mathf.Infinity, Mathf.Infinity);
        var highest = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);

        /*
        foreach (var point in SitePoints(site)) {
            if (lowest.x > point.x) lowest.x = point.x;
            if (lowest.y > point.y) lowest.y = point.y;
            if (highest.x < point.x) highest.x = point.x;
            if (highest.y < point.y) highest.y = point.y;
        }
        */

        return new Vector2[] { lowest, highest };
    }

    List<Pencil.Segment> SiteSegments(Site site)
    {
        // generate vertices list
        var segs = new List<Pencil.Segment>();
        foreach (Edge edge in site.Edges) {
            if (edge.ClippedEnds == null) continue;
            var a = ToVector2(edge.ClippedEnds[LR.LEFT]);
            var b = ToVector2(edge.ClippedEnds[LR.RIGHT]);
            segs.Add(new Pencil.Segment { a = a, b = b });
        }

        return segs;
    }
}
