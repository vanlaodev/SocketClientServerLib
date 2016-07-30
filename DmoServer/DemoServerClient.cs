using SocketClientServerLib;

namespace DmoServer
{
    public class DemoServerClient : SslServerSessionBase, IDemoServerClient
    {
        public DemoServerClient(int receiveBufferSize) : base(receiveBufferSize)
        {
        }
    }
}