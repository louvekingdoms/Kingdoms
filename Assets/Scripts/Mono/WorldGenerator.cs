using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;


public class WorldGenerator : MonoBehaviour
{

    public Map.Parameters parameters;
    public float refreshRate = 1f;
    public int seed = 1;
    public World world;
    public int kingdoms = 0;
    public int maxKingdomStrength = 5;

    public WorldDisplayer displayer;

    void Start()
    {
        world = new World(seed);

        world.map.Generate(parameters, world);
        world.PopulateWithKingdoms(kingdoms, maxKingdomStrength);

        StartCoroutine(RefreshDiagram());
    }

    IEnumerator RefreshDiagram()
    {

        displayer.DrawMap(world.map, Mathf.RoundToInt(parameters.resolution));

        yield return new WaitForSeconds(refreshRate);

        yield return StartCoroutine(RefreshDiagram());
    }

    void ResetDiagram()
    {
    }
}      
