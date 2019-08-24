using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using KingdomsSharedCode.Networking;
using static KingdomsSharedCode.Generic.Logger;

namespace Kingdoms.Network.Controllers
{
    class CON_Desynchronized : Controller
    {
        public override void Execute(Client me, Message message)
        {
            GameLogger.logger.Error("Simulation desynchronized between clients.");
        }
    }
}
