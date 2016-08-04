using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
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
        private bool _sendHeartbeat = true;
        private int _heartbeatInterval = 60000; // default 60s
        private readonly List<IIncomingClientChecker> _incomingClientCheckers = new List<IIncomingClientChecker>();

        public event Action<IServerBase, Exception> InternalError;
        public event Action<IServerBase, ServerState> StateChanged;
        public event Action<IServerBase, IServerSessionBase, SessionState> ClientStateChanged;
        public event Action<IServerBase, IServerSessionBase, Exception> ClientInternalError;
        public event Action<IServerBase, ISessionBase, Packet> ClientDataSent;
        public event Action<IServerBase, ISessionBase, Packet> ClientDataReceived;
        public event Action<IServerBase, ISessionBase, bool> ClientSendDataReady;

        public IPEndPoint EndPoint { get; private set; }

        public bool SendHeartbeat
        {
            get { return _sendHeartbeat; }
            set
            {
                if (State != ServerState.Stopping && State != ServerState.Stopped) throw new InvalidOperationException("Invalid server state.");
                _sendHeartbeat = value;
            }
        }

        public int HeartbeatInterval
        {
            get { return _heartbeatInterval; }
            set
            {
                if (State != ServerState.Stopping && State != ServerState.Stopped) throw new InvalidOperationException("Invalid server state.");
                _heartbeatInterval = value;
            }
        }

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

        public bool Start(IPAddress bindAddr, int port)
        {
            if (State != ServerState.Stopped) return false;
            lock (_lock)
            {
                if (State != ServerState.Stopped) return false;
                State = ServerState.Starting;
                try
                {
                    _tcpListener = new TcpListener(bindAddr, port);
                    _tcpListener.Start();
                    EndPoint = (IPEndPoint)_tcpListener.Server.LocalEndPoint;
                    _listenThread = new Thread(DoListen);
                    _listenThread.Start();
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
            List<IIncomingClientChecker> incomingClientCheckers = null;
            lock (_lock)
            {
                if (State == ServerState.Starting)
                {
                    State = ServerState.Started;
                    lock (_incomingClientCheckers)
                    {
                        incomingClientCheckers = _incomingClientCheckers.ToList();
                    }
                }
            }
            while (State == ServerState.Started)
            {
                try
                {
                    var tcpClient = _tcpListener.AcceptTcpClient();
                    try
                    {
                        var endPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
                        foreach (var icc in incomingClientCheckers)
                        {
                            if (!icc.Allow(tcpClient))
                            {
                                throw new UnauthorizedAccessException(string.Format("Incoming client checker \"{0}\" blocked client \"{1}:{2}\".", icc.Name, endPoint.Address, endPoint.Port));
                            }
                        }
                        var client = CreateServerClient(tcpClient);
                        client.StateChanged += ClientOnStateChanged;
                        client.InternalError += ClientOnInternalError;
                        client.DataReceived += ClientOnDataReceived;
                        client.DataSent += ClientOnDataSent;
                        client.SendHeartbeat = SendHeartbeat;
                        client.HeartbeatInterval = HeartbeatInterval;
                        client.SendDataReady += ClientOnSendDataReady;
                        client.AttachTcpClient(tcpClient);
                    }
                    catch (Exception ex)
                    {
                        ReportInternalError(ex);
                        tcpClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    if (State == ServerState.Started)
                    {
                        ReportInternalError(ex);
                        Stop();
                    }
                    // stopping or already stopped - not handle the error
                    return;
                }
            }
        }

        private void ClientOnSendDataReady(ISessionBase sessionBase, bool b)
        {
            if (ClientSendDataReady != null)
            {
                ClientSendDataReady(this, sessionBase, b);
            }
        }

        private void ClientOnDataSent(ISessionBase sessionBase, Packet packet)
        {
            if (ClientDataSent != null)
            {
                ClientDataSent(this, sessionBase, packet);
            }
        }

        private void ClientOnDataReceived(ISessionBase sessionBase, Packet packet)
        {
            if (ClientDataReceived != null)
            {
                ClientDataReceived(this, sessionBase, packet);
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
            if (State != ServerState.Started) return false;
            lock (_lock)
            {
                if (State != ServerState.Started) return false;
                State = ServerState.Stopping;
                try
                {
                    _tcpListener.Stop();
                    if (!Thread.CurrentThread.Equals(_listenThread))
                    {
                        _listenThread.Join();
                    }
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

        public void AddIncomingClientChecker(IIncomingClientChecker incomingClientChecker)
        {
            if (State != ServerState.Stopping && State != ServerState.Stopped) throw new InvalidOperationException("Invalid server state.");
            lock (_incomingClientCheckers)
            {
                _incomingClientCheckers.Add(incomingClientChecker);
            }
        }

        public void RemoveIncomingClientChecker(IIncomingClientChecker incomingClientChecker)
        {
            if (State != ServerState.Stopping && State != ServerState.Stopped) throw new InvalidOperationException("Invalid server state.");
            lock (_incomingClientCheckers)
            {
                _incomingClientCheckers.Remove(incomingClientChecker);
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