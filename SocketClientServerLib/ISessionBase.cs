using System;
using System.Net.Sockets;

namespace SocketClientServerLib
{
    public interface ISessionBase : IDisposable
    {
        event Action<ISessionBase, SessionState> StateChanged;
        event Action<ISessionBase, Exception> InternalError;
        event Action<ISessionBase, Packet> DataSent;
        event Action<ISessionBase, Packet> DataReceived;
        event Action<ISessionBase, bool> SendDataReady;

        SessionState State { get; }

        bool AttachTcpClient(TcpClient tcpClient);

        bool Disconnect();

        bool SendData(Packet data);
    }
}