using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace SocketClientServerLib
{
    internal class ReceiveDataWorker : IDisposable
    {
        private readonly byte[] _buffer;
        private volatile ReceiveDataWorkerState _state = ReceiveDataWorkerState.Stopped;
        private readonly object _lock = new object();
        private Thread _workerThread;
        private readonly IIncomingDataProcessor _incomingDataProcessor;

        public event Action<ReceiveDataWorker, ReceiveDataWorkerState> StateChanged;
        public event Action<ReceiveDataWorker, Exception> Error;
        public event Action<ReceiveDataWorker, Packet> Received;

        private CancellationTokenSource _cts;

        public ReceiveDataWorkerState State
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

        public ReceiveDataWorker(int bufferSize, IIncomingDataProcessor incomingDataProcessor)
        {
            _incomingDataProcessor = incomingDataProcessor;
            _buffer = new byte[bufferSize];
        }

        public bool Start(Stream stream)
        {
            if (State != ReceiveDataWorkerState.Stopped) return false;
            lock (_lock)
            {
                if (State != ReceiveDataWorkerState.Stopped) return false;
                if (stream == null) throw new ArgumentNullException("stream");
                State = ReceiveDataWorkerState.Starting;
                _workerThread = new Thread(DoReceive);
                _workerThread.Start(stream);
                return true;
            }
        }

        private async void DoReceive(object obj)
        {
            lock (_lock)
            {
                if (State == ReceiveDataWorkerState.Starting)
                {
                    State = ReceiveDataWorkerState.Started;
                    _cts = new CancellationTokenSource();
                    _incomingDataProcessor.Reset();
                }
            }
            var stream = (Stream)obj;
            while (State == ReceiveDataWorkerState.Started)
            {
                try
                {
                    var lenRead = await stream.ReadAsync(_buffer, 0, _buffer.Length, _cts.Token);
                    if (lenRead == 0)
                    {
                        throw new EndOfStreamException("Remote end has closed the connection.");
                    }
                    var packets = _incomingDataProcessor.Process(_buffer, 0, lenRead);
                    if (Received != null && packets.Any())
                    {
                        foreach (var packet in packets)
                        {
                            Received(this, packet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (State == ReceiveDataWorkerState.Started)
                    {
                        if (Error != null)
                        {
                            Error(this, ex);
                        }
                        Stop();
                    }
                    // stopping or already stopped - not handle the error
                    return;
                }
            }
        }

        public bool Stop()
        {
            if (State != ReceiveDataWorkerState.Started) return false;
            lock (_lock)
            {
                if (State != ReceiveDataWorkerState.Started) return false;
                State = ReceiveDataWorkerState.Stopping;
                if (_cts != null)
                {
                    _cts.Cancel();
                }
                if (!Thread.CurrentThread.Equals(_workerThread))
                {
                    _workerThread.Join();
                }
                State = ReceiveDataWorkerState.Stopped;
                return true;
            }
        }

        public void Dispose()
        {
            try
            {
                Stop();
                _incomingDataProcessor.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        public enum ReceiveDataWorkerState
        {
            Started, Starting, Stopping, Stopped
        }
    }
}