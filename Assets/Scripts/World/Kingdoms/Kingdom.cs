using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kingdom
{
    List<Region> territory = new List<Region>();
    Race mainRace;

    public int mainland;
    public string name;
    public Color color;
    public Ruler ruler;

    public Kingdom(string _name, List<Region> _territory, Race _mainRace, Ruler _ruler)
    {
        SetName(_name);
        mainRace = _mainRace;
        ruler = _ruler;

        Logger.Debug("Kingdom " + GetDebugSignature() + " (ruled by " + _ruler.name + ":" + _ruler.GetHashCode() + ") is born");

        TakeOwnership(_territory);
        mainland = 0;
    }

    ///////////////////////////////////
    ///
    ///     TERRITORY OWNERSHIP
    ///
    public void TakeOwnership(Region region) { TakeOwnership(new List<Region>() { region }); }

    public void TakeOwnership(List<Region> regions)
    {
        foreach (Region region in regions)
        {
            territory.Add(region);
            if (region.owner != null)
            {
                region.owner.RemoveOwnership(region);
            }

            Logger.Debug("Kingdom " + GetDebugSignature() + " took ownership of region " + region.GetHashCode() + (region.owner==null? "" : ". It previously belonged to " + region.owner.GetDebugSignature()));
            region.owner = this;
        }
    }

    public void RemoveOwnership(Region region) { RemoveOwnership(new List<Region>() { region }); }

    public void RemoveOwnership(List<Region> regions)
    {
        foreach (Region region in regions)
        {
            if (territory.Contains(region)) Logger.Debug("Kingdom " + GetDebugSignature() + " lost ownership of region " + region.GetHashCode()); ;
            territory.RemoveAll(o => o == region);
        }
    }

    public List<Region> GetNeighborRegions()
    {
        List<Region> regions = new List<Region>();
        foreach(Region region in territory)
        {
            foreach(Region neighbor in region.GetNeighbors())
            {
                if (!regions.Contains(neighbor) && !territory.Contains(neighbor)) regions.Add(neighbor);
            }
        }

        return regions;
    }

    public List<Region> GetTerritory()
    {
        return new List<Region>(territory.ToArray());
    }
    ///
    ///////////////////////////////////

    public List<Edge> GetFrontiers()
    {
        var edges = new List<Edge>();

        List<Edge> internalEdges = new List<Edge>();
        foreach (Region region in territory)
        {
            var bounds = region.GetFrontiers().outerEdges;
            
            foreach (Edge edge in bounds)
            {
                if (internalEdges.Contains(edge))
                {
                    edges.RemoveAll(o => o == edge);
                    continue;
                }
                internalEdges.Add(edge);
                edges.Add(edge);
            }
        }
        
        return edges;
    }

    public Region GetMainland()
    {
        return GetRegion(mainland);
    }

    public Region GetRegion(int index)
    {
        return territory[index];
    }

    public Color GetColor()
    {
        return color;
    }

    public Race GetMainRace()
    {
        return mainRace;
    }

    public int GetPopulation()
    {
        int total = 0;
        foreach(var region in territory)
        {
            total += region.population;
        }

        return total;
    }

    public void SetName(string _name)
    {
        var rnd = new System.Random(_name.GetHashCode());
        color = Color.HSVToRGB((float)rnd.NextDouble(), ((float)rnd.NextDouble()) / 3f + 0.3f, ((float)rnd.NextDouble()) / 5f + 0.5f);

        Logger.Debug("Kingdom " + GetDebugSignature() + " is now named ["+_name+"]") ;
        name = _name;
    }

    public string GetDebugSignature()
    {
        return name + "_" + mainRace + ":" + GetHashCode();
    }
}
