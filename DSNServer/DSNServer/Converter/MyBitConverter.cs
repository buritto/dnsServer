using System;

namespace DSNServer.Converter
{
    public static class MyBitConverter
    {
        public static ushort ToUInt16(byte[] data, ref int pointer)
        {
            var number = BitConverter.ToUInt16(data, pointer);
            if (BitConverter.IsLittleEndian)
            {
                number = ReverseBytesFromInt16(number);
            }
            pointer += sizeof(short);
            return number;
        }

        public static ushort ToUInt16(byte[] data, int pointer)
        {
            var number = BitConverter.ToUInt16(data, pointer);
            if (BitConverter.IsLittleEndian)
            {
                number = ReverseBytesFromInt16(number);
            }
            return number;
        }
        
        public static uint ToUInt32(byte[] data,ref int pointer)
        {
            var number = BitConverter.ToUInt32(data, pointer);
            if (BitConverter.IsLittleEndian)
            {
                number = ReverseBytesFromInt132(number);
            }
            pointer += sizeof(int);
            return number;
        }
        
        private static ushort ReverseBytesFromInt16(ushort val)
        {
            byte[] intAsBytes = BitConverter.GetBytes(val);
            Array.Reverse(intAsBytes);
            return BitConverter.ToUInt16(intAsBytes, 0);
        }
        
        private static ushort ReverseBytesFromInt132(uint val)
        {
            byte[] intAsBytes = BitConverter.GetBytes(val);
            Array.Reverse(intAsBytes);
            return BitConverter.ToUInt16(intAsBytes, 0);
        }
    }
}