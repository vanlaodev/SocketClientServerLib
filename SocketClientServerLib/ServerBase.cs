using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketClientServerLib
{
    public abstract class ServerBase : IServerBase
    {
        private readonly List<IServerSessionBase> _clients = new List<IServerSessionBase>();
        private readonly object _lock = new object();
        private Thread _listenThread;
        private volatile ServerState _state = ServerState.Stopped;
        private TcpListener _tcpListener;

        public event Action<IServerBase, Exception> InternalError;
        public event Action<IServerBase, ServerState> StateChanged;
        public event Action<IServerBase, IServerSessionBase, SessionState> ClientStateChanged;
        public event Action<IServerBase, IServerSessionBase, Exception> ClientInternalError;

        public List<IServerSessionBase> Clients
        {
            get
            {
                lock (_clients)
                {
                    return _clients.ToList();
                }
            }
        }

        public ServerState State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    if (StateChanged != null)
                    {
                        StateChanged(this, value);
                    }
                }
            }
        }

        protected void ReportInternalError(Exception ex)
        {
            if (ex != null && InternalError != null)
            {
                InternalError(this, ex);
            }
        }

        public bool Start(int port)
        {
            lock (_lock)
            {
                if (State != ServerState.Stopped) return false;
                State = ServerState.Starting;
                try
                {
                    _tcpListener = new TcpListener(IPAddress.Any, port);
                    _tcpListener.Start();
                    _listenThread = new Thread(DoListen);
                    _listenThread.Start();
                    State = ServerState.Started;
                    return true;
                }
                catch
                {
                    State = ServerState.Stopped;
                    throw;
                }
            }
        }

        private void DoListen()
        {
            while (State == ServerState.Started)
            {
                try
                {
                    var tcpClient = _tcpListener.AcceptTcpClient();
                    var client = CreateServerClient(tcpClient);
                    client.StateChanged += ClientOnStateChanged;
                    client.InternalError += ClientOnInternalError;
                    client.AttachTcpClient(tcpClient);
                }
                catch (Exception ex)
                {
                    ReportInternalError(ex);
                    if (State == ServerState.Started)
                    {
                        Stop();
                    }
                    break;
                }
            }
        }

        private void ClientOnInternalError(ISessionBase sessionBase, Exception exception)
        {
            if (ClientInternalError != null)
            {
                ClientInternalError(this, (IServerSessionBase)sessionBase, exception);
            }
        }

        private void ClientOnStateChanged(ISessionBase sessionBase, SessionState sessionState)
        {
            if (ClientStateChanged != null)
            {
                ClientStateChanged(this, (IServerSessionBase)sessionBase, sessionState);
            }
            if (sessionState == SessionState.Connected)
            {
                lock (_lock)
                {
                    if (sessionState != SessionState.Connected) return;
                    lock (_clients)
                    {
                        _clients.Add((IServerSessionBase)sessionBase);
                    }
                }
            }
            else if (sessionState == SessionState.Disconnected)
            {
                lock (_clients)
                {
                    _clients.Remove((IServerSessionBase)sessionBase);
                }
            }
        }

        protected abstract IServerSessionBase CreateServerClient(TcpClient tcpClient);

        public bool Stop()
        {
            lock (_lock)
            {
                if (State != ServerState.Started) return false;
                State = ServerState.Stopping;
                try
                {
                    _tcpListener.Stop();
                    var clients = Clients;
                    foreach (var client in clients)
                    {
                        client.Disconnect();
                    }
                    return true;
                }
                finally
                {
                    State = ServerState.Stopped;
                }
            }
        }

        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch
            {
                // ignored
            }
        }

    }
}