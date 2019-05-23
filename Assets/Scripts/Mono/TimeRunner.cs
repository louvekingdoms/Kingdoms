using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeRunner : MonoBehaviour
{
    public int yearLength;
    public int monthCount;
    public TextMeshProUGUI display;
    public float yearDurationSeconds = 100f; 

    float i;

    // Start is called before the first frame update
    void Awake()
    {
        Game.clock = new Clock(Rules.set[RULE.DAYS_IN_YEAR].GetInt(), Rules.set[RULE.MONTHS_IN_YEAR].GetInt());
    }

    // Update is called once per frame
    void Update()
    {
        Game.clock.SetYearDurationSeconds(yearDurationSeconds);
        var incrementer = Time.deltaTime / Game.clock.GetTimeScale() ;
        i += incrementer;

        if (i > 1f) {
            var adv = Game.clock.Advance();
            if (!adv) {
                Logger.Warn("The clock is running behind! Date is "+ Game.clock.GetDate().ToString()+", timescale is "+ Game.clock.GetTimeScale()+". Skipping advance...");
            }
            i = 0f;
            display.color = !adv ? Color.red : Color.white;
        }

        if (display != null) {
            display.text = Game.clock.GetDate().ToString();
        }
    }
}
