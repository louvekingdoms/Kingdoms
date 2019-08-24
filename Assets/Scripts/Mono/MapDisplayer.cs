using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using csDelaunay;

public class MapDisplayer : MonoBehaviour
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
    Region selectedRegion;
    Dictionary<Site, RegionDisplayer> displayedSites = new Dictionary<Site, RegionDisplayer>();

    public void DrawMap(Map map)
    {
        CheckMousePosition(GetMouseUV());   
        DrawCells(map);
    }

    void SetHovered(Site site, bool isHovered)
    {
        foreach(var cell in cells) {
            if (cell.region.sites.Contains(site)) {
                cell.isHighlighted = isHovered;
                cell.MarkDirty();
                return;
            }
        }
    }

    RegionDisplayer GetSiteDisplayer(Site site, Region parentRegion)
    {
        if (!displayedSites.ContainsKey(site)) {
            var g = Instantiate(displayedCellExample, regionsLayer.transform);
            var displayer = g.GetComponent<RegionDisplayer>();
            var frontiers = parentRegion.GetFrontiers();
            var innerPoints = frontiers.innerEdges.ToSegments().Points();

            var scaledPoints = new List<Vector2>();

            for (int i = 0; i < innerPoints.Count; i++) {
                var point = innerPoints[i];
                var size = GetMapBoxSize();
                point -= site.Coord.ToVector2();
                point *= size * regionScale;

                scaledPoints.Add(point);
            }

            for (int i = 1; i < scaledPoints.Count; i++) {
                displayer.nonStrokeableSegments.Add(new Geometry.Segment() {
                    a= scaledPoints[i-1],
                    b= scaledPoints[i]
                });
            }
            
            displayedSites.Add(site, displayer);
        }
        return displayedSites[site];
    }

    void DestroyUnusedCells(List<Site> whitelist, Map map)
    {
        var toDestroy = new List<Site>();

        foreach (var site in displayedSites.Keys) {
            if (whitelist.Contains(site)) continue;
            toDestroy.Add(site);
        }

        foreach(var site in toDestroy) {    
            Destroy(displayedSites[site].gameObject);
            displayedSites.Remove(site);
        }

        var regionsToDestroy = new List<Region>();
        foreach(var cell in cells.ToArray()) {
            if (!map.regions.Contains(cell.region)) {
                cells.Remove(cell);
            }
        }
    }

    void DrawCells(Map map)
    {
        if (cells.Count <= 0) {
            MakeCells(map.regions);
            print("Made "+cells.Count+" cells");
        }

        var whitelist = new List<Site>();

        foreach (var cell in cells) {
            
            foreach (var site in cell.region.sites) {
                whitelist.Add(site);

                if (cell.IsDirty()) {
                    // If cell is dirty, draw again
                    var displayer = GetSiteDisplayer(site, cell.region);
                    var size = GetMapBoxSize();
                    displayer.GetComponent<RectTransform>().anchoredPosition = (size) * site.Coord.ToVector2() - new Vector2(size, size) / 2f;
                    displayer.SetColor(Color.white);
                    switch (cell.mode) {
                        case DisplayMode.POLITICAL:
                            if (cell.region.IsOwned()) {
                                var color = cell.region.owner.color;

                                displayer.SetColor(new Color(color.R / 255f, color.G / 255f, color.B / 255f));
                            }
                            break;
                    }
                    if (cell.isHighlighted)  displayer.SetColor(new Color(1f, 0f, 1f));
                    
                    var poly = site.ToPolygon().Move(-site.Coord.ToVector2()).Scale(size * regionScale);

                    displayer.polygon = poly;
                    displayer.SetAllDirty();
                }
            }

            cell.Clean();
        }

        DestroyUnusedCells(whitelist, map);
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
            var highlighted = false;
            foreach (var site in cell.region.sites) {
                var points = site.ToPolygon().Points();
                for (int i = 1; i < points.Count; i++) {
                    var triangle = new List<Vector2> {
                         points[i-1],
                         points[i],
                         site.Coord.ToVector2()
                    };
                    if (mouseUV.IsInTriangle(triangle.ToArray())) {
                        highlighted = true;
                        break;
                    }
                }
                if (highlighted) {
                    break;
                }
            }

            if (highlighted) {
                if (!cell.isHighlighted) cell.MarkDirty();
                cell.isHighlighted = true;
                if (Input.GetMouseButtonDown(0)) {
                    selectedRegion = cell.region;
                }
            }
            else {
                if (cell.isHighlighted) cell.MarkDirty();
                cell.isHighlighted = false;
            }
        }
    }

    public Region GetSelectedRegion()
    {
        return selectedRegion;
    }
}
