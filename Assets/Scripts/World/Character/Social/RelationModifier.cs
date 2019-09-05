using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RelationModifier : Clock.IExpirable
{
    bool isEternal = true;
    int timeRemaining = 0;
    int impact = 0;

    public Action OnDestroy;

    public void Age()
    {
        if (isEternal) return;

        timeRemaining--;
        if (timeRemaining <= 0 && OnDestroy != null)
        {
            OnDestroy.Invoke();
        }
    }

    public void RegisterClockReceiver()
    {
        Game.clock.RegisterClockReceiver(this);
    }

    public int GetImpact()
    {
        return impact;
    }
}