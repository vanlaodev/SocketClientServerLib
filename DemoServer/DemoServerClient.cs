using SocketClientServerLib;

namespace DemoServer
{
    public class DemoServerClient : SslServerSessionBase, IDemoServerClient
    {
        public DemoServerClient(IIncomingDataProcessor incomingDataProcessor, IOutgoingDataProcessor outgoingDataProcessor, int receiveBufferSize) : base(incomingDataProcessor, outgoingDataProcessor, receiveBufferSize)
        {
        }
    }
}