using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using csDelaunay;

public class WorldDisplayer2 : MonoBehaviour
{
    public GameObject regionsLayer;
    [Range(0f, 1024f)] public int regionDPI = 1024;
    public DisplayMode mode;
    [Range(0f, 100f)] public float mapSize = 10f;
    [Range(0f, 1000f)] public float regionScale = 1f;
    public Material strokeMaterial;
    public enum DisplayMode { REGION, TERRITORY, KINGDOM };


    Dictionary<Site, GameObject> sitesGameObjects = new Dictionary<Site, GameObject>();

    public void DrawMap(Map map)
    {


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

    void DrawRegions(List<Region> regions)
    {
        foreach (var region in regions)
        {
            foreach (var site in region.sites)
            {
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
        var g = GetSiteObject(site, regionsLayer.transform);

        var position = ToVector2(site.Coord);
        var image = g.GetComponent<RawImage>();
        var size = regionsLayer.transform.parent.GetComponent<RectTransform>().sizeDelta.x;

        var dim = SiteDimensions(site);
        var bounds = SiteBounds(site);

        var tex = ((Texture2D)image.mainTexture);
        tex.SetPencilColor(Color.yellow);
        tex.Circle(new Vector2(0.5f, 0.5f), 1f);

        tex.SetPencilColor(color);
        var UVs = new List<Vector2>();
        foreach(var point in site.Points()) {
            UVs.Add((point - bounds[0]) / dim);
        }
        tex.Polygon(UVs, 3f);
        tex.Apply();
        
        // Set position and size
        var rect = g.GetComponent<RectTransform>();
        rect.anchoredPosition = position*size-new Vector2(size/2, size/2);
        rect.sizeDelta = (dim)* regionScale;
    }

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

    public static Vector2 ToVector2(Vector2f f)
    {
        return new Vector2(f.x, f.y);
    }
    
    // Pooling
    GameObject GetSiteObject(Site site, Transform parent)
    {
        if (sitesGameObjects.ContainsKey(site)) {
            return sitesGameObjects[site];
        }

        var gameO = new GameObject();
        gameO.transform.parent = parent;
        var image = gameO.AddComponent<RawImage>(); // add a sprite renderer
        var dim = SiteDimensions(site);

        var texture = new Texture2D(Mathf.RoundToInt(regionDPI * dim.x) + 1, Mathf.RoundToInt(regionDPI * dim.y + 1), TextureFormat.ARGB32, false, false); // create a texture larger than your maximum polygon size
        var color = new Color(0f, 0f, 0f, 0f);

        // Fill with color
        List<Color> cols = new List<Color>();// create an array and fill the texture with your color
        for (int i = 0; i < (texture.width * texture.height); i++)
            cols.Add(color);
        texture.SetPixels(cols.ToArray());
        texture.Apply();

        //image.sprite = Sprite.Create(texture, new Rect(0, 0, regionResolution, regionResolution), Vector2.zero, 1); //create a sprite with the texture we just created and colored in
        image.texture = texture;

        sitesGameObjects[site] = gameO;

        return gameO;

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

        foreach (var point in site.Points()) {
            if (lowest.x > point.x) lowest.x = point.x;
            if (lowest.y > point.y) lowest.y = point.y;
            if (highest.x < point.x) highest.x = point.x;
            if (highest.y < point.y) highest.y = point.y;
            //print("site:" + site.GetHashCode() + " => added point " + point);
            //print("site:" + site.GetHashCode() + " => range "+ lowest + "/"+highest);
        }

        return new Vector2[] { lowest, highest };
    }
}
