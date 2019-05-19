using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QueryTool : MonoBehaviour
{
    public TextMeshProUGUI text;

    MapDisplayer displayer;

    void Start()
    {
        displayer = FindObjectOfType<MapDisplayer>();
    }

    // Update is called once per frame
    void Update()
    {

        if (displayer == null) return;
        var reg = displayer.GetSelectedRegion();
        if (reg == null) return;
        string kingdom = "none";

        if (reg.owner != null) {
            var info = new List<string>() { reg.owner.name + " (" + reg.owner.GetMainRace().GetPlural() + ")" };
            foreach(var rsc in reg.owner.resources.Keys) {
                info.Add(rsc + ": " + reg.owner.resources[rsc].GetValue());
            }
            kingdom = string.Join("\n", info);
        }

        text.text = string.Join("\n", new List<string>() {
            "kingdom: "+kingdom,
            "population: "+reg.population,
            "capital: "+reg.sites[reg.capital].Coord.ToVector2(),
            "elevation: "+reg.elevation,
            "moisture: "+reg.moisture
        }.ToArray());
    }
}
