using System.Collections.Generic;
using System.IO;
using MsgPack.Serialization;
using SocketClientServerLib;

namespace DemoCommon
{
    public class DemoOutgoingDataProcessor : VHOutgoingDataProcessorBase
    {
        public DemoOutgoingDataProcessor(byte[] beginning, CompressionType compressionType) : base(beginning, compressionType)
        {
        }

        protected override byte[] SerializeHeaders(Dictionary<string, string> headers)
        {
            var serializer = SerializationContext.Default.GetSerializer<Dictionary<string, string>>();
            using (var ms = new MemoryStream())
            {
                serializer.Pack(ms, headers);
                return ms.ToArray();
            }
        }
    }
}