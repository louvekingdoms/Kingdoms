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
        public static Logger logger;

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
            logger.Info("Connected on " + addr + ":" + port);
        }

        public void Run()
        {
            logger.Debug("Starting heartbeat for client and listening");
            heartbeatInterval = new System.Threading
                .Timer(
                e => {
                    using (NetworkStream stream = client.NewStream())
                    {
                        if (!stream.CanWrite)
                        {
                            logger.Warn("Stream closed, reinitializing client...");
                            Initialize();
                            return;
                        }

                        WriteToStream(stream, new Message()
                        {
                            controller = (byte)RELAY_HEARTBEAT,
                            session = session,
                            sumAtBeat = Game.state == null ? null : Game.state.Sum(),
                            body = Game.clock.GetBeat().ToString()
                        });
                    }
                },
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(HEARTBEAT_FREQUENCY));


            // Listens for messages
            using (NetworkStream clientStream = client.NewStream())
                while (client.Connected && shouldRun)
                {
                    if (clientStream.DataAvailable)
                        using (BinaryReader reader = new BinaryReader(clientStream, Encoding.UTF8, leaveOpen: true))
                        {
                            var msg = new Message(reader);
                            logger.Trace("<< " + msg);
                            ExecuteMessage(msg);
                        }
                }

            // Client dies if it reaches this place
            heartbeatInterval.Dispose();
        }

        void ExecuteMessage(Message message)
        {
            if (ControllerSet.set.ContainsKey(message.controller))
            {
                ControllerSet.set[message.controller].Execute(this, message);
            }
            else
            {
                logger.Error("No such controller as " + message.controller);
                ControllerSet.relay.Execute(this, message);
            }
        }

        public void RequestJoinSession(uint id)
        {
            using (NetworkStream stream = client.NewStream())
            {
                WriteToStream(stream, new Message()
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
                WriteToStream(stream, msg);
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
                WriteToStream(stream, msg);
            }
        }

        public void LeaveCurrentSession()
        {
            using (NetworkStream stream = client.NewStream())
                WriteToStream(stream, new Message()
                {
                    controller = (byte)RELAY_LEAVE_SESSION,
                    session = session
                });
            UnsetSession();
        }

        public void SetSession(uint id, ushort beat)
        {
            logger.Info("Entering session " + id.ToString("X") + " at beat " + beat);
            Game.clock.Start(beat);
            hasSession = true;
            session = id;
        }

        void UnsetSession()
        {
            logger.Info("Exiting " + session.ToString("X"));
            Game.clock.Pause();
            hasSession = false;
        }

        public void Kill()
        {
            shouldRun = false;
        }

        void WriteToStream(NetworkStream stream, Message msg)
        {
            logger.Trace(">> "+msg);
            stream.Write(msg);
        }
    }
}
