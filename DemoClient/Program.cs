using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DemoCommon;
using SocketClientServerLib;

namespace DemoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IDemoClient client = new DemoClient(new DemoIncomingDataProcessor(new byte[] { 0xFF, 0xFF }), new DemoOutgoingDataProcessor(new byte[] { 0xFF, 0xFF }, CompressionType.GZip), 4096);
            client.ReconnectInterval = 5000;
            client.HeartbeatInterval = 5000;
            client.InternalError += ClientOnInternalError;
            client.StateChanged += ClientOnStateChanged;
            client.DataReceived += ClientOnDataReceived;
            client.SendDataReady += ClientOnSendDataReady;
            client.DataSent += ClientOnDataSent;
            client.UseSsl = true;
            client.AutoReconnect = true;
            //            client.SendHeartbeat = false;
            client.ServerCn = "DemoServer";
            client.ClientCertificate = new X509Certificate2("DemoClient.pfx", "green");
            Connect(client);

            var quit = false;
            do
            {
                var input = Console.ReadLine();
                if (input != null)
                {
                    switch (input.ToLower())
                    {
                        case "connect":
                            Connect(client);
                            break;
                        case "disconnect":
                            Disconnect(client);
                            break;
                        case "status":
                            Console.WriteLine("Status: " + client.State);
                            break;
                        case "q":
                        case "quit":
                        case "exit":
                            quit = true;
                            break;
                        default:
                            //                            client.SendAndForget(input);
                            EchoTest(client, input);
                            break;
                    }
                }
            } while (!quit);

            client.Dispose();
        }

        private static void EchoTest(IDemoClient client, string input)
        {
            try
            {
                var reply = client.SendAndWaitReply(input, 5000);
                Console.WriteLine("Replied: " + reply);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Echo test error: " + ex.Message);
            }
        }

        private static void ClientOnSendDataReady(ISessionBase sessionBase, bool b)
        {
            if (b)
            {
                Console.WriteLine("Send data ready");
            }
        }

        private static void ClientOnDataSent(ISessionBase sessionBase, Packet packet)
        {
            //            Console.WriteLine("Data sent: " + Encoding.UTF8.GetString(packet.Data));
        }

        private static void ClientOnDataReceived(ISessionBase sessionBase, Packet packet)
        {
            if (packet is HeartbeatPacket)
            {
                Console.WriteLine("Heartbeat.");
                return;
            }
            if (packet is DemoPacket && !string.IsNullOrEmpty(((DemoPacket)packet).ReplyId))
            {
                return; // ignored the echo packet
            }
            Console.WriteLine("Data received: " + Encoding.UTF8.GetString(packet.Data));
        }

        private static void Disconnect(IDemoClient client)
        {
            try
            {
                if (!client.Disconnect())
                {
                    Console.WriteLine("Disconnecting or already disconnected.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static void Connect(IDemoClient client)
        {
            try
            {
                if (!client.Connect("localhost", 54321))
                {
                    Console.WriteLine("Connecting or already connected.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static void ClientOnStateChanged(ISessionBase sessionBase, SessionState sessionState)
        {
            Console.WriteLine("State changed: " + sessionState);
        }

        private static void ClientOnInternalError(ISessionBase sessionBase, Exception exception)
        {
            Console.WriteLine("Internal error: " + exception.Message);
        }
    }
}
