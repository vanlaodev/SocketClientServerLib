using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SocketClientServerLib
{
    internal class SendDataWorker : IDisposable
    {
        private volatile SendDataWorkerState _state = SendDataWorkerState.Stopped;
        private readonly object _lock = new object();
        private Thread _workerThread;
        private readonly Queue<Packet> _sendQueue = new Queue<Packet>();
        private readonly IOutgoingDataProcessor _outgoingDataProcessor;

        public SendDataWorker(IOutgoingDataProcessor outgoingDataProcessor)
        {
            _outgoingDataProcessor = outgoingDataProcessor;
        }

        public event Action<SendDataWorker, SendDataWorkerState> StateChanged;
        public event Action<SendDataWorker, Exception> Error;
        public event Action<SendDataWorker, Packet> Sent;

        private CancellationTokenSource _cts;

        public SendDataWorkerState State
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

        public bool SendData(Packet data)
        {
            if (State != SendDataWorkerState.Started) return false;
            lock (_sendQueue)
            {
                if (State != SendDataWorkerState.Started) return false;
                _sendQueue.Enqueue(data);
                Monitor.Pulse(_sendQueue);
                return true;
            }
        }

        public bool Start(Stream stream)
        {
            if (State != SendDataWorkerState.Stopped) return false;
            lock (_lock)
            {
                if (State != SendDataWorkerState.Stopped) return false;
                if (stream == null) throw new ArgumentNullException("stream");
                State = SendDataWorkerState.Starting;
                _workerThread = new Thread(DoSend);
                _workerThread.Start(stream);
                return true;
            }
        }

        private async void DoSend(object obj)
        {
            lock (_lock)
            {
                if (State == SendDataWorkerState.Starting)
                {
                    State = SendDataWorkerState.Started;
                    _cts = new CancellationTokenSource();
                }
            }
            var stream = (Stream)obj;
            while (State == SendDataWorkerState.Started)
            {
                Packet dataToBeSent = null;
                lock (_sendQueue)
                {
                    if (State == SendDataWorkerState.Started)
                    {
                        if (!_sendQueue.Any())
                        {
                            Monitor.Wait(_sendQueue);
                        }
                        if (_sendQueue.Any())
                        {
                            dataToBeSent = _sendQueue.Dequeue();
                        }
                    }
                    else
                    {
                        // stopped
                        return;
                    }
                }
                if (dataToBeSent == null)
                {
                    // stopped
                    return;
                }
                try
                {
                    var dataBytes = _outgoingDataProcessor.Process(dataToBeSent);
                    await stream.WriteAsync(dataBytes, 0, dataBytes.Length, _cts.Token);
//                    await stream.FlushAsync();
                    if (Sent != null)
                    {
                        Sent(this, dataToBeSent);
                    }
                }
                catch (Exception ex)
                {
                    if (State == SendDataWorkerState.Started)
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
            if (State != SendDataWorkerState.Started) return false;
            lock (_lock)
            {
                if (State != SendDataWorkerState.Started) return false;
                State = SendDataWorkerState.Stopping;
                if (_cts != null)
                {
                    _cts.Cancel();
                }
                lock (_sendQueue)
                {
                    _sendQueue.Clear();
                    Monitor.Pulse(_sendQueue);
                }
                if (!Thread.CurrentThread.Equals(_workerThread))
                {
                    _workerThread.Join();
                }
                State = SendDataWorkerState.Stopped;
                return true;
            }
        }

        public void Dispose()
        {
            try
            {
                Stop();
                _outgoingDataProcessor.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        public enum SendDataWorkerState
        {
            Started, Starting, Stopping, Stopped
        }
    }
}