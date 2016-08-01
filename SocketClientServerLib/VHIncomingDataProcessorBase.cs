using System;
using System.Collections.Generic;

namespace SocketClientServerLib
{
    public abstract class VHIncomingDataProcessorBase : DefaultIncomingDataProcessor
    {
        protected VHIncomingDataProcessorBase(byte[] beginning) : base(beginning)
        {
        }

        protected override Packet CreatePacket(byte[] data)
        {
            var headersLength = ConversionUtil.ToInt32(data, 0, 4);
            var headers = DeserializeHeaders(data, 4, headersLength);
            var realData = new byte[data.Length - headersLength - 4];
            Array.Copy(data, 4 + headersLength, realData, 0, realData.Length);
            return CreatePacket(headers, realData);
        }

        protected virtual VHPacket CreatePacket(Dictionary<string, string> headers, byte[] realData)
        {
            return new VHPacket(headers)
            {
                Data = realData
            };
        }

        protected abstract Dictionary<string, string> DeserializeHeaders(byte[] buffer, int idx, int len);
    }
}