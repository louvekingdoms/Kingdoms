using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using System.Linq;

public class Region
{
    public List<Site> sites = new List<Site>();
    public int capital;
    public Map map { get; }
    public Kingdom owner = null;
    public int population;

    public float elevation = 0f;
    public float moisture = 0f;

    public Region(Map _map, List<Site> _sites, int _capital = 0)
    {
        sites = _sites;
        capital = _capital;
        map = _map;
        population = Rules.set[RULE.STARTING_POPULATION_PER_REGION].GetInt();
    }

    public class Frontiers
    {
        public List<Edge> innerEdges;
        public List<Edge> outerEdges;
    }

    public Frontiers GetFrontiers()
    {
        var edges = new List<Edge>();

        foreach(Site site in sites)
        {
            foreach(Edge edge in site.Edges)
            {
                edges.Add(edge);
            }
        }

        List<Edge> internalEdges = new List<Edge>();
        List<Edge> finalEdges = new List<Edge>();
        foreach (Edge edge in edges) {
            if (internalEdges.Contains(edge))
            {
                finalEdges.RemoveAll(o => o == edge);
                continue;
            }
            internalEdges.Add(edge);
            finalEdges.Add(edge);
        }

        internalEdges.RemoveAll(o => finalEdges.Contains(o));

        return new Frontiers() { innerEdges = internalEdges, outerEdges = finalEdges };
    }

    public List<Region> GetNeighbors()
    {
        List<Region> neighbors = new List<Region>();

        List<Site> neighborSites = GetNeighborSites();
        foreach(Site site in sites)
        {
            foreach(Site neighborSite in site.NeighborSites())
            {
                if (!sites.Contains(neighborSite) && !neighborSites.Contains(neighborSite))
                {
                    neighborSites.Add(neighborSite);
                }
            }
        }

        foreach(Site neighborSite in neighborSites)
        {
            foreach(Region region in map.regions)
            {
                if (region.sites.Contains(neighborSite) && !neighbors.Contains(region))
                {
                    neighbors.Add(region);
                }
            }
        }

        return neighbors;
    }

    public List<Site> GetNeighborSites()
    {
        List<Site> neighborSites = new List<Site>();
        foreach (Site site in sites)
        {
            foreach (Site neighborSite in site.NeighborSites())
            {
                if (!sites.Contains(neighborSite) && !neighborSites.Contains(neighborSite))
                {
                    neighborSites.Add(neighborSite);
                }
            }
        }
        return neighborSites;
    }

    public bool IsOwned()
    {
        return owner != null;
    }
}
