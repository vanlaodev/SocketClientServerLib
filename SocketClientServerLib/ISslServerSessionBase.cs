using System;
using System.Collections.Generic;
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

        IEnumerable<KeyValuePair<string, string>> ClientSslSubjectKVList { get; }

        string ClientCn { get; }
    }
}