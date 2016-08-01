using System.Net.Sockets;

namespace SocketClientServerLib
{
    public interface ISecurityChecker
    {
        string Name { get; }

        bool Check(TcpClient tcpClient);
    }
}