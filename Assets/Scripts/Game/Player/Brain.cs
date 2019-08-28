using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Brain
{
    Ruler ruler;

    public abstract bool IsLocal();
    public abstract int GetId();
    public virtual void Own(Ruler ruler)
    {
        this.ruler = ruler;
        ruler.brain = this;
    }
    public virtual bool CanSeeSecretsOf(Ruler ruler)
    {
        if (ruler.brain == this)
        {
            return true;
        }
        return false;
    }
}
