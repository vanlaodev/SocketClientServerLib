using System;

namespace SocketClientServerLib
{
    public interface IOutgoingDataProcessor : IDisposable
    {
        byte[] Process(Packet packet);
    }
}