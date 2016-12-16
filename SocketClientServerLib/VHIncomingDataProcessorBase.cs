using System;
using System.Collections.Generic;

namespace SocketClientServerLib
{
    public abstract class VHIncomingDataProcessorBase : DefaultIncomingDataProcessor
    {
        protected VHIncomingDataProcessorBase(byte[] beginning)
            : base(beginning)
        {
        }

        protected sealed override Packet CreatePacket(byte[] data)
        {
            var headersLength = ConversionUtil.ToInt32(data, 0, 4);
            var headers = DeserializeHeaders(data, 4, headersLength);
            var realDataLen = data.Length - headersLength - 4;
            if (realDataLen > 0)
            {
                var realData = new byte[realDataLen];
                Array.Copy(data, 4 + headersLength, realData, 0, realDataLen);
                return CreatePacket(headers, realData);
            }
            else
            {
                return CreatePacket(headers, null);
            }
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