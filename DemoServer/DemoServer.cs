using System.Net.Sockets;
using SocketClientServerLib;

namespace DemoServer
{
    public class DemoServer : SslServerBase, IDemoServer
    {
        protected override ISslServerSessionBase CreateSslServerClient(TcpClient tcpClient)
        {
            return new DemoServerClient(4096);
        }
    }
}