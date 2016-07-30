using System.Net.Sockets;

namespace SocketClientServerLib
{
    public abstract class ClientSessionBase : SessionBase, IClientSessionBase
    {
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