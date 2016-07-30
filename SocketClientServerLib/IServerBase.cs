﻿using System;
using System.Collections.Generic;

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

        List<IServerSessionBase> Clients { get; }

        ServerState State { get; }

        bool Start(int port);

        bool Stop();
    }
}