using System.Collections;
using System.Collections.Generic;
using System;
using Superbest_random;

[Serializable]
public class World
{
    public Map map;
    public Kingdoms kingdoms = new Kingdoms();
    
    public static World Generate(Map.Parameters paraneters)
    {
        var world = new World();
        world.map = Map.Generate(paraneters);

        return world;
    }

    public void PopulateWithKingdoms(int amount, int maxSize=1)
    {
        kingdoms.Clear();

        for (int i = 0; i < amount; i++)
        {
            List<Region> territory = new List<Region>();
            int size = 1+KMaths.FloorToInt((1 - KMaths.Clamp01(Math.Abs((float)Game.state.random.NextGaussian()))) * maxSize) ;
            Region startingRegion = map.regions[KMaths.FloorToInt(Game.state.random.NextFloat() * map.regions.Count)];

            var race = Library.races[Game.state.random.Range(0, Library.races.Count)];
            Kingdom kingdom = new Kingdom(i, race.GetRandomKingdomName(), new List<Region>() { startingRegion }, race, Ruler.CreateRuler());

            for (int j = 0; j < size; j++)
            {
                List<Region> blobCandidates = kingdom.GetNeighborRegions();
                blobCandidates.RemoveAll(o => o.IsOwned());

                if (blobCandidates.Count <= 0)
                {
                    break;
                }

                kingdom.TakeOwnership(blobCandidates[KMaths.FloorToInt(blobCandidates.Count* Game.state.random.NextFloat())]);
            }

            kingdoms.Add(kingdom);
        }
    }
}
