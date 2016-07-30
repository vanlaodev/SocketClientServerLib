using SocketClientServerLib;

namespace DemoClient
{
    public class DemoClient : SslClientSessionBase, IDemoClient
    {
        public DemoClient(IIncomingDataProcessor incomingDataProcessor, IOutgoingDataProcessor outgoingDataProcessor, int receiveBufferSize) : base(incomingDataProcessor, outgoingDataProcessor, receiveBufferSize)
        {
        }
    }
}