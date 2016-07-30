using System.Net.Sockets;

namespace SocketClientServerLib
{
    public abstract class ClientSessionBase : SessionBase, IClientSessionBase
    {
        protected ClientSessionBase(int receiveBufferSize, int heartbeatInterval) : base(receiveBufferSize, heartbeatInterval)
        {
        }

        public bool Connect(string host, int port)
        {
            return AttachTcpClient(() =>
            {
                State = SessionState.Connecting;
                return new TcpClient(host, port);
            });
        }
    }
}