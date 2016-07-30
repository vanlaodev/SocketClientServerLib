using System;

namespace SocketClientServerLib
{
    public class DefaultOutgoingDataProcessor : IOutgoingDataProcessor
    {
        private const int FixHeaderLength = 7;

        private readonly byte[] _fixedHeaderBuffer = new byte[FixHeaderLength];
        private readonly CompressionType _compressionType;

        public DefaultOutgoingDataProcessor(byte[] beginning, CompressionType compressionType)
        {
            _compressionType = compressionType;

            _fixedHeaderBuffer[0] = beginning[0];
            _fixedHeaderBuffer[1] = beginning[1];
            _fixedHeaderBuffer[2] = (byte)_compressionType;
        }

        public void Dispose()
        {

        }

        public byte[] Process(Packet packet)
        {
            var data = CompressionUtil.Compress(_compressionType, GetDataBytes(packet));
            var dataLength = data == null ? 0 : data.Length;
            ConversionUtil.ToBytes(_fixedHeaderBuffer, 3, 4, dataLength);
            var result = new byte[dataLength + FixHeaderLength];
            Array.Copy(_fixedHeaderBuffer, result, FixHeaderLength);
            if (data != null && dataLength > 0)
            {
                Array.Copy(data, 0, result, FixHeaderLength, dataLength);
            }
            return result;
        }

        protected virtual byte[] GetDataBytes(Packet packet)
        {
            return packet.Data;
        }
    }
}