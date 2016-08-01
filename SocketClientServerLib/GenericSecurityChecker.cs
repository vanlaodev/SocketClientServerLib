using System;
using System.Net.Sockets;

namespace SocketClientServerLib
{
    public class GenericSecurityChecker : ISecurityChecker
    {
        private readonly Func<TcpClient, bool> _func;

        public string Name { get; private set; }

        public GenericSecurityChecker(string name, Func<TcpClient, bool> func)
        {
            Name = name;
            _func = func;
        }

        public bool Check(TcpClient tcpClient)
        {
            return _func != null && _func(tcpClient);
        }
    }
}