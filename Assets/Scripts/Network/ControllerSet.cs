using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KingdomsSharedCode.Networking.Controller;
using KingdomsGame.Networking.Controllers;

namespace KingdomsGame.Networking
{
    public class ControllerSet
    {
        public static Dictionary<byte, Controller> set = new Dictionary<byte, Controller>();
        public static Controller relay = new Controller();

        static ControllerSet()
        {
            set.Add(    (byte)BROADCAST,        new CON_Broadcast());
            set.Add(    (byte)SESSION_INFO,     new CON_SessionInfo());
            set.Add(    (byte)GO,               new CON_Go());
            set.Add(    (byte)WAIT,             new CON_Wait());
            set.Add(    (byte)DESYNCHRONIZED,   new CON_Desynchronized());
            set.Add(    (byte)CHAT,             new CON_Chat());
        }
    }
}
