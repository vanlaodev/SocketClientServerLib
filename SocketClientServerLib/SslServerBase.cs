using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace SocketClientServerLib
{
    public abstract class SslServerBase : ServerBase, ISslServerBase
    {
        private bool _useSsl;
        private int _authenticateTimeout = 30000; // default 30s
        private X509Certificate2 _serverCertificate;

        public bool UseSsl
        {
            get { return _useSsl; }
            set
            {
                if (State != ServerState.Stopping && State != ServerState.Stopped) throw new InvalidOperationException("Invalid server state.");
                _useSsl = value;
            }
        }

        public int AuthenticateTimeout
        {
            get { return _authenticateTimeout; }
            set
            {
                if (State != ServerState.Stopping && State != ServerState.Stopped) throw new InvalidOperationException("Invalid server state.");
                _authenticateTimeout = value;
            }
        }

        public X509Certificate2 ServerCertificate
        {
            get { return _serverCertificate; }
            set
            {
                if (State != ServerState.Stopping && State != ServerState.Stopped) throw new InvalidOperationException("Invalid server state.");
                _serverCertificate = value;
            }
        }

        protected override IServerSessionBase CreateServerClient(TcpClient tcpClient)
        {
            var serverClient = CreateSslServerClient(tcpClient);
            serverClient.AuthenticateTimeout = AuthenticateTimeout;
            serverClient.ServerCertificate = ServerCertificate;
            serverClient.UseSsl = UseSsl;
            return serverClient;
        }

        protected abstract ISslServerSessionBase CreateSslServerClient(TcpClient tcpClient);
    }
}