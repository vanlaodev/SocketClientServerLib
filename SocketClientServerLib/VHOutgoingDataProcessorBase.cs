﻿using System;
using System.Collections.Generic;

namespace SocketClientServerLib
{
    public abstract class VHOutgoingDataProcessorBase : DefaultOutgoingDataProcessor
    {
        protected VHOutgoingDataProcessorBase(byte[] beginning, CompressionType compressionType) : base(beginning, compressionType)
        {
        }

        protected override byte[] GetDataBytes(Packet packet)
        {
            var vhPacket = packet as VHPacket;
            if (vhPacket == null) return base.GetDataBytes(packet);
            var headersBytes = SerializeHeaders(vhPacket.Headers);
            var dataBytes = new byte[4 + headersBytes.Length + packet.Data.Length];
            ConversionUtil.ToBytes(dataBytes, 0, 4, headersBytes.Length);
            Array.Copy(headersBytes, 0, dataBytes, 4, headersBytes.Length);
            Array.Copy(packet.Data, 0, dataBytes, 4 + headersBytes.Length, packet.Data.Length);
            return dataBytes;
        }

        protected abstract byte[] SerializeHeaders(Dictionary<string, string> headers);
    }
}