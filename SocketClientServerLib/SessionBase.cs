using System;
using System.IO;
using System.Net.Sockets;

namespace SocketClientServerLib
{
    public abstract class SessionBase : ISessionBase
    {
        private readonly object _lock = new object();
        private Stream _stream;
        private TcpClient _tcpClient;
        private SessionState _state = SessionState.Disconnected;
        private readonly object _stateLock = new object();
        private readonly SendDataWorker _sendDataWorker;
        private readonly ReceiveDataWorker _receiveDataWorker;

        public event Action<ISessionBase, SessionState> StateChanged;

        public event Action<ISessionBase, Exception> InternalError;
        public event Action<ISessionBase, Packet> DataSent;
        public event Action<ISessionBase, Packet> DataReceived;
        public event Action<ISessionBase, bool> SendDataReady;

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

        protected SessionBase(int receiveBufferSize)
        {
            _sendDataWorker = new SendDataWorker();
            _sendDataWorker.Sent += SendDataWorkerOnSent;
            _sendDataWorker.Error += SendDataWorkerOnError;
            _sendDataWorker.StateChanged += SendDataWorkerOnStateChanged;

            _receiveDataWorker = new ReceiveDataWorker(receiveBufferSize);
            _receiveDataWorker.Received += ReceiveDataWorkerOnReceived;
            _receiveDataWorker.Error += ReceiveDataWorkerOnError;
            _receiveDataWorker.StateChanged += ReceiveDataWorkerOnStateChanged;
        }

        private void ReceiveDataWorkerOnStateChanged(ReceiveDataWorker receiveDataWorker, ReceiveDataWorker.ReceiveDataWorkerState receiveDataWorkerState)
        {

        }

        private void ReceiveDataWorkerOnError(ReceiveDataWorker receiveDataWorker, Exception exception)
        {
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
            if (SendDataReady != null)
            {
                SendDataReady(this, sendDataWorkerState == SendDataWorker.SendDataWorkerState.Started);
            }
        }

        private void SendDataWorkerOnError(SendDataWorker sendDataWorker, Exception exception)
        {
            Disconnect();
        }

        private void SendDataWorkerOnSent(SendDataWorker sendDataWorker, Packet packet)
        {
            if (DataSent != null)
            {
                DataSent(this, packet);
            }
        }

        public virtual bool AttachTcpClient(TcpClient tcpClient)
        {
            return AttachTcpClient(() => tcpClient);
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
                    _sendDataWorker.Stop();
                    _receiveDataWorker.Stop();
                    _stream.Close();
                    _tcpClient.Close();
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

        protected bool AttachTcpClient(Func<TcpClient> getTcpClient)
        {
            if (State != SessionState.Disconnected) return false;
            lock (_lock)
            {
                if (State != SessionState.Disconnected) return false;
                try
                {
                    _tcpClient = getTcpClient();
                    _stream = GetStream(_tcpClient);
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

        public void Dispose()
        {
            try
            {
                Disconnect();
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