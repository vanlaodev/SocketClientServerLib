using System;
using System.Text;

namespace SocketClientServerLib
{
    public class HeartbeatPacket : Packet
    {
        public HeartbeatPacket()
        {
        }

        public override byte[] Data
        {
            get { return null; }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override object Clone()
        {
            throw new NotSupportedException();
        }
    }
}