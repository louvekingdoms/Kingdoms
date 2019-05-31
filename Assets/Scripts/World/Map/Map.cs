using csDelaunay;
using Superbest_random;
using System;
using System.Collections;
using System.Collections.Generic;

public class Map
{
    public List<Region> regions = new List<Region>();
    public World world;

    Dictionary<Vector2f, Site> sites;
    List<Edge> edges;

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

        public int maxMountains = 4;
        public int minMountains = 1;
        [UnityEngine.Range(0f, 1f)] public float mountainsCentrality = 0.4f;
        [UnityEngine.Range(1f, 3f)] public float steepness = 1f;

        public int minRegionSize = 1;
        public int maxRegionSize = 3;
        public float countrySize = 0.85f;

        public Frontier topFrontier;
        public Frontier bottomFrontier;
        public Frontier leftFrontier;
        public Frontier rightFrontier;
    }

    public void Generate(Parameters parameters, World _world)
    {
        world = _world;
        
        Game.random = new Random(parameters.seed);

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
        Voronoi voronoi = new Voronoi(points, bounds, Game.random.Range(parameters.minLloydIterations, parameters.maxLloydIterations));

        // But you could also create it without lloyd relaxtion and call that function later if you want
        //Voronoi voronoi = new Voronoi(points,bounds);
        //voronoi.LloydRelaxation(5);

        // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
        sites = voronoi.SitesIndexedByLocation;
        edges = voronoi.Edges;

        var mountains = GetRandomPoints(parameters, Game.random.Range(parameters.minMountains, parameters.maxMountains), parameters.mountainsCentrality);

        regions = GenerateRegions(parameters, sites, mountains);
    }

    List<Vector2f> GetRandomPoints(Parameters parameters, int amount=1, float range=1f, float minRadius=0f)
    {
        var points = new List<Vector2f>();
        for (int i = 0; i < amount; i++)
        {
            var x = (Game.random.NextGaussian(parameters.mu, parameters.sigma) + parameters.sigma * 2)/range + minRadius;
            var y = (Game.random.NextGaussian(parameters.mu, parameters.sigma) + parameters.sigma * 2)/range + minRadius;
            points.Add(new Vector2f((float)x, (float)y));
        }
        return points;
    }

    List<Region> GenerateRegions(Parameters parameters, Dictionary<Vector2f, Site> sites, List<Vector2f> mountains)
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

            Region region = new Region(id, this, new List<Site>() { site });

            // Calculating elevation
            Vector2f nearestMountain = new Vector2f();
            bool firstLoop = true;
            foreach(var mountain in mountains)
            {
                if (firstLoop || nearestMountain.Distance(site.Coord) > mountain.Distance(site.Coord))
                {
                    nearestMountain = mountain;
                    firstLoop = false;
                }
            }
            region.elevation = Math.Max(0f, Math.Min(1f, 
                1f - nearestMountain.Distance(site.Coord) * parameters.steepness
            ));

            id++;

            int size = Game.random.Range(parameters.minRegionSize, parameters.maxRegionSize);

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

                    Site toBlob = blobCandidates[(int)Math.Floor(Game.random.NextFloat() * blobCandidates.Count)];
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

        Logger.Info("Generated " + regions.Count + " regions from "+ sites.Count+" sites");

        return regions;
    }


    List<Vector2f> CreateRandomGaussianPoints(Parameters parameters)
    {
        List<Vector2f> points = new List<Vector2f>();

        for (int i = 0; i < parameters.polygonNumber; i++)
        {
            var x = (Game.random.NextGaussian(parameters.mu, parameters.sigma) + parameters.sigma * 2);
            var y = (Game.random.NextGaussian(parameters.mu, parameters.sigma) + parameters.sigma * 2);
            var point = new Vector2f(x, y);
            points.Add(point);
            if (point.x > 1 || point.y > 1) {
                Logger.Warn("Warning out of bounds region : " + point);
            }
        }

        return points;
    }

    List<Vector2f> CreateRandomPoints(Parameters parameters)
    {
        // Use Vector2f, instead of Vector2
        // Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
        List<Vector2f> points = new List<Vector2f>();
        for (int i = 0; i < parameters.polygonNumber; i++)
        {
            var point = new Vector2f(Game.random.Range(0, 1), Game.random.Range(0, 1));
            points.Add(point);
        }

        return points;
    }
}
