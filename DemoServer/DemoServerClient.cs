using SocketClientServerLib;

namespace DemoServer
{
    public class DemoServerClient : SslServerSessionBase, IDemoServerClient
    {
        public DemoServerClient(int receiveBufferSize, int heartbeatInterval) : base(receiveBufferSize, heartbeatInterval)
        {
        }
    }
}