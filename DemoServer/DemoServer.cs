using System.Net.Sockets;
using SocketClientServerLib;

namespace DemoServer
{
    public class DemoServer : SslServerBase, IDemoServer
    {
        protected override ISslServerSessionBase CreateSslServerClient(TcpClient tcpClient)
        {
            return new DemoServerClient(new DefaultIncomingDataProcessor(new byte[] { 0xFF, 0xFF }), new DefaultOutgoingDataProcessor(new byte[] { 0xFF, 0xFF }, CompressionType.GZip), 4096);
        }
    }
}