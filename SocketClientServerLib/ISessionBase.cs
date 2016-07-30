using System;
using System.Net.Sockets;

namespace SocketClientServerLib
{
    public interface ISessionBase : IDisposable
    {
        event Action<ISessionBase, SessionState> StateChanged;
        event Action<ISessionBase, Exception> InternalError;

        SessionState State { get; }

        bool AttachTcpClient(TcpClient tcpClient);

        bool Disconnect();
    }
}