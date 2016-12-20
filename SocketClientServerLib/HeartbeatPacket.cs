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
            }
        }

        public override object Clone()
        {
            return new HeartbeatPacket();
        }
    }
}