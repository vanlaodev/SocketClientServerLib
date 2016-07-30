using System.Net.Sockets;
using SocketClientServerLib;

namespace DmoServer
{
    public class DemoServer : ServerBase, IDemoServer
    {
        protected override IServerSessionBase CreateServerClient(TcpClient tcpClient)
        {
            return new DemoServerClient();
        }
    }
}