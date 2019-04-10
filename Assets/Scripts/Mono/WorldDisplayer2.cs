using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using csDelaunay;

public class WorldDisplayer2 : MonoBehaviour
{
    public GameObject regionsLayer;
    public int regionResolution = 256;
    public float regionScale = 1f;
    public float mapSize = 20f;

    Dictionary<Site, GameObject> sitesGameObjects = new Dictionary<Site, GameObject>();

    public void DrawMap(Map map)
    {

        foreach (var region in map.regions) {
            foreach (var site in region.sites) {

                // generate vertices list
                List<Vector2> points = new List<Vector2>();
                
                foreach(Edge edge in site.Edges) {
                    if (edge.ClippedEnds == null) continue;
                    var left = ToVector2(edge.ClippedEnds[LR.LEFT]);
                    var right = ToVector2(edge.ClippedEnds[LR.RIGHT]);

                    /*
                    if (left.x < bottomLeft.x) bottomLeft.x = left.x;
                    if (left.y < bottomLeft.y) bottomLeft.y = left.y;
                    if (right.x < bottomLeft.x) bottomLeft.x = right.x;
                    if (right.y < bottomLeft.y) bottomLeft.y = right.y;

                    if (left.x > topRight.x) topRight.x = left.x;
                    if (left.y > topRight.y) topRight.y = left.y;
                    if (right.x > topRight.x) topRight.x = right.x;
                    if (right.y > topRight.y) topRight.y = right.y;
                    */
    
                    points.Add(left);
                    points.Add(right);
                }

                List<ushort> triangles = new List<ushort>();
                for (int i = 1; i < points.Count - 1; i++) {
                    triangles.Add(0);
                    triangles.Add((ushort)(i));
                    triangles.Add((ushort)(i + 1));
                }
                triangles.Add(0);
                triangles.Add((ushort)(points.Count - 1));
                triangles.Add((ushort)(1));
                
                var g = GetSiteObject(site, regionsLayer.transform);
                var spr = g.GetComponent<SpriteRenderer>().sprite;
                spr.OverrideGeometry(points.ToArray(), triangles.ToArray()); // set the vertices and triangles

                g.transform.localPosition = (ToVector2(site.Coord) - new Vector2(0.5f, 0.5f)) * mapSize; // return to world space

            }
        }
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
        var color = new Color(0f, 1f, 0f, 1f);

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
