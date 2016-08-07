using System;

namespace SocketClientServerLib
{
    public class Packet : ICloneable, IDisposable
    {
        public virtual byte[] Data { get; set; }

        public virtual object Clone()
        {
            return new Packet()
            {
                Data = CloneInternalData()
            };
        }

        protected byte[] CloneInternalData()
        {
            byte[] data = null;
            if (Data != null)
            {
                data = new byte[Data.Length];
                Array.Copy(Data, data, Data.Length);
            }
            return data;
        }

        public void Dispose()
        {
            Data = null;
        }
    }
}