namespace SocketClientServerLib
{
    public abstract class ServerSessionBase : SessionBase, IServerSessionBase
    {
        protected ServerSessionBase(IIncomingDataProcessor incomingDataProcessor, IOutgoingDataProcessor outgoingDataProcessor, int receiveBufferSize) : base(incomingDataProcessor, outgoingDataProcessor, receiveBufferSize)
        {
        }
    }
}