using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Logger = KingdomsSharedCode.Generic.Logger;
using System;

public class TimeRunner : MonoBehaviour
{
    public TextMeshProUGUI display;
    [Range(1, 10)] public int beatsPerDay = 5;

    // Update is called once per frame
    void Update()
    {
        Game.clock.SetBeatsPerDay(beatsPerDay);

        if (display != null)
        {
            display.text = Game.clock.GetDate().ToString()+" - "+BitConverter.ToString(Game.state.Sum());
        }
    }
}
