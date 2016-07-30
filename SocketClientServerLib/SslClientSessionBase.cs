using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SocketClientServerLib
{
    public abstract class SslClientSessionBase : ClientSessionBase, ISslClientSessionBase
    {
        private bool _useSsl;
        private string _serverCn;
        private int _authenticateTimeout = 30000; // default 30s
        private X509Certificate2 _clientCertificate;

        public bool UseSsl
        {
            get { return _useSsl; }
            set
            {
                if (State != SessionState.Disconnected && State != SessionState.Disconnecting) throw new InvalidOperationException("Invalid session state.");
                _useSsl = value;
            }
        }

        public string ServerCn
        {
            get { return _serverCn; }
            set
            {
                if (State != SessionState.Disconnected && State != SessionState.Disconnecting) throw new InvalidOperationException("Invalid session state.");
                _serverCn = value;
            }
        }

        public int AuthenticateTimeout
        {
            get { return _authenticateTimeout; }
            set
            {
                if (State != SessionState.Disconnected && State != SessionState.Disconnecting) throw new InvalidOperationException("Invalid session state.");
                _authenticateTimeout = value;
            }
        }

        public X509Certificate2 ClientCertificate
        {
            get { return _clientCertificate; }
            set
            {
                if (State != SessionState.Disconnected && State != SessionState.Disconnecting) throw new InvalidOperationException();
                _clientCertificate = value;
            }
        }

        protected override Stream GetStream(TcpClient tcpClient)
        {
            var stream = tcpClient.GetStream();
            if (!UseSsl)
            {
                return stream;
            }
            var sslStream = new SslStream(stream, false, UserCertificateValidationCallback, null);
            return AuthenticateAsync(sslStream).Result;
        }

        private async Task<SslStream> AuthenticateAsync(SslStream sslStream)
        {
            var authenticateTask = sslStream.AuthenticateAsClientAsync(ServerCn,
                new X509Certificate2Collection(ClientCertificate), SslProtocols.Tls12, false);
            if (await Task.WhenAny(authenticateTask, Task.Delay(AuthenticateTimeout)) != authenticateTask)
            {
                throw new TimeoutException("Ssl authentication timed-out.");
            }
            await authenticateTask;
            return sslStream;
        }

        private bool UserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            return false;
        }
    }
}