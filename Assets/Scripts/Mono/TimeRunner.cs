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
    Clock clock;

    // Start is called before the first frame update
    void Start()
    {
        clock = new Clock(Rules.set[RULE.DAYS_IN_YEAR].GetInt(), Rules.set[RULE.MONTHS_IN_YEAR].GetInt());
    }

    // Update is called once per frame
    void Update()
    {
        clock.SetYearDurationSeconds(yearDurationSeconds);
        var incrementer = Time.deltaTime / clock.GetTimeScale() ;
        i += incrementer;

        if (i > 1f) {
            var adv = clock.Advance();
            if (!adv) {
                Logger.Warn("The clock is running behind! Date is "+clock.GetDate().ToString()+", timescale is "+clock.GetTimeScale()+". Skipping advance...");
            }
            i = 0f;
        }

        if (display != null) display.text = clock.GetDate().ToString();
    }
}
