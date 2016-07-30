using System.Text;

namespace SocketClientServerLib
{
    public class HeartbeatPacket : Packet
    {
        private static readonly byte[] Bytes = Encoding.UTF8.GetBytes("Heartbeat"); // TODO: debug purpose, delete it later

        public HeartbeatPacket()
        {
            Data = Bytes;
        }
    }
}