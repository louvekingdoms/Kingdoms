using KingdomsSharedCode.JSON;
using KingdomsSharedCode.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Chat
{
    public Action<string, string> OnNewMessage;

    public void SendMessage(string content)
    {
        var body = new JSONObject();
        body["author"] = Environment.UserName;
        body["content"] = content;

        var msg = new Message()
        {
            controller = (byte)Controller.CHAT,
            beat = Game.clock.GetNextPlannableBeat(),
            body = body.ToString()
        };
        Game.networkClient.SendGeneric(msg);
    }
}
