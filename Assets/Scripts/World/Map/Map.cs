using csDelaunay;
using Newtonsoft.Json;
using Superbest_random;
using System;
using System.Collections;
using System.Collections.Generic;
using static GameLogger;

[Serializable]
public class Map
{
    public List<Region> regions = new List<Region>();
    public World world { get; }

    Dictionary<Vector2f, Site> sites;
    List<Edge> edges;
    Random r;
    
    public enum Frontier { DESERT, PEAKS, OCEAN, EMPIRE};

    [System.Serializable]
    public class Parameters
    {
        public int polygonNumber = 200;
        public int seed = 1;
        public int maxLloydIterations = 5;
        public int minLloydIterations = 5;
        public float mu;
        public float sigma;

        public bool isGaussianRandom = true;

        public int minRegionSize = 1;
        public int maxRegionSize = 3;

        public Frontier topFrontier;
        public Frontier bottomFrontier;
        public Frontier leftFrontier;
        public Frontier rightFrontier;
    }

    public static Map Generate(Parameters parameters)
    {
        var map = new Map();

        Game.state.random = new Random(parameters.seed);
        map.r = Game.state.random;

        // Create your sites (lets call that the center of your polygons)
        List<Vector2f> points = new List<Vector2f>();
        if (parameters.isGaussianRandom)
        {
            points = CreateRandomGaussianPoints(parameters);
        }
        else
        {
            points = CreateRandomPoints(parameters);
        }

        // Create the bounds of the voronoi diagram
        // Use Rectf instead of Rect; it's a struct just like Rect and does pretty much the same,
        // but like that it allows you to run the delaunay library outside of unity (which mean also in another tread)
        Rectf bounds = new Rectf(0, 0, 1, 1);

        // There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
        Voronoi voronoi = new Voronoi(points, bounds, map.r.Range(parameters.minLloydIterations, parameters.maxLloydIterations));

        // But you could also create it without lloyd relaxtion and call that function later if you want
        //Voronoi voronoi = new Voronoi(points,bounds);
        //voronoi.LloydRelaxation(5);

        // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
        map.sites = voronoi.SitesIndexedByLocation;
        map.edges = voronoi.Edges;

        map.regions = GenerateRegions(map.sites, parameters, map);

        return map;
    }

    static List<Region> GenerateRegions(Dictionary<Vector2f, Site> sites, Parameters parameters, Map parentMap)
    {
        List<Region> regions = new List<Region>();
        List<Site> occupiedSites = new List<Site>();
        int id = 1;
        foreach (Site site in sites.Values)
        {
            if (occupiedSites.Contains(site))
            {
                continue;
            }

            Region region = new Region(id, parentMap, new List<Site>() { site });
            id++;

            int size = parentMap.r.Range(parameters.minRegionSize, parameters.maxRegionSize);

            if (size > 0)
            {
                // Making list of candidates for expansion
                List<Site> blobCandidates = new List<Site>();
                foreach (Site neighbor in site.NeighborSites())
                {
                    if (!occupiedSites.Contains(neighbor))
                    {
                        blobCandidates.Add(neighbor);
                    }
                }

                for (int i = 0; i < size; i++)
                {
                    if (blobCandidates.Count <= 0)
                    {
                        break;
                    }

                    Site toBlob = blobCandidates[KMaths.FloorToInt(parentMap.r.NextFloat() * blobCandidates.Count)];
                    region.sites.Add(toBlob);
                    occupiedSites.Add(toBlob);

                    blobCandidates.Clear();

                    // Updating neighbors
                    var newNeighbors = region.GetNeighborSites();
                    foreach (Site neighbor in newNeighbors)
                    {
                        if (!occupiedSites.Contains(neighbor))
                        {
                            blobCandidates.Add(neighbor);
                        }
                    }
                }
            }

            occupiedSites.Add(site);
            regions.Add(region);
        }

        logger.Info("Generated " + regions.Count + " regions from "+ sites.Count+" sites");

        return regions;
    }


    static List<Vector2f> CreateRandomGaussianPoints(Parameters parameters)
    {
        List<Vector2f> points = new List<Vector2f>();

        for (int i = 0; i < parameters.polygonNumber; i++)
        {
            var x = (Game.state.random.NextGaussian(parameters.mu, parameters.sigma) + parameters.sigma * 2);
            var y = (Game.state.random.NextGaussian(parameters.mu, parameters.sigma) + parameters.sigma * 2);
            var point = new Vector2f(x, y);
            points.Add(point);
            if (point.x > 1 || point.y > 1) {
                logger.Warn("Warning out of bounds region : " + point);
            }
        }

        return points;
    }

    static List<Vector2f> CreateRandomPoints(Parameters parameters)
    {
        // Use Vector2f, instead of Vector2
        // Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
        List<Vector2f> points = new List<Vector2f>();
        for (int i = 0; i < parameters.polygonNumber; i++)
        {
            var point = new Vector2f(Game.state.random.Range(0, 1), Game.state.random.Range(0, 1  ));
            points.Add(point);
        }

        return points;
    }
}
