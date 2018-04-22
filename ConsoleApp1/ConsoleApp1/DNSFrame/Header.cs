using System;
using System.Linq;

namespace ConsoleApp1
{
    public class Header
    {
        public ushort Id;
        public byte QR;
        public byte OpCode;
        public bool IsAutAns;
        public bool TrunCation;
        public bool IsRecursion;
        public bool RecursionAvailable;
        public byte RCODE;
        public ushort QDCOUNT;
        public ushort ANCOUNT;
        public ushort NSCOUNT;
        public ushort ARCOUNT;
        
        public Header(byte[] headerToByte)
        {
            var id = headerToByte.Take(2).ToArray();
            Id = BitConverter.ToUInt16(id, 0);
            GetFieldFrom3Bytes(headerToByte[2]);
            GetFieldFrom4Bytes(headerToByte[3]);
           
            QDCOUNT = BitConverter.ToUInt16(ReverseByte(headerToByte[4], headerToByte[5]), 0);
            ANCOUNT = BitConverter.ToUInt16(ReverseByte(headerToByte[6], headerToByte[7]), 0);
            NSCOUNT = BitConverter.ToUInt16(ReverseByte(headerToByte[8], headerToByte[9]), 0);
            ARCOUNT = BitConverter.ToUInt16(ReverseByte(headerToByte[10], headerToByte[11]), 0);
            //Console.WriteLine(QDCOUNT);
        }

        private void GetFieldFrom3Bytes(byte b)
        {
            QR = (byte)(b & (1 << 7));
            OpCode = (byte) (b & (15 << 3));
            IsAutAns = (b & (1 << 2)) == 1;
            TrunCation = (b & (1 << 1)) == 1;
            IsRecursion = (b & 1) == 1;
            
        }



        private byte[] ReverseByte(byte first, byte second)
        {
            return  new byte[]{second, first};
        }
        
        private void GetFieldFrom4Bytes(byte b)
        {
            RecursionAvailable = (b & (1 << 7)) == 1;
            RCODE = (byte) (b & 15);
        }
    }
}