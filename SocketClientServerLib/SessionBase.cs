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

        public event Action<ISessionBase, SessionState> StateChanged;

        public event Action<ISessionBase, Exception> InternalError;

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

        public virtual bool AttachTcpClient(TcpClient tcpClient)
        {
            return AttachTcpClient(() => tcpClient);
        }

        public bool Disconnect()
        {
            lock (_lock)
            {
                if (State != SessionState.Connected) return false;
                State = SessionState.Disconnecting;
                try
                {
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

        protected bool AttachTcpClient(Func<TcpClient> getTcpClient)
        {
            lock (_lock)
            {
                if (State != SessionState.Disconnected) return false;
                try
                {
                    _tcpClient = getTcpClient();
                    _stream = GetStream(_tcpClient);
                    State = SessionState.Connected;
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
            }
            catch
            {
                // ignored
            }
        }
    }
}