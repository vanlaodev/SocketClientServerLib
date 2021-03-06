﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SocketClientServerLib
{
    public abstract class SslServerSessionBase : ServerSessionBase, ISslServerSessionBase
    {
        private bool _useSsl;
        private int _authenticateTimeout;
        private X509Certificate2 _serverCertificate;
        private SslProtocols _sslProtocols = SslProtocols.Tls;
        private readonly List<KeyValuePair<string, string>> _clientSslSubjectKVList = new List<KeyValuePair<string, string>>();

        public event Action<ISslServerSessionBase> SslAuthenticated;

        public bool UseSsl
        {
            get { return _useSsl; }
            set
            {
                if (State != SessionState.Disconnected && State != SessionState.Disconnecting) throw new InvalidOperationException("Invalid session state.");
                _useSsl = value;
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

        public X509Certificate2 ServerCertificate
        {
            get { return _serverCertificate; }
            set
            {
                if (State != SessionState.Disconnected && State != SessionState.Disconnecting) throw new InvalidOperationException("Invalid session state.");
                _serverCertificate = value;
            }
        }

        public SslProtocols SslProtocols
        {
            get { return _sslProtocols; }
            set
            {
                if (State != SessionState.Disconnected && State != SessionState.Disconnecting) throw new InvalidOperationException("Invalid session state.");
                _sslProtocols = value;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> ClientSslSubjectKVList
        {
            get
            {
                return _clientSslSubjectKVList.ToList();
            }
        }

        public string ClientCn
        {
            get
            {
                var cnKV = _clientSslSubjectKVList.FirstOrDefault(x => x.Key.Equals("CN", StringComparison.InvariantCultureIgnoreCase));
                return cnKV.Value;
            }
        }

        protected SslServerSessionBase(IIncomingDataProcessor incomingDataProcessor, IOutgoingDataProcessor outgoingDataProcessor, int receiveBufferSize)
            : base(incomingDataProcessor, outgoingDataProcessor, receiveBufferSize)
        {
        }

        protected override Stream GetStream(TcpClient tcpClient)
        {
            var stream = tcpClient.GetStream();
            if (!UseSsl)
            {
                return stream;
            }
            var sslStream = new SslStream(stream, false, UserCertificateValidationCallback, null);
            sslStream = AuthenticateAsync(sslStream).Result;
            if (SslAuthenticated != null)
            {
                SslAuthenticated(this);
            }
            return sslStream;
        }

        private async Task<SslStream> AuthenticateAsync(SslStream sslStream)
        {
            var authenticateTask = sslStream.AuthenticateAsServerAsync(ServerCertificate, true, SslProtocols, false);
            if (await Task.WhenAny(authenticateTask, Task.Delay(AuthenticateTimeout)).ConfigureAwait(false) != authenticateTask)
            {
                throw new TimeoutException("Ssl authentication timed-out.");
            }
            await authenticateTask;
            _clientSslSubjectKVList.Clear();
            var arr = sslStream.RemoteCertificate.Subject.Split(",".ToCharArray());
            foreach (var i in arr)
            {
                var keyVal = i.Split("=".ToCharArray());
                _clientSslSubjectKVList.Add(new KeyValuePair<string, string>(keyVal[0].Trim(), keyVal[1].Trim()));
            }
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