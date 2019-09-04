using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using KingdomsSharedCode.Networking;
using static GameLogger;

namespace KingdomsGame.Networking.Controllers
{
    class CON_Broadcast : Controller
    {
        public override void Execute(Client me, Message message)
        {
            logger.Debug("Received broadcast: " + message.body);
        }
    }
}
