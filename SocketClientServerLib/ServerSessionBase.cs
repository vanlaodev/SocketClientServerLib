namespace SocketClientServerLib
{
    public abstract class ServerSessionBase : SessionBase, IServerSessionBase
    {
        protected ServerSessionBase(int receiveBufferSize, int heartbeatInterval) : base(receiveBufferSize, heartbeatInterval)
        {
        }
    }
}