using System.Collections.Generic;

namespace SocketClientServerLib
{
    // Packet with variable header
    public class VHPacket : Packet
    {
        public Dictionary<string, string> Headers { get; private set; }

        public VHPacket() : this(new Dictionary<string, string>())
        {
        }

        public VHPacket(Dictionary<string, string> headers)
        {
            Headers = headers;
        }
    }
}