using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using csDelaunay;

public class WorldDisplayer2 : MonoBehaviour
{
    [System.Serializable]
    private class Cell : System.Object
    {
        public readonly Region region;
        public bool isSelected;
        bool isHighlighted;
        public DisplayMode mode = DisplayMode.HIDDEN;
        Cell previousState;

        public Cell(Region _region)
        {
            region = _region;
        }

        public void MarkDirty()
        {
            previousState = new Cell(region) {
                isHighlighted = isHighlighted,
                isSelected = isSelected,
                mode = mode
            };
        }
        
        public bool IsDirty()
        {
            return !ReferenceEquals(this, previousState);
        }

        public void Clean()
        {
            previousState = this;
        }

        public void SetHighlight(bool value)
        {
            if (value != isHighlighted) MarkDirty();
            isHighlighted = value;
        }

        public bool IsHighlighted()
        {
            return isHighlighted;
        }
    }

    public RawImage regionsLayer;
    public DisplayMode defaultMode;
    public Material strokeMaterial;
    public enum DisplayMode { HIDDEN, CLEAN, TOPOLOGY, POLITICAL };

    public int drawRegion;

    List<Cell> cells = new List<Cell>();

    public void DrawMap(Map map)
    {
        UpdateLayerTexture();
        CheckMousePosition(GetMouseUV());
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

        // Sorting..?

        var tex = GetTex();

        foreach (var cell in cells) {
            if (cell.IsDirty()) {
                var edges = cell.region.GetFrontiers().outerEdges;
                var segs = ToSegments(edges);

                tex.SetPencilColor(Color.green);

                var UVs = new List<Pencil.Segment>();
                foreach (var seg in segs) {
                    UVs.Add(seg);
                }

                float width = 2f;
                tex.SetPencilColor(Color.white);
                switch (cell.mode) {
                    case DisplayMode.POLITICAL:
                        if (cell.region.IsOwned()) {
                            tex.SetPencilColor(cell.region.owner.color);
                            width = 4f;
                        }
                        break;
                }
                if (cell.IsHighlighted()) tex.SetPencilColor(new Color(1f, 0f, 1f));

                tex.Lines(UVs, width);
                cell.Clean();
            }
        }

        tex.SetPencilColor(Color.red);
        try { tex.Circle(GetMouseUV(), 2f); } catch { }
        tex.Apply();
    }

    void MakeCells(List<Region> regions)
    {
        foreach(var region in regions) {
            var cell = new Cell(region);
            cell.mode = defaultMode;
            cell.MarkDirty();
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

    Vector2 GetMouseUV()
    {
        var mouse = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        var layerPos = regionsLayer.GetComponent<RectTransform>().position;
        var layerPos2D = new Vector2(layerPos.x, layerPos.y);
        var size = regionsLayer.transform.parent.GetComponent<RectTransform>().sizeDelta.x + regionsLayer.GetComponent<RectTransform>().sizeDelta.x;
        var UVMouse = (mouse - layerPos2D) / size + new Vector2(0.5f, 0.5f);

        return UVMouse;
    }

    void CheckMousePosition(Vector2 mouseUV)
    {
        
        foreach (var cell in cells) {
            if (IsPointInPolygon(ToSegments(cell.region.GetFrontiers().outerEdges), mouseUV)) {
                cell.SetHighlight(true);
            }
            else {
                cell.SetHighlight(false);
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
        var bounds = PolygonBounds(SiteSegments(site));
        var dim = bounds[1] - bounds[0];
        return dim;
    }

    Vector2[] PolygonBounds(List<Pencil.Segment> polygon)
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

    List<Pencil.Segment> SiteSegments(Site site)
    {
        return ToSegments(site.Edges);
    }

    bool IsPointInPolygon(List<Pencil.Segment> polygon, Vector2 point)
    {
        bool isInside = false;
        var bounds = PolygonBounds(polygon);

        if (!IsPointInBounds(point, bounds)) return false;

        foreach (var segment in polygon) {

        }

        return isInside;
    }

    bool IsPointInBounds(Vector2 point, Vector2[] bounds)
    {
        return point.x > bounds[0].x
            && point.x < bounds[1].x
            && point.x > bounds[0].y
            && point.x < bounds[1].y;
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
