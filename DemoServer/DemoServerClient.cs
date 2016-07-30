using SocketClientServerLib;

namespace DemoServer
{
    public class DemoServerClient : SslServerSessionBase, IDemoServerClient
    {
        public DemoServerClient(int receiveBufferSize) : base(receiveBufferSize)
        {
        }
    }
}