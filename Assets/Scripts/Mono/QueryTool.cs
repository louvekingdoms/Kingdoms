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
        var reg = displayer.GetSelectedRegion();
        if (reg == null) return;

        text.text = string.Join("\n", new List<string>() {
            "owner: "+(reg.owner!=null?reg.owner.name:"none"),
            "population: "+reg.population,
            "capital: "+reg.sites[reg.capital].Coord.ToVector2(),
            "elevation: "+reg.elevation,
            "moisture: "+reg.moisture
        }.ToArray());
    }
}
