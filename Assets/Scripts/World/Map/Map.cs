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
    Dictionary<Vector2f, Frontier> frontiers = new Dictionary<Vector2f, Frontier>();


    public class Frontier
    {
        public enum Type { DESERT, PEAKS, OCEAN, EMPIRE };
        public Type type;
        public List<Region> frontieredRegions = new List<Region>();
    }

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

        public Frontier.Type topFrontier;
        public Frontier.Type bottomFrontier;
        public Frontier.Type leftFrontier;
        public Frontier.Type rightFrontier;
    }

    public void Generate(Parameters parameters, World _world)
    {
        world = _world;
        
        Game.random = new Random(parameters.seed);
        CalculateFrontiers(parameters);

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

    void CalculateFrontiers(Parameters parameters)
    {
        frontiers.Clear();
        frontiers.Add(new Vector2f(0f, 0.5f), new Frontier() { type = parameters.leftFrontier });
        frontiers.Add(new Vector2f(1f, 0.5f), new Frontier() { type = parameters.rightFrontier });
        frontiers.Add(new Vector2f(0.5f, 0f), new Frontier() { type = parameters.topFrontier });
        frontiers.Add(new Vector2f(0.5f, 1f), new Frontier() { type = parameters.bottomFrontier });
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


            // Calculating out-of-map region
            float distanceWithCenter = site.Coord.Distance(new Vector2f(0.5f, 0.5f));
            if (distanceWithCenter > parameters.countrySize)
            {
                OutFrontierize(region);
            }

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

    void OutFrontierize(Region region)
    {
        region.isControllable = false;
        Vector2f center = region.sites[region.capital].Coord;
        Vector2f nearestFrontierPos = new Vector2f();
        bool foundNearest = false;
        foreach(var frontierPos in frontiers.Keys)
        {
            if (!foundNearest || nearestFrontierPos.Distance(center) > frontierPos.Distance(center))
            {
                nearestFrontierPos = frontierPos;
                foundNearest = true;
            }
        }

        var frontier = frontiers[nearestFrontierPos];
        region.frontier = frontier;
        frontier.frontieredRegions.Add(region);

        switch (frontier.type)
        {
            case Frontier.Type.DESERT:
                region.moisture = region.temperature < 0.5f ? 1f : 0f;
                region.elevation = Game.random.NextFloat() * 0.05f;
                region.temperature = region.temperature < 0.5f ? 0f : 1f;
                break;

            case Frontier.Type.EMPIRE:
                if (frontier.frontieredRegions.Count > 0) {
                    region.owner = frontier.frontieredRegions[0].owner;
                }
                else
                {   // Creating empire
                    var race = Library.races[Game.random.Range(0, Library.races.Count)];
                    region.owner = new Kingdom(-1, race.GetRandomKingdomName(), new List<Region>() { region }, race, Ruler.CreateRuler());
                    region.owner.name = "Empire of "+ region.owner.name;
                }
                break;

            case Frontier.Type.OCEAN:
                region.moisture = 1f;
                region.elevation = 0f;
                region.temperature *= 0.5f;
                break;

            case Frontier.Type.PEAKS:
                region.elevation = 1f;
                break;
        }
    }
}
