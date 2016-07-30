using System;
using System.Threading;

namespace SocketClientServerLib
{
    internal class HeartbeatWorker : IDisposable
    {
        private readonly SendDataWorker _sendDataWorker;
        private volatile HeartbeatWorkerState _state = HeartbeatWorkerState.Stopped;
        private readonly object _lock = new object();
        private Thread _workerThread;
        public event Action<HeartbeatWorker, HeartbeatWorkerState> StateChanged;

        public HeartbeatWorkerState State
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

        public HeartbeatWorker(SendDataWorker sendDataWorker)
        {
            _sendDataWorker = sendDataWorker;
        }

        public bool Start(int heartbeatInterval)
        {
            if (State != HeartbeatWorkerState.Stopped) return false;
            lock (_lock)
            {
                if (State != HeartbeatWorkerState.Stopped) return false;
                State = HeartbeatWorkerState.Starting;
                _workerThread = new Thread(DoHeartbeat);
                _workerThread.Start(heartbeatInterval);
                return true;
            }
        }

        private void DoHeartbeat(object obj)
        {
            lock (_lock)
            {
                if (State == HeartbeatWorkerState.Starting)
                {
                    State = HeartbeatWorkerState.Started;
                }
            }
            var heartbeatInterval = (int)obj;
            while (State == HeartbeatWorkerState.Started)
            {
                _sendDataWorker.SendData(new HeartbeatPacket());
                lock (_workerThread)
                {
                    if (State == HeartbeatWorkerState.Started)
                    {
                        Monitor.Wait(_workerThread, heartbeatInterval);
                    }
                }
            }
        }

        public bool Stop()
        {
            if (State != HeartbeatWorkerState.Started) return false;
            lock (_lock)
            {
                if (State != HeartbeatWorkerState.Started) return false;
                State = HeartbeatWorkerState.Stopping;
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
                State = HeartbeatWorkerState.Stopped;
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

        public enum HeartbeatWorkerState
        {
            Started, Starting, Stopping, Stopped
        }
    }
}