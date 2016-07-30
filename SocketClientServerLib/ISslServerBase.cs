using System.Security.Cryptography.X509Certificates;

namespace SocketClientServerLib
{
    public interface ISslServerBase : IServerBase
    {
        bool UseSsl { get; set; }

        int AuthenticateTimeout { get; set; }

        X509Certificate2 ServerCertificate { get; set; }
    }
}