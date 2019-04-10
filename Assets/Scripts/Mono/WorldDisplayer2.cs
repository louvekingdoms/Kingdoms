using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using csDelaunay;

public class WorldDisplayer2 : MonoBehaviour
{
    public GameObject regionsLayer;
    public int regionResolution = 256;
    public DisplayMode mode;
    [Range(0f, 10f)] public float mapSize = 10f;

    public enum DisplayMode { REGION, TERRITORY, KINGDOM };

    float regionScale = 0f;

    Dictionary<Site, GameObject> sitesFills = new Dictionary<Site, GameObject>();
    Dictionary<Site, GameObject> sitesStrokes = new Dictionary<Site, GameObject>();

    public void DrawMap(Map map)
    {

        regionScale = 10f - mapSize;



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
                FillSite(site, Color.black);
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

    void StrokeSite(Site site, Color color, float scale = 1f)
    {

    }

    void FillSite(Site site, Color color, float scale=1f)
    {
        // generate vertices list
        List<Vector2> points = new List<Vector2>();

        foreach (Edge edge in site.Edges)
        {
            if (edge.ClippedEnds == null) continue;
            var left = ToVector2(edge.ClippedEnds[LR.LEFT]);
            var right = ToVector2(edge.ClippedEnds[LR.RIGHT]);

            points.Add(left * regionScale * scale);
            points.Add(right * regionScale * scale);
        }

        var position = ToVector2(site.Coord);
        var g = GetSiteObject(site, regionsLayer.transform);
        var spriteRenderer = g.GetComponent<SpriteRenderer>();
        var spr = spriteRenderer.sprite;
        
        spriteRenderer.color = color;

        ShapeSprite(points, position, spr);
        g.transform.localPosition = (position) * mapSize - new Vector2(regionScale / 2, regionScale / 2) - new Vector2(mapSize / 2, mapSize / 2); // return to world space
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
        var sr = gameO.AddComponent<SpriteRenderer>(); // add a sprite renderer
        var texture = new Texture2D(regionResolution + 1, regionResolution + 1); // create a texture larger than your maximum polygon size
        var color = new Color(1f, 1f, 1f, 1f);

        // Fill with color
        List<Color> cols = new List<Color>();// create an array and fill the texture with your color
        for (int i = 0; i < (texture.width * texture.height); i++)
            cols.Add(color);
        texture.SetPixels(cols.ToArray());
        texture.Apply();

        sr.sprite = Sprite.Create(texture, new Rect(0, 0, regionResolution, regionResolution), Vector2.zero, 1); //create a sprite with the texture we just created and colored in

        sitesGameObjects[site] = gameO;

        return gameO;

    }
}
