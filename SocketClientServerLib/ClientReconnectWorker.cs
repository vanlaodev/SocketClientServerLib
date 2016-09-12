using System;
using System.Threading;

namespace SocketClientServerLib
{
    internal class ClientReconnectWorker : IDisposable
    {
        private const int MaxBackOffMultiplexer = 16;

        private volatile ClientReconnectWorkerState _state = ClientReconnectWorkerState.Stopped;
        private readonly object _lock = new object();
        private Thread _workerThread;
        public event Action<ClientReconnectWorker, ClientReconnectWorkerState> StateChanged;

        public ClientReconnectWorkerState State
        {
            get
            {
                return _state;
            }
            set
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

        public bool Start(IClientSessionBase client, string host, int port, int reconnectInterval)
        {
            if (State != ClientReconnectWorkerState.Stopped) return false;
            lock (_lock)
            {
                if (State != ClientReconnectWorkerState.Stopped) return false;
                if (client == null) throw new ArgumentNullException("client");
                if (string.IsNullOrEmpty(host)) throw new ArgumentNullException("host");
                if (port == 0) throw new ArgumentNullException("port");
                State = ClientReconnectWorkerState.Starting;
                _workerThread = new Thread(DoReconnect);
                _workerThread.Start(new WorkerParameterHolder { Client = client, Host = host, Port = port, ReconnectInterval = reconnectInterval });
                return true;
            }
        }

        private void DoReconnect(object obj)
        {
            lock (_lock)
            {
                if (State == ClientReconnectWorkerState.Starting)
                {
                    State = ClientReconnectWorkerState.Started;
                }
            }
            var backOffMultiplexer = 1;
            var workerParameterHolder = (WorkerParameterHolder)obj;
            while (State == ClientReconnectWorkerState.Started)
            {
                lock (_workerThread)
                {
                    if (State == ClientReconnectWorkerState.Started)
                    {
                        Monitor.Wait(_workerThread, workerParameterHolder.ReconnectInterval * backOffMultiplexer);
                    }
                }
                try
                {
                    workerParameterHolder.Client.Connect(workerParameterHolder.Host, workerParameterHolder.Port);
                }
                catch
                {
                    // ignored
                }
                if (backOffMultiplexer * 2 <= MaxBackOffMultiplexer)
                {
                    // back-off
                    backOffMultiplexer *= 2;
                }
            }
        }

        public bool Stop()
        {
            if (State != ClientReconnectWorkerState.Started) return false;
            lock (_lock)
            {
                if (State != ClientReconnectWorkerState.Started) return false;
                State = ClientReconnectWorkerState.Stopping;
                if (_workerThread != null)
                {
                    lock (_workerThread)
                    {
                        Monitor.Pulse(_workerThread);
                    }
                    if (!Thread.CurrentThread.Equals(_workerThread))
                    {
                        _workerThread.Join();
                    }
                }
                State = ClientReconnectWorkerState.Stopped;
                return true;
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

        public enum ClientReconnectWorkerState
        {
            Started, Starting, Stopping, Stopped
        }

        private class WorkerParameterHolder
        {
            public IClientSessionBase Client { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public int ReconnectInterval { get; set; }
        }
    }
}