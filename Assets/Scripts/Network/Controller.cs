using KingdomsSharedCode.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KingdomsGame.Networking
{
    public class Controller
    {
        public virtual void Execute(Client client, Message message)
        {
            throw new NotImplementedException();
        }
    }
}
