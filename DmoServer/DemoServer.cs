﻿using System.Net.Sockets;
using SocketClientServerLib;

namespace DmoServer
{
    public class DemoServer : SslServerBase, IDemoServer
    {
        protected override ISslServerSessionBase CreateSslServerClient(TcpClient tcpClient)
        {
            return new DemoServerClient();
        }
    }
}