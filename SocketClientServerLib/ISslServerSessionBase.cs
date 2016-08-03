using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace SocketClientServerLib
{
    public interface ISslServerSessionBase : IServerSessionBase
    {
        event Action<ISslServerSessionBase> SslAuthenticated;

        bool UseSsl { get; set; }

        int AuthenticateTimeout { get; set; }

        X509Certificate2 ServerCertificate { get; set; }

        SslProtocols SslProtocols { get; set; }

        string ClientCn { get; }
    }
}