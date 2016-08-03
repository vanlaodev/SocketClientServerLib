using System;
using System.Collections.Generic;
using System.Net;

namespace SocketClientServerLib
{
    public interface IServerBase : IDisposable
    {
        event Action<IServerBase, Exception> InternalError;
        event Action<IServerBase, ServerState> StateChanged;
        event Action<IServerBase, IServerSessionBase, SessionState> ClientStateChanged;
        event Action<IServerBase, IServerSessionBase, Exception> ClientInternalError;
        event Action<IServerBase, ISessionBase, Packet> ClientDataSent;
        event Action<IServerBase, ISessionBase, Packet> ClientDataReceived;
        event Action<IServerBase, ISessionBase, bool> ClientSendDataReady;

        IPEndPoint EndPoint { get; }

        bool SendHeartbeat { get; set; }

        int HeartbeatInterval { get; set; }

        List<IServerSessionBase> Clients { get; }

        ServerState State { get; }

        bool Start(IPAddress bind, int port);

        bool Stop();

        void AddIncomingClientChecker(IIncomingClientChecker incomingClientChecker);

        void RemoveIncomingClientChecker(IIncomingClientChecker incomingClientChecker);
    }
}