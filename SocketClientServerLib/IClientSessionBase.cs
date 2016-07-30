namespace SocketClientServerLib
{
    public interface IClientSessionBase : ISessionBase
    {
        bool Connect(string host, int port);
    }
}