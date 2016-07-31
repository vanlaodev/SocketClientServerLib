using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SocketClientServerLib
{
    public class DefaultIncomingDataProcessor : IIncomingDataProcessor
    {
        private const int FixHeaderLength = 7;
        private readonly byte[] _fixHeaderBuffer = new byte[FixHeaderLength];
        private MemoryStream _cachedData;

        private readonly byte[] _beginning;

        public DefaultIncomingDataProcessor(byte[] beginning)
        {
            _beginning = beginning;
        }

        public void Dispose()
        {
            Reset();
        }

        public void Reset()
        {
            try
            {
                if (_cachedData != null)
                {
                    _cachedData.Dispose();
                    _cachedData = null;
                }
            }
            catch
            {
                // ignored
            }
        }

        public IEnumerable<Packet> Process(byte[] receiveBuffer, int idx, int len)
        {
            if (_cachedData == null)
            {
                _cachedData = new MemoryStream();
            }
            else
            {
                _cachedData.Position = _cachedData.Length;
            }
            _cachedData.Write(receiveBuffer, idx, len);
            _cachedData.Position = 0;
            var packets = new List<Packet>();
            while (_cachedData.Length - _cachedData.Position >= FixHeaderLength)
            {
                _cachedData.Read(_fixHeaderBuffer, 0, FixHeaderLength);
                if (_fixHeaderBuffer[0] != _beginning[0] || _fixHeaderBuffer[1] != _beginning[1])
                {
                    throw new FormatException("Invalid packet format.");
                }
                var compressionType = (CompressionType)_fixHeaderBuffer[2];
                var dataLength = ConversionUtil.ToInt32(_fixHeaderBuffer, 3, 4);
                if (dataLength == 0)
                {
                    packets.Add(new HeartbeatPacket());
                }
                else if (_cachedData.Length - _cachedData.Position >= dataLength)
                {
                    var data = new byte[dataLength];
                    _cachedData.Read(data, 0, dataLength);
                    packets.Add(CreatePacket(CompressionUtil.Decompress(compressionType, data)));
                }
                else
                {
                    _cachedData.Position -= FixHeaderLength;
                    break;
                }
            }
            if (packets.Any())
            {
                ResizeCachedDataByRemainingData();
            }
            return packets;
        }

        private void ResizeCachedDataByRemainingData()
        {
            var remainingDataLength = _cachedData.Length - _cachedData.Position;
            MemoryStream newCachedData = null;
            if (remainingDataLength > 0)
            {
                var remainingDataBytes = new byte[remainingDataLength];
                _cachedData.Read(remainingDataBytes, 0, remainingDataBytes.Length);
                newCachedData = new MemoryStream();
                newCachedData.Write(remainingDataBytes, 0, remainingDataBytes.Length);
            }
            Reset();
            if (remainingDataLength > 0)
            {
                _cachedData = newCachedData;
            }
        }

        protected virtual Packet CreatePacket(byte[] data)
        {
            return new Packet()
            {
                Data = data
            };
        }
    }
}