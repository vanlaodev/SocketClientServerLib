using System.Net.Sockets;

namespace SocketClientServerLib
{
    public interface IIncomingClientChecker
    {
        string Name { get; }

        bool Allow(TcpClient tcpClient);
    }
}