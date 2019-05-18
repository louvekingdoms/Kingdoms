using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Ruler : Character
{
    public Ruler() : base()
    {
        name.preTitle = race.rulerTitle;
    }

    public new class CreationRules : Character.CreationRules
    {
        public int stock = 15;
        public float lifespanToStockRatio = 0.8f;
        public int maxStartingAge;
    }

}
