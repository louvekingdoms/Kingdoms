using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Logger = KingdomsSharedCode.Generic.Logger;
using static KingdomsSharedCode.Networking.Controller;

using KingdomsSharedCode.Networking;
using System.IO;
using System.Text;

namespace Kingdoms.Network { 
    public class Client
    {
        public readonly int HEARTBEAT_FREQUENCY = 200;

        Socket client;

        bool hasSession = false;
        uint session = 0;

        string addr;
        int port;
        bool isSlave = false;

        bool shouldRun = true;
        System.Threading.Timer heartbeatInterval;

        public Client(string addr, int port, bool isSlave = false)
        {
            this.addr = addr;
            this.port = port;
            this.isSlave = isSlave;
        
            Initialize();
        }

        void Initialize()
        {
            client = new Socket(SocketType.Stream, ProtocolType.Tcp);
            client.Connect(IPAddress.Parse(addr), port);
            Logger.Info("Connected on " + addr + ":" + port);
        }

        public void Run()
        {
            heartbeatInterval = new System.Threading
                .Timer(
                e => {
                    using (NetworkStream stream = client.NewStream())
                    {
                        if (!stream.CanWrite)
                        {
                            Logger.Warn("Stream closed, reinitializing client...");
                            Initialize();
                            return;
                        }

                        stream.Write(new Message()
                        {
                            controller = (byte)RELAY_HEARTBEAT,
                            session = session,
                            body = Game.clock.GetBeat().ToString()
                        });
                    }
                },
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(HEARTBEAT_FREQUENCY));

                
            // Listens for messages
            while (client.Connected && shouldRun)
            {
                using (NetworkStream clientStream = client.NewStream())
                    if (clientStream.DataAvailable)
                        using (BinaryReader reader = new BinaryReader(clientStream, Encoding.UTF8, leaveOpen: true))
                        {
                            var msg = new Message(reader);
                            ExecuteMessage(msg);
                        }
            }

            // Client dies if it reaches this place
            heartbeatInterval.Dispose();
        }

        void ExecuteMessage(Message message)
        {
            Logger.Trace("RECEIVED: " + message);
            if (ControllerSet.set.ContainsKey(message.controller))
            {
                ControllerSet.set[message.controller].Execute(this, message);
            }
            else
            {
                Logger.Error("No such controller as " + message.controller);
                ControllerSet.relay.Execute(this, message);
            }
        }

        public void RequestJoinSession(uint id)
        {
            using (NetworkStream stream = client.NewStream())
            {
                stream.Write(new Message()
                {
                    controller = (byte)RELAY_JOIN_SESSION,
                    body = id.ToString()
                });
            }
        }

        public void RequestNewSession()
        {
            using (NetworkStream stream = client.NewStream())
            {
                // Hosting
                var msg = new Message()
                {
                    controller = (byte)RELAY_HOST_SESSION
                };
                stream.Write(msg);
            }
        }

        public void Broadcast(string message)
        {
            using (NetworkStream stream = client.NewStream())
            {
                var msg = new Message()
                {
                    controller = (byte)BROADCAST,
                    body = message,
                    session = session
                };
                stream.Write(msg);
            }
        }

        public void LeaveCurrentSession()
        {
            using (NetworkStream stream = client.NewStream())
                stream.Write(new Message()
                {
                    controller = (byte)RELAY_LEAVE_SESSION,
                    session = session
                });
            UnsetSession();
        }

        public void SetSession(uint id, ushort beat)
        {
            Logger.Info("Entering session " + id.ToString("X") + " at beat " + beat);
            Game.clock.Start(beat);
            hasSession = true;
            session = id;
        }

        void UnsetSession()
        {
            Logger.Info("Exiting " + session.ToString("X"));
            Game.clock.Pause();
            hasSession = false;
        }

        public void Kill()
        {
            shouldRun = false;
        }
    }
}
