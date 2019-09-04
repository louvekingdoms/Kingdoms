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

        if (reg.IsOwned()) {
            var info = new List<string>() { reg.GetOwner().name + " (" + reg.GetOwner().GetMainRace().GetPlural() + ")" };
            foreach(var rsc in reg.GetOwner().resources.Keys) {
                info.Add(rsc + ": " + reg.GetOwner().resources[rsc].GetValue().FloorToInt().ToString("D"));
            }
            kingdom = " - "+string.Join("\n - ", info);
        }

        var regionRsc = new List<string>();
        foreach (var rsc in reg.resources.Keys) {
            regionRsc.Add(rsc + ": " + reg.resources[rsc].GetValue().FloorToInt().ToString("D"));
        }

        text.text = string.Join("\n - ", new List<string>() {
            "kingdom: "+kingdom,
            "capital: "+reg.sites[reg.capital].Coord.ToVector2(),
            "------",
            string.Join("\n - ", regionRsc),
            "------",
            "elevation: "+reg.topography.elevation,
            "moisture: "+reg.topography.moisture,
            "temperature: "+reg.topography.temperature
        }.ToArray());
    }
}
