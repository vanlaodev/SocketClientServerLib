using System;
using System.Net;
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

        IPEndPoint EndPoint { get; }

        SessionState State { get; }

        bool SendHeartbeat { get; set; }

        int HeartbeatInterval { get; set; }

        bool AttachTcpClient(TcpClient tcpClient);

        bool Disconnect();

        bool SendData(Packet data);
    }
}