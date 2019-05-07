using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using csDelaunay;

public class WorldDisplayer2 : MonoBehaviour
{
    [System.Serializable]
    class Cell
    {
        public readonly Region region;
        public bool isSelected { set { SetAltered(); isSelected = value; } }
        public bool isHighlighted { set { SetAltered(); isHighlighted = value; } }
        bool hasChanged;
        DisplayMode mode;

        public Cell(Region region)
        {
            this.region = region;
        }

        public void SetAltered()
        {
            hasChanged = true;
        }

        public void SetClean()
        {
            hasChanged = false;
        }

        public bool Differs()
        {
            return hasChanged;
        }

        public DisplayMode GetMode()
        {
            return mode;
        }

        public void SetMode(DisplayMode mode)
        {
            SetAltered();
            this.mode = mode;
        }
    }

    public RawImage regionsLayer;
    public DisplayMode defaultMode;
    public Material strokeMaterial;
    public enum DisplayMode { CLEAN, TOPOLOGY, POLITICAL };

    public int drawRegion;

    List<Cell> cells = new List<Cell>();

    public void DrawMap(Map map)
    {
        UpdateLayerTexture();
        DrawCells(map);

        /*
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
        */
    }

    void DrawCells(Map map)
    {
        if (cells.Count <= 0) {
            MakeCells(map.regions);
            print("Made "+cells.Count+" cells");
        }

        var tex = GetTex();

        foreach (var cell in cells) {
            if (cell.Differs()) {
                var edges = cell.region.GetFrontiers().outerEdges;
                var segs = ToSegments(edges);

                tex.SetPencilColor(Color.green);

                var UVs = new List<Pencil.Segment>();
                foreach (var seg in segs) {
                    UVs.Add(seg);
                }

                float width = 2f;
                tex.SetPencilColor(Color.white);
                switch (cell.GetMode()) {
                    case DisplayMode.POLITICAL:
                        if (cell.region.IsOwned()) {
                            tex.SetPencilColor(cell.region.owner.color); width = 4f;
                        }
                        break;
                }
                tex.Lines(UVs, width);
                print("Lined cell " + cell);
                cell.SetClean();
            }
        }
        tex.Apply();
    }

    void MakeCells(List<Region> regions)
    {
        foreach(var region in regions) {
            var cell = new Cell(region);
            cell.SetMode(defaultMode);
            cells.Add(cell);
        }
    }

    void UpdateLayerTexture()
    {
        var size = regionsLayer.transform.parent.GetComponent<RectTransform>().sizeDelta.x + regionsLayer.GetComponent<RectTransform>().sizeDelta.x;
        var sizei = Mathf.RoundToInt(size);

        if (regionsLayer.texture == null || GetTex().width != sizei || GetTex().height != sizei) {

            if (regionsLayer.texture) Destroy(regionsLayer.texture);
            regionsLayer.texture = new Texture2D(sizei, sizei);

        }
    }

    void DrawRegions(List<Region> regions)
    {
        var a = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        var b = regionsLayer.GetComponent<RectTransform>().position;
        var c = new Vector2(b.x, b.y);
        var size = regionsLayer.transform.parent.GetComponent<RectTransform>().sizeDelta.x + regionsLayer.GetComponent<RectTransform>().sizeDelta.x;
        var UVMouse = (a - c) / size + new Vector2(0.5f, 0.5f);



        var i = 0;
        foreach (var region in regions)
        {
            foreach (var site in region.sites) {
                i++;
                var col = Color.red;
                if (IsPointInPolygon(SiteSegments(site), UVMouse)) {
                    col = Color.white;
                    DrawSite(site, col);
                    return;
                }
                DrawSite(site, col);
            }
        }
    }

    void DrawSite(Site site, Color color)
    {
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
    }
    
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
        return ToSegments(site.Edges);
    }

    bool IsPointInPolygon(List<Pencil.Segment> polygon, Vector2 point)
    {
        bool isInside = false;
        foreach(var seg in polygon) { 
            if (
            ((seg.b.y > point.y) != (seg.a.y > point.y)) &&
            (point.x < (seg.a.x - seg.b.x) * (point.y - seg.b.y) / (seg.a.y - seg.b.y) + seg.b.x)
            ) {
                isInside = !isInside;
            }
        }
        return isInside;
    }

    List<Pencil.Segment> ToSegments(List<Edge> edges)
    {
        var segs = new List<Pencil.Segment>();
        foreach (Edge edge in edges) {
            if (edge.ClippedEnds == null) continue;
            var a = ToVector2(edge.ClippedEnds[LR.LEFT]);
            var b = ToVector2(edge.ClippedEnds[LR.RIGHT]);
            segs.Add(new Pencil.Segment { a = a, b = b });
        }
        return segs;
    }

    Pencil.Segment ToSegment(Edge edge)
    {
        var a = ToVector2(edge.ClippedEnds[LR.LEFT]);
        var b = ToVector2(edge.ClippedEnds[LR.RIGHT]);
        return new Pencil.Segment { a = a, b = b };
    }

    Texture2D GetTex()
    {

        return ((Texture2D)regionsLayer.mainTexture);
    }
}
