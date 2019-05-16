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
    public int seed = 1;

    public MapDisplayer displayer;

    private void Awake()
    {
        Library.Initialize();
        Ruleset rules = new Ruleset();
        Rules.LoadRuleset(rules);
    }

    void Start()
    {

        world = new World(seed);

        world.map.Generate(parameters, world);
        world.PopulateWithKingdoms(Rules.set[RULE.STARTING_KINGDOMS].GetInt(), Rules.set[RULE.MAX_STARTING_KINGDOM_STRENGTH].GetInt());

        /*
         * narration!
         * 
        Logger.Debug("Shortly after the fall of the Great Empire, a number of kingdoms arised from the ashes.");
        Logger.Debug("Among them, were...");
        foreach (var kingdom in world.kingdoms)
        {
            Logger.Debug(
                "- The kingdom of " + 
                kingdom.name + ", counting " + 
                kingdom.GetPopulation() + " "+
                kingdom.GetMainRace().name+" souls, led by the migthy " + 
                kingdom.ruler.name + 
            "");
        }
        Logger.Debug("And the great war was about to start.");
        */
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
        displayer.DrawMap(world.map);

        yield return new WaitForSeconds(refreshRate);

        yield return StartCoroutine(RefreshDiagram());
    }

    void ResetDiagram()
    {
    }
}      
