using System;
using System.Collections.Generic;

namespace SocketClientServerLib
{
    public interface IIncomingDataProcessor : IDisposable
    {
        void Reset();

        IEnumerable<Packet> Process(byte[] receiveBuffer, int idx, int len);
    }
}