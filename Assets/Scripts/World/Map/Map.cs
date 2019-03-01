using csDelaunay;
using Superbest_random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
    public List<Region> regions = new List<Region>();
    public World world;

    Dictionary<Vector2f, Site> sites;
    List<Edge> edges;
    System.Random r;

    public enum Frontier { DESERT, PEAKS, OCEAN};

    [System.Serializable]
    public class Parameters
    {
        public int polygonNumber = 200;
        public int seed = 1;
        public int maxLloydIterations = 5;
        public int minLloydIterations = 5;
        public float mu;
        public float sigma;
        public float resolution = 256f;

        public bool isGaussianRandom = true;

        public int minRegionSize = 1;
        public int maxRegionSize = 3;

        public Frontier topFrontier;
        public Frontier bottomFrontier;
        public Frontier leftFrontier;
        public Frontier rightFrontier;
    }

    public void Generate(Parameters parameters, World _world)
    {
        world = _world;

        Random.InitState(parameters.seed);
        r = new System.Random(parameters.seed);

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
        Rectf bounds = new Rectf(0, 0, parameters.resolution, parameters.resolution);

        // There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
        Voronoi voronoi = new Voronoi(points, bounds, Random.Range(parameters.minLloydIterations, parameters.maxLloydIterations));

        // But you could also create it without lloyd relaxtion and call that function later if you want
        //Voronoi voronoi = new Voronoi(points,bounds);
        //voronoi.LloydRelaxation(5);

        // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
        sites = voronoi.SitesIndexedByLocation;
        edges = voronoi.Edges;

        regions = GenerateRegions(sites, parameters);
    }

    List<Region> GenerateRegions(Dictionary<Vector2f, Site> sites, Parameters parameters)
    {
        List<Region> regions = new List<Region>();
        List<Site> occupiedSites = new List<Site>();

        foreach (Site site in sites.Values)
        {
            if (occupiedSites.Contains(site))
            {
                continue;
            }

            Region region = new Region(this, new List<Site>() { site });

            int size = Random.Range(parameters.minRegionSize, parameters.maxRegionSize);

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

                    Site toBlob = blobCandidates[Mathf.FloorToInt(Random.value * blobCandidates.Count)];
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

        return regions;
    }


    List<Vector2f> CreateRandomGaussianPoints(Parameters parameters)
    {
        List<Vector2f> points = new List<Vector2f>();

        for (int i = 0; i < parameters.polygonNumber; i++)
        {
            var x = (r.NextGaussian(parameters.mu, parameters.sigma) + parameters.sigma * 2);
            var y = (r.NextGaussian(parameters.mu, parameters.sigma) + parameters.sigma * 2);
            var point = new Vector2f(x, y);
            points.Add(point);
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
            var point = new Vector2f(Random.Range(0, parameters.resolution), Random.Range(0, parameters.resolution));
            points.Add(point);
        }

        return points;
    }
}
