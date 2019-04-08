using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using CatchCo;

public class WorldGenerator : MonoBehaviour
{

    public Map.Parameters parameters;
    public World world;
    public float refreshRate = 1f;
    public int seed = 1;

    public WorldDisplayer displayer;

    private void Awake()
    {
        Library.Load();
    }

    void Start()
    {

        Ruleset rules = new Ruleset();
        Rules.LoadRuleset(rules);

        world = new World(seed);

        world.map.Generate(parameters, world);
        world.PopulateWithKingdoms(Rules.set[RULE.STARTING_KINGDOMS].GetInt(), Rules.set[RULE.MAX_STARTING_KINGDOM_STRENGTH].GetInt());

        StartCoroutine(RefreshDiagram());
    }


    [ExposeMethodInEditor]
    public void Regenerate()
    {
        seed = Random.Range(0, 1000);
        world = new World(seed);

        world.map.Generate(parameters, world);
        world.PopulateWithKingdoms(Rules.set[RULE.STARTING_KINGDOMS].GetInt(), Rules.set[RULE.MAX_STARTING_KINGDOM_STRENGTH].GetInt());
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
