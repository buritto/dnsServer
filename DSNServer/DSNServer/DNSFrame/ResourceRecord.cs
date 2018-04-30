using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSNServer.Converter;

namespace DSNServer
{
    public class Record
    {
        public int Pointer;
        public string Name;
        public ushort QType;
        public ushort QClass;
        public uint Ttl;
        public ushort CompressionPointer;
        public ushort DataLenght;
        public IEnumerable<byte> Data;
        public int LengthRecord;
        public ushort PointerToData;
        
        private ushort[] availableTypes = {12, 2, 1};

        public Record(byte[] data, int pointer, bool isQuestion = false)
        {
            var previousePointer = pointer;
            Name = GetName(data, ref pointer);
            QType = MyBitConverter.ToUInt16(data, ref pointer);
            if (!availableTypes.Contains(QType))
               throw new Exception($"Incorrect type {QType}");
            QClass = MyBitConverter.ToUInt16(data, ref pointer);
            if (isQuestion)
            {
                LengthRecord = pointer - previousePointer;
                Pointer = pointer;
                return;
            }

            Ttl = MyBitConverter.ToUInt32(data, ref pointer);
            DataLenght = MyBitConverter.ToUInt16(data, ref pointer);
            PointerToData = (ushort)pointer;
            Data = data.Skip(pointer).Take(DataLenght);
            Pointer = pointer + DataLenght;
            LengthRecord = Pointer - previousePointer;
        }

        private string GetName(byte[] data,ref int pointer)
        {
            var name = new StringBuilder();
            while (data[pointer] < 192 && data[pointer] != 0 )
            {
                var length = data[pointer];
                for (int i = pointer + 1; i <= pointer + length; i++)
                {
                    name.Append(Convert.ToChar(data[i]));
                }
                pointer += length + 1;
            }

            if (data[pointer] == 0)
            {
                pointer++;
                return name.ToString();
            }

            var p = MyBitConverter.ToUInt16(data, ref pointer);
            CompressionPointer = p;
            var offset = GetOffset(p);
            return name + GetName(data, ref offset);
        }

        private int GetOffset(ushort p)
        {
            return p & 16383;
        }
    }

}