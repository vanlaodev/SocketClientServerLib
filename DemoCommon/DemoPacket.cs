using System;
using System.Collections.Generic;
using System.Text;
using SocketClientServerLib;

namespace DemoCommon
{
    public class DemoPacket : SNWRPacket
    {
        public string Text
        {
            get { return Encoding.UTF8.GetString(Data); }
            set { Data = Encoding.UTF8.GetBytes(value); }
        }

        public DemoPacket()
        {
        }

        public DemoPacket(Dictionary<string, string> headers) : base(headers)
        {
        }

        public override object Clone()
        {
            return new DemoPacket(CloneHeaders())
            {
                Data = CloneInternalData()
            };
        }
    }
}