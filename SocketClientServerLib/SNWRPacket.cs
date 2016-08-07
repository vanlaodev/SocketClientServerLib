using System;
using System.Collections.Generic;

namespace SocketClientServerLib
{
    // Send-aNd-Wait-Reply packet
    public class SNWRPacket : VHPacket
    {
        public string Id
        {
            get { return Headers.ContainsKey("Id") ? Headers["Id"] : null; }
            private set { Headers["Id"] = value; }
        }

        public string ReplyId
        {
            get { return Headers.ContainsKey("ReplyId") ? Headers["ReplyId"] : null; }
            set { Headers["ReplyId"] = value; }
        }

        public SNWRPacket()
        {
            Id = Guid.NewGuid().ToString();
        }

        public SNWRPacket(Dictionary<string, string> headers) : base(headers)
        {
        }

        public override object Clone()
        {
            return new SNWRPacket(CloneHeaders())
            {
                Data = CloneInternalData()
            };
        }
    }
}