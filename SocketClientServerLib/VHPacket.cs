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

        public override object Clone()
        {
            return new VHPacket(CloneHeaders())
            {
                Data = CloneInternalData()
            };
        }

        protected virtual Dictionary<string, string> CloneHeaders()
        {
            if (Headers == null) return null;
            return new Dictionary<string, string>(Headers);
        }
    }
}