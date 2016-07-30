using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SocketClientServerLib;

namespace DemoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IDemoClient client = new DemoClient();
            client.InternalError += ClientOnInternalError;
            client.StateChanged += ClientOnStateChanged;
            client.UseSsl = true;
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
                    }
                }
            } while (!quit);

            client.Dispose();
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
