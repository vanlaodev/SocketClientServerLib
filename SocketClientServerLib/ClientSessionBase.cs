﻿using System;
using System.Net.Sockets;

namespace SocketClientServerLib
{
    public abstract class ClientSessionBase : SessionBase, IClientSessionBase
    {
        private int _reconnectInterval = 15000; // default 15s
        private readonly ClientReconnectWorker _clientReconnectWorker;

        public int LastPort { get; private set; }
        public string LastHost { get; private set; }
        public bool AutoReconnect { get; set; }
        public int ReconnectInterval { get; set; }

        protected ClientSessionBase(int receiveBufferSize) : base(receiveBufferSize)
        {
            _clientReconnectWorker = new ClientReconnectWorker();
            _clientReconnectWorker.StateChanged += ClientReconnectWorkerOnStateChanged;

            AutoReconnect = true;

            StateChanged += OnStateChanged;
        }

        private void ClientReconnectWorkerOnStateChanged(ClientReconnectWorker clientReconnectWorker, ClientReconnectWorker.ClientReconnectWorkerState clientReconnectWorkerState)
        {

        }

        private void OnStateChanged(ISessionBase sessionBase, SessionState sessionState)
        {
            if (AutoReconnect && sessionState == SessionState.Disconnected)
            {
                StartReconnect();
            }
            else if (sessionState == SessionState.Connected)
            {
                StopReconnect();
            }
        }

        public bool Connect(string host, int port)
        {
            try
            {
                LastHost = host;
                LastPort = port;
                return AttachTcpClient(() =>
                {
                    State = SessionState.Connecting;
                    return new TcpClient(host, port);
                });
            }
            catch (Exception ex)
            {
                ReportInternalError(ex);
                if (AutoReconnect)
                {
                    StartReconnect();
                    return false;
                }
                throw;
            }
        }

        private void StartReconnect()
        {
            _clientReconnectWorker.Start(this, LastHost, LastPort, ReconnectInterval);
        }

        private void StopReconnect()
        {
            _clientReconnectWorker.Stop();
        }

        public override void Dispose()
        {
            AutoReconnect = false;
            _clientReconnectWorker.Dispose();
            base.Dispose();
        }
    }
}