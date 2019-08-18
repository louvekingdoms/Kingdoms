using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Logger = KingdomsSharedCode.Generic.Logger;

public class TimeRunner : MonoBehaviour
{
    public int yearLength;
    public int monthCount;
    public TextMeshProUGUI display;
    [Range(1, 10)] public int beatsPerDay = 5;

    float i;

    // Start is called before the first frame update
    void Awake()
    {
        Game.clock = new Clock();
        Game.clock.SetCalendar(Rules.set[RULE.DAYS_IN_YEAR].GetInt(), Rules.set[RULE.MONTHS_IN_YEAR].GetInt());
    }

    private void Start()
    {
        Game.clock.Start();
    }

    // Update is called once per frame
    void Update()
    {
        Game.clock.SetBeatsPerDay(beatsPerDay);

        if (display != null)
        {
            display.text = Game.clock.GetDate().ToString()+" - "+Game.state.Sum();
        }
    }
}
