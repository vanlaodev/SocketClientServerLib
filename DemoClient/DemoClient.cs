using SocketClientServerLib;

namespace DemoClient
{
    public class DemoClient : SslClientSessionBase, IDemoClient
    {
        public DemoClient(int receiveBufferSize, int heartbeatInterval) : base(receiveBufferSize, heartbeatInterval)
        {
        }
    }
}