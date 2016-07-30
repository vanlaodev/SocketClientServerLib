using SocketClientServerLib;

namespace DemoClient
{
    public class DemoClient : SslClientSessionBase, IDemoClient
    {
        public DemoClient(int receiveBufferSize) : base(receiveBufferSize)
        {
        }
    }
}