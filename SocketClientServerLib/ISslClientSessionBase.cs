using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace SocketClientServerLib
{
    public interface ISslClientSessionBase : IClientSessionBase
    {
        bool UseSsl { get; set; }

        string ServerCn { get; set; }

        int AuthenticateTimeout { get; set; }

        SslProtocols SslProtocols { get; set; }

        X509Certificate2 ClientCertificate { get; set; }
    }
}