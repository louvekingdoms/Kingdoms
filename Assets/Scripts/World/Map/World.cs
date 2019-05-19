using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Superbest_random;

public class World
{
    public Map map = new Map();
    public List<Kingdom> kingdoms = new List<Kingdom>();

    public int seed { get; }

    public World(int _seed)
    {
        seed = _seed;
    }

    public void PopulateWithKingdoms(int amount, int maxSize=1)
    {
        kingdoms.Clear();

        var r = new System.Random(seed);
        for (int i = 0; i < amount; i++)
        {
            List<Region> territory = new List<Region>();
            int size = 1+Mathf.FloorToInt((1 - Mathf.Clamp01(Mathf.Abs((float)r.NextGaussian()))) * maxSize) ;
            Region startingRegion = map.regions[Mathf.FloorToInt(Random.value * map.regions.Count)];

            var race = Library.races[Random.Range(0, Library.races.Count)];
            Kingdom kingdom = new Kingdom(i, race.GetRandomKingdomName(), new List<Region>() { startingRegion }, race, Ruler.CreateRuler());

            for (int j = 0; j < size; j++)
            {
                List<Region> blobCandidates = kingdom.GetNeighborRegions();
                blobCandidates.RemoveAll(o => o.owner != null);

                if (blobCandidates.Count <= 0)
                {
                    break;
                }

                kingdom.TakeOwnership(blobCandidates[Mathf.FloorToInt(blobCandidates.Count*Random.value)]);
            }

            kingdoms.Add(kingdom);
        }
    }
}
