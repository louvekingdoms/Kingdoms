using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using KingdomsSharedCode.Networking;
using KingdomsSharedCode.JSON;
using static KingdomsSharedCode.Generic.Logger;

namespace KingdomsGame.Networking.Controllers
{
    class CON_SessionInfo : Controller
    {
        public override void Execute(Client me, Message message)
        {
            var data = JSON.Parse(message.body);
            
            me.SetSession(
                Convert.ToUInt32(data["session"].AsInt), 
                Convert.ToUInt16(data["beat"].AsInt)
            );
        }
    }
}
