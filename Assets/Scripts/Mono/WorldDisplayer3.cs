using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using csDelaunay;

public class WorldDisplayer3 : MonoBehaviour
{
    [System.Serializable]
    private class Cell : System.Object
    {
        public readonly Region region;
        public bool isSelected;
        public bool isHighlighted;
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
    }

    public RawImage regionsLayer;
    public DisplayMode defaultMode;
    public Material strokeMaterial;
    public enum DisplayMode { HIDDEN, CLEAN, TOPOLOGY, POLITICAL };

    public GameObject displayedCellExample;
    [Range(0.75f, 1f)] public float regionScale = 0.9f;

    List<Cell> cells = new List<Cell>();
    Dictionary<Site, RegionDisplayer> displayedSites = new Dictionary<Site, RegionDisplayer>();

    public void DrawMap(Map map)
    {
        DrawCells(map);
    }

    void SetHovered(Site site, bool isHovered)
    {
        foreach(var cell in cells) {
            if (cell.region.sites.Contains(site)) {
                cell.isHighlighted = isHovered;
                cell.MarkDirty();
                print("Set highlight to " + isHovered);
                return;
            }
        }
    }

    RegionDisplayer GetSiteDisplayer(Site site)
    {
        if (!displayedSites.ContainsKey(site)) {
            var g = Instantiate(displayedCellExample, regionsLayer.transform);
            var displayer = g.GetComponent<RegionDisplayer>();
            displayedSites.Add(site, displayer);
            displayer.onMouseEnter += delegate { SetHovered(site, true); };
            displayer.onMouseExit += delegate { SetHovered(site, false); };
        }
        return displayedSites[site];
    }

    void DrawCells(Map map)
    {
        if (cells.Count <= 0) {
            MakeCells(map.regions);
            print("Made "+cells.Count+" cells");
        }

        // Sorting..?

        foreach (var cell in cells) {
            if (cell.IsDirty()) {
                foreach (var site in cell.region.sites) {
                    var displayer = GetSiteDisplayer(site);
                    var size = GetMapBoxSize();
                    displayer.GetComponent<RectTransform>().anchoredPosition = (size) * ToVector2(site.Coord) - new Vector2(size, size) / 2f;
                    displayer.SetColor(Color.white);
                    switch (cell.mode) {
                        case DisplayMode.POLITICAL:
                            if (cell.region.IsOwned()) {
                                displayer.SetColor(cell.region.owner.color);
                            }
                            break;
                    }
                    if (cell.isHighlighted) displayer.SetColor(new Color(1f, 0f, 1f));

                    displayer.polygon = ScalePolygon(
                        MovePolygon(ToSegments(site.Edges), -ToVector2(site.Coord)), size * regionScale);
                    displayer.SetAllDirty();

                    cell.Clean();
                }
            }
        }
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

    float GetMapBoxSize()
    {
        return regionsLayer.transform.parent.GetComponent<RectTransform>().sizeDelta.x + regionsLayer.GetComponent<RectTransform>().sizeDelta.x;
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

    List<Pencil.Segment> MovePolygon(List<Pencil.Segment> segments, Vector2 offset)
    {
        var newPoly = new List<Pencil.Segment>();
        foreach (var seg in segments) {
            newPoly.Add(new Pencil.Segment() {
                a = seg.a + offset,
                b = seg.b + offset
            });
        }
        return newPoly;
    }

    List<Pencil.Segment> ScalePolygon(List<Pencil.Segment> segments, float factor)
    {
        var newPoly = new List<Pencil.Segment>();
        foreach(var seg in segments) {
            newPoly.Add(new Pencil.Segment() {
                a = seg.a * factor,
                b = seg.b * factor
            });
        }
        return newPoly;
    }

    List<Pencil.Segment> SiteSegments(Site site)
    {
        return ToSegments(site.Edges);
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
}
