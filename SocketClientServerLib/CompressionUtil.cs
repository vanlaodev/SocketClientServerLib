using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace SocketClientServerLib
{
    internal static class CompressionUtil
    {
        public static byte[] Compress(CompressionType type, byte[] plainBytes)
        {
            if (plainBytes == null || plainBytes.Length == 0) return plainBytes;
            switch (type)
            {
                case CompressionType.None:
                    return plainBytes;
                case CompressionType.GZip:
                    return CompressByGZip(plainBytes);
                default:
                    throw new NotSupportedException("Not supported compress type.");
            }
        }

        private static byte[] CompressByGZip(byte[] plainBytes)
        {
            using (var destMs = new MemoryStream())
            {
                using (var gs = new GZipStream(destMs, CompressionLevel.Optimal))
                {
                    using (var srcMs = new MemoryStream(plainBytes))
                    {
                        srcMs.CopyTo(gs);
                    }
                }
                return destMs.ToArray();
            }
        }

        public static byte[] Decompress(CompressionType type, byte[] compressedBytes)
        {
            if (compressedBytes == null || compressedBytes.Length == 0) return compressedBytes;
            switch (type)
            {
                case CompressionType.None:
                    return compressedBytes;
                case CompressionType.GZip:
                    return DecompressByGZip(compressedBytes);
                default:
                    throw new NotSupportedException("Not supported compress type.");
            }
        }

        private static byte[] DecompressByGZip(byte[] compressedBytes)
        {
            using (var sourceMs = new MemoryStream(compressedBytes))
            {
                using (var gs = new GZipStream(sourceMs, CompressionMode.Decompress))
                {
                    using (var destMs = new MemoryStream())
                    {
                        gs.CopyTo(destMs);
                        return destMs.ToArray();
                    }
                }
            }
        }
    }
}