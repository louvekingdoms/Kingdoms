using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using KingdomsSharedCode.Networking;
using static KingdomsSharedCode.Generic.Logger;

namespace KingdomsGame.Networking.Controllers
{
    class CON_Go : Controller
    {
        public override void Execute(Client me, Message message)
        {
            Game.clock.Play();
        }
    }
}
