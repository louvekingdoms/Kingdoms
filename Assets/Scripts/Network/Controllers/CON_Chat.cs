using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using KingdomsSharedCode.JSON;
using KingdomsSharedCode.Networking;

namespace KingdomsGame.Networking.Controllers
{
    class CON_Chat : Controller
    {
        public override void Execute(Client me, Message message)
        {
            var body = JSON.Parse(message.body);

            Game.clock.Plan(message.beat, delegate
            {
                Game.chat.OnNewMessage.Invoke(
                    body["author"].ToString(),
                    body["content"].ToString()
                );
            });
        }
    }
}
