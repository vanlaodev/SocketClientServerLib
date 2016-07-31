using System.Collections.Generic;
using System.IO;
using MsgPack.Serialization;
using SocketClientServerLib;

namespace DemoCommon
{
    public class DemoIncomingDataProcessor : VHIncomingDataProcessorBase
    {
        public DemoIncomingDataProcessor(byte[] beginning) : base(beginning)
        {
        }

        protected override Dictionary<string, string> DeserializeHeaders(byte[] buffer, int idx, int len)
        {
            var serializer = SerializationContext.Default.GetSerializer<Dictionary<string, string>>();
            using (var ms = new MemoryStream(buffer, idx, len))
            {
                return serializer.Unpack(ms);
            }
        }
    }
}