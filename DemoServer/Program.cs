using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using DemoCommon;
using SocketClientServerLib;

namespace DemoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IDemoServer server = new DemoServer();
            server.InternalError += ServerOnInternalError;
            server.StateChanged += ServerOnStateChanged;
            server.ClientStateChanged += ServerOnClientStateChanged;
            server.ClientInternalError += ServerOnClientInternalError;
            server.ClientDataReceived += ServerOnClientDataReceived;
            server.ClientDataSent += ServerOnClientDataSent;
            server.ClientSendDataReady += ServerOnClientSendDataReady;
            server.UseSsl = true;
            server.HeartbeatInterval = 5000;
            //            server.SendHeartbeat = false;
            server.ServerCertificate = new X509Certificate2("DemoServer.pfx", "green");
            server.AddSecurityChecker(new GenericSecurityChecker("Whitelist", client =>
            {
                var endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                return new[] { "127.0.0.1" }.Contains(endPoint.Address.ToString());
            }));
            StartServer(server);

            var quit = false;
            do
            {
                var input = Console.ReadLine();
                if (input != null)
                {
                    switch (input.ToLower())
                    {
                        case "clients":
                            Console.WriteLine("Clients: " + server.Clients.Count);
                            break;
                        case "start":
                            StartServer(server);
                            break;
                        case "stop":
                            StopServer(server);
                            break;
                        case "status":
                            Console.WriteLine("Status: " + server.State);
                            break;
                        case "q":
                        case "quit":
                        case "exit":
                            quit = true;
                            break;
                        default:
                            server.Clients.ForEach(client => ((IDemoServerClient)client).SendAndForget(input));
                            break;
                    }
                }
            } while (!quit);

            server.Dispose();
        }

        private static void ServerOnClientSendDataReady(IServerBase serverBase, ISessionBase sessionBase, bool arg3)
        {
            if (arg3)
            {
                Console.WriteLine("Client send data ready");
            }
        }

        private static void ServerOnClientDataSent(IServerBase serverBase, ISessionBase sessionBase, Packet arg3)
        {
            //            Console.WriteLine("Client data sent: " + Encoding.UTF8.GetString(arg3.Data));
        }

        private static void ServerOnClientDataReceived(IServerBase serverBase, ISessionBase sessionBase, Packet arg3)
        {
            if (arg3 is HeartbeatPacket)
            {
                Console.WriteLine("Heartbeat.");
                return;
            }
            Console.WriteLine("Client data received: " + Encoding.UTF8.GetString(arg3.Data));
            if (arg3 is DemoPacket)
            {
                var dp = (DemoPacket)arg3;
                //                Thread.Sleep(5000); // simulate IO works
                ((IDemoServerClient)sessionBase).Reply(dp.Id, dp.Text); // echo
            }
        }

        private static void ServerOnClientInternalError(IServerBase serverBase, IServerSessionBase serverSessionBase, Exception arg3)
        {
            Console.WriteLine("client Internal error: " + arg3.Message);
        }

        private static void ServerOnClientStateChanged(IServerBase serverBase, IServerSessionBase serverSessionBase, SessionState arg3)
        {
            if (arg3 == SessionState.Connected)
            {
                Console.WriteLine("Client connected.");
            }
            else if (arg3 == SessionState.Disconnected)
            {
                Console.WriteLine("Client disconnected.");
            }
        }

        private static void StopServer(IDemoServer server)
        {
            try
            {
                if (!server.Stop())
                {
                    Console.WriteLine("Stopping or already stopped.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static void StartServer(IDemoServer server)
        {
            try
            {
                var defaultPort = 54321;
                var defaultUseSSL = true;
                Console.Write("Enter port [{0}]: ", defaultPort);
                var input = Console.ReadLine();
                var port = string.IsNullOrEmpty(input) ? defaultPort : int.Parse(input);
                Console.Write("Use SSL [{0}]: ", defaultUseSSL);
                input = Console.ReadLine();
                var useSSL = string.IsNullOrEmpty(input) ? defaultUseSSL : bool.Parse(input);
                server.UseSsl = useSSL;
                if (!server.Start(port))
                {
                    Console.WriteLine("Starting or already started.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static void ServerOnInternalError(IServerBase serverBase, Exception exception)
        {
            if (exception is AggregateException)
            {
                foreach (var ie in ((AggregateException)exception).InnerExceptions)
                {
                    Console.WriteLine("Internal error: " + ie);
                }
                return;
            }
            Console.WriteLine("Internal error: " + exception.Message);
        }

        private static void ServerOnStateChanged(IServerBase serverBase, ServerState serverState)
        {
            Console.WriteLine("State changed: " + serverState);
        }
    }
}
