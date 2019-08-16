using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using CatchCo;

public class WorldGenerator : MonoBehaviour
{

    public Map.Parameters parameters;
    public World world;
    public float refreshRate = 0.25f;

    public MapDisplayer displayer;

    private void Awake()
    {
        Library.Initialize();
        Ruleset rules = new Ruleset();
        Rules.LoadRuleset(rules);
    }

    void Start()
    {
        world = new World(parameters.seed);
        world.map.Generate(parameters, world);
        world.PopulateWithKingdoms(Rules.set[RULE.STARTING_KINGDOMS].GetInt(), Rules.set[RULE.MAX_STARTING_KINGDOM_STRENGTH].GetInt());

        Game.state.world = world;
        
        // (game start)
        foreach (var region in Game.state.world.map.regions) {
            Region.behavior.onGameStart.Invoke(region);
        }

        StartCoroutine(RefreshDiagram());
    }


    [ExposeMethodInEditor]
    public void Regenerate()
    {
        parameters.seed = Random.Range(0, 1000);
        world = new World(parameters.seed);

        world.map.Generate(parameters, world);
        world.PopulateWithKingdoms(Rules.set[RULE.STARTING_KINGDOMS].GetInt(), Rules.set[RULE.MAX_STARTING_KINGDOM_STRENGTH].GetInt());
    }

    IEnumerator RefreshDiagram()
    {
        if (displayer!= null && displayer.enabled && displayer.gameObject.activeSelf) displayer.DrawMap(world.map);

        yield return new WaitForSeconds(refreshRate);

        yield return StartCoroutine(RefreshDiagram());
    }

    void ResetDiagram()
    {
    }
}      
