using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class GameFlags
{
    public enum ReleaseType {RELEASE, DEMO, DEBUG };

    public readonly ReleaseType RELEASE_TYPE;
    public bool IS_OMNISCIENT = false;





}
