namespace SocketClientServerLib
{
    internal static class ConversionUtil
    {
        public static byte[] ToBytes(int val)
        {
            var bytes = new byte[4];
            ToBytes(bytes, 0, 4, val);
            return bytes;
        }

        public static void ToBytes(byte[] buffer, int idx, int length, int val)
        {
            for (var i = 0; i < length; ++i)
            {
                buffer[idx + i] = (byte)((val >> (i * 8)) & 0xFF);
            }
        }

        public static int ToInt32(byte[] bytes)
        {
            return ToInt32(bytes, 0, bytes.Length);
        }

        public static int ToInt32(byte[] buffer, int idx, int len)
        {
            var val = 0;
            for (var i = 0; i < len; ++i)
            {
                val |= buffer[idx + i] << (i * 8);
            }
            return val;
        }
    }
}