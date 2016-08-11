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

        int ReadTimeout { get; set; }

        int WriteTimeout { get; set; }

        bool SendHeartbeat { get; set; }

        int HeartbeatInterval { get; set; }

        void EnsureSendDataReady(int timeout);

        bool AttachTcpClient(TcpClient tcpClient);

        bool Disconnect();

        bool SendData(Packet data);
    }
}