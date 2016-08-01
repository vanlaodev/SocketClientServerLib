using System;
using System.Net.Sockets;

namespace SocketClientServerLib
{
    public class GenericIncomingClientChecker : IIncomingClientChecker
    {
        private readonly Func<TcpClient, bool> _func;

        public string Name { get; private set; }

        public GenericIncomingClientChecker(string name, Func<TcpClient, bool> func)
        {
            Name = name;
            _func = func;
        }

        public bool Allow(TcpClient tcpClient)
        {
            return _func != null && _func(tcpClient);
        }
    }
}