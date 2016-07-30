namespace SocketClientServerLib
{
    public interface IClientSessionBase : ISessionBase
    {
        int LastPort { get; }

        string LastHost { get; }

        bool AutoReconnect { get; set; }

        int ReconnectInterval { get; set; }

        bool Connect(string host, int port);
    }
}