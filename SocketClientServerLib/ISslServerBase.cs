using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace SocketClientServerLib
{
    public interface ISslServerBase : IServerBase
    {
        event Action<ISslServerBase, ISslServerSessionBase> ClientSslAuthenticated;

        bool UseSsl { get; set; }

        int AuthenticateTimeout { get; set; }

        X509Certificate2 ServerCertificate { get; set; }

        SslProtocols SslProtocols { get; set; }
    }
}