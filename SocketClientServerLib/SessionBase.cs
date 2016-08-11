using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketClientServerLib
{
    public abstract class SessionBase : ISessionBase
    {
        private readonly object _lock = new object();
        private Stream _stream;
        private TcpClient _tcpClient;
        private SessionState _state = SessionState.Disconnected;
        private readonly object _stateLock = new object();
        private readonly object _ensureSendDataReadyLock = new object();
        private readonly SendDataWorker _sendDataWorker;
        private readonly ReceiveDataWorker _receiveDataWorker;
        private readonly HeartbeatWorker _heartbeatWorker;
        private bool _sendHeartbeat = true;
        private int _heartbeatInterval = 60000; // default 60s
        private int _readTimeout = Timeout.Infinite;
        private int _writeTimeout = Timeout.Infinite;

        public event Action<ISessionBase, SessionState> StateChanged;

        public event Action<ISessionBase, Exception> InternalError;
        public event Action<ISessionBase, Packet> DataSent;
        public event Action<ISessionBase, Packet> DataReceived;
        public event Action<ISessionBase, bool> SendDataReady;

        public int ReadTimeout
        {
            get { return _readTimeout; }
            set
            {
                if (State != SessionState.Disconnected && State != SessionState.Disconnecting) throw new InvalidOperationException("Invalid session state.");
                _readTimeout = value;
            }
        }

        public int WriteTimeout
        {
            get { return _writeTimeout; }
            set
            {
                if (State != SessionState.Disconnected && State != SessionState.Disconnecting) throw new InvalidOperationException("Invalid session state.");
                _writeTimeout = value;
            }
        }

        public bool SendHeartbeat
        {
            get { return _sendHeartbeat; }
            set
            {
                if (State != SessionState.Disconnected && State != SessionState.Disconnecting) throw new InvalidOperationException("Invalid session state.");
                _sendHeartbeat = value;
            }
        }

        public int HeartbeatInterval
        {
            get { return _heartbeatInterval; }
            set
            {
                if (State != SessionState.Disconnected && State != SessionState.Disconnecting) throw new InvalidOperationException("Invalid session state.");
                _heartbeatInterval = value;
            }
        }

        public IPEndPoint EndPoint { get; private set; }

        public SessionState State
        {
            get
            {
                lock (_stateLock)
                {
                    return _state;
                }
            }
            protected set
            {
                var stateChanged = false;
                lock (_stateLock)
                {
                    if (_state != value)
                    {
                        _state = value;
                        stateChanged = true;
                    }
                }
                if (stateChanged && StateChanged != null)
                {
                    StateChanged(this, value);
                }
            }
        }

        protected SessionBase(IIncomingDataProcessor incomingDataProcessor, IOutgoingDataProcessor outgoingDataProcessor, int receiveBufferSize)
        {
            _sendDataWorker = new SendDataWorker(outgoingDataProcessor);
            _sendDataWorker.Sent += SendDataWorkerOnSent;
            _sendDataWorker.Error += SendDataWorkerOnError;
            _sendDataWorker.StateChanged += SendDataWorkerOnStateChanged;

            _heartbeatWorker = new HeartbeatWorker(_sendDataWorker);
            _heartbeatWorker.StateChanged += HeartbeatWorkerOnStateChanged;

            _receiveDataWorker = new ReceiveDataWorker(receiveBufferSize, incomingDataProcessor);
            _receiveDataWorker.Received += ReceiveDataWorkerOnReceived;
            _receiveDataWorker.Error += ReceiveDataWorkerOnError;
            _receiveDataWorker.StateChanged += ReceiveDataWorkerOnStateChanged;
        }

        private void HeartbeatWorkerOnStateChanged(HeartbeatWorker heartbeatWorker, HeartbeatWorker.HeartbeatWorkerState heartbeatWorkerState)
        {

        }

        private void ReceiveDataWorkerOnStateChanged(ReceiveDataWorker receiveDataWorker, ReceiveDataWorker.ReceiveDataWorkerState receiveDataWorkerState)
        {

        }

        private void ReceiveDataWorkerOnError(ReceiveDataWorker receiveDataWorker, Exception exception)
        {
            ReportInternalError(exception);
            Disconnect();
        }

        private void ReceiveDataWorkerOnReceived(ReceiveDataWorker receiveDataWorker, Packet packet)
        {
            if (DataReceived != null)
            {
                DataReceived(this, packet);
            }
        }

        private void SendDataWorkerOnStateChanged(SendDataWorker sendDataWorker, SendDataWorker.SendDataWorkerState sendDataWorkerState)
        {
            var ready = sendDataWorkerState == SendDataWorker.SendDataWorkerState.Started;
            if (ready)
            {
                lock (_ensureSendDataReadyLock)
                {
                    Monitor.Pulse(_ensureSendDataReadyLock);
                }
            }
            if (ready && SendHeartbeat)
            {
                _heartbeatWorker.Start(HeartbeatInterval);
            }
            else
            {
                _heartbeatWorker.Stop();
            }
            if (SendDataReady != null)
            {
                SendDataReady(this, ready);
            }
        }

        private void SendDataWorkerOnError(SendDataWorker sendDataWorker, Exception exception)
        {
            ReportInternalError(exception);
            Disconnect();
        }

        private void SendDataWorkerOnSent(SendDataWorker sendDataWorker, Packet packet)
        {
            if (DataSent != null)
            {
                DataSent(this, packet);
            }
        }

        public void EnsureSendDataReady(int timeout)
        {
            if (State == SessionState.Connected) return;
            lock (_ensureSendDataReadyLock)
            {
                if (State == SessionState.Connected) return;
                if (!Monitor.Wait(_ensureSendDataReadyLock, timeout))
                {
                    throw new TimeoutException("Waiting for send data ready timed-out.");
                }
            }
        }

        public bool AttachTcpClient(TcpClient tcpClient)
        {
            return AttachTcpClient(() => tcpClient);
        }

        protected virtual bool AttachTcpClient(Func<TcpClient> getTcpClient)
        {
            if (State != SessionState.Disconnected) return false;
            lock (_lock)
            {
                if (State != SessionState.Disconnected) return false;
                try
                {
                    State = SessionState.Connecting;
                    _tcpClient = getTcpClient();
                    EndPoint = (IPEndPoint)_tcpClient.Client.RemoteEndPoint;
                    _stream = GetStream(_tcpClient);
                    _stream.WriteTimeout = WriteTimeout;
                    _stream.ReadTimeout = ReadTimeout;
                    State = SessionState.Connected;
                    _sendDataWorker.Start(_stream);
                    _receiveDataWorker.Start(_stream);
                    return true;
                }
                catch
                {
                    State = SessionState.Disconnected;
                    throw;
                }
            }
        }

        public bool Disconnect()
        {
            if (State != SessionState.Connected) return false;
            lock (_lock)
            {
                if (State != SessionState.Connected) return false;
                State = SessionState.Disconnecting;
                try
                {
                    _stream.Close();
                    _tcpClient.Close();
                    _sendDataWorker.Stop();
                    _receiveDataWorker.Stop();
                    return true;
                }
                finally
                {
                    State = SessionState.Disconnected;
                }
            }
        }

        public bool SendData(Packet data)
        {
            if (State != SessionState.Connected) return false;
            lock (_lock)
            {
                if (State != SessionState.Connected) return false;
                return _sendDataWorker.SendData(data);
            }
        }

        protected virtual Stream GetStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected void ReportInternalError(Exception ex)
        {
            if (ex != null && InternalError != null)
            {
                InternalError(this, ex);
            }
        }

        public virtual void Dispose()
        {
            try
            {
                Disconnect();
                _heartbeatWorker.Dispose();
                _sendDataWorker.Dispose();
                _receiveDataWorker.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }
}