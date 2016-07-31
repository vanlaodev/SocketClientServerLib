using System.Net.Sockets;
using DemoCommon;
using SocketClientServerLib;

namespace DemoServer
{
    public class DemoServer : SslServerBase, IDemoServer
    {
        protected override ISslServerSessionBase CreateSslServerClient(TcpClient tcpClient)
        {
            return new DemoServerClient(new DemoIncomingDataProcessor(new byte[] { 0xFF, 0xFF }), new DemoOutgoingDataProcessor(new byte[] { 0xFF, 0xFF }, CompressionType.GZip), 4096);
        }
    }
}