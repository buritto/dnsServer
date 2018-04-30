using System;
using System.Reflection;
using DSNServer.Converter;

namespace DSNServer
{
    public class Flasg
    {
        public bool isResponse;
        public ushort Opcode;
        public bool IsAuthoritativeAnswer;
        public bool IsTrunCation;
        public bool RecursionDesired;
        public bool RecursionAvailable;
        public ushort Rcode;
        public Flasg(ushort data)
        {
            isResponse = ((data & 32768) >> 15) == 1;
            Opcode = (ushort)((data & 30720) >> 14);
            IsAuthoritativeAnswer = ((data & 1024) >> 10) == 1;
            IsTrunCation = (data & 512 >> 9) == 1;
            RecursionDesired = ((data & 256) >> 8) == 1;
            RecursionAvailable = (data & 128 >> 7) == 1;
            Rcode = (ushort)(data & 8);
        }
    }
    
    public class Header
    {
        public ushort Id;
        public Flasg FrameFlags;
        public ushort QuestionCount;
        public ushort AnswerCount;
        public ushort NameServerCount;
        public ushort AdditionalCount;
        
        public Header(byte[] data)
        {
            var pointer = 0;
            Id = MyBitConverter.ToUInt16(data, ref pointer);
            FrameFlags = new Flasg(MyBitConverter.ToUInt16(data, ref pointer));
            QuestionCount = MyBitConverter.ToUInt16(data, ref pointer);
            AnswerCount = MyBitConverter.ToUInt16(data, ref pointer);
            NameServerCount = MyBitConverter.ToUInt16(data, ref pointer);
            AdditionalCount = MyBitConverter.ToUInt16(data, ref pointer);
        }
    }
}