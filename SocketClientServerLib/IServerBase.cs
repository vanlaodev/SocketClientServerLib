using System;
using System.Collections.Generic;

namespace SocketClientServerLib
{
    public interface IServerBase : IDisposable
    {
        event Action<IServerBase, Exception> InternalError;
        event Action<IServerBase, ServerState> StateChanged;
        event Action<IServerBase, IServerSessionBase, SessionState> ClientStateChanged;
        event Action<IServerBase, IServerSessionBase, Exception> ClientInternalError;

        List<IServerSessionBase> Clients { get; }

        ServerState State { get; }

        bool Start(int port);

        bool Stop();
    }
}