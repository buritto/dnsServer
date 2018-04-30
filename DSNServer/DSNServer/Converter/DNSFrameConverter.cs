using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace DSNServer.Converter
{
    public static class DNSFrameConverter
    {
        public static byte[] ToByte(DNSFrame frame)
        {
            var sequence = new List<byte>();
            sequence.AddRange(GetHeaderAsByte(frame.FrameHeader));
            sequence.AddRange(frame.data.Skip(frame.sizeOfHeader).Take(frame.Question.LenghtResource));
            if (frame.FrameHeader.AnswerCount > 0)
                sequence.AddRange(GetDataFromRecords(frame.Answer));
            if (frame.FrameHeader.NameServerCount > 0)
                sequence.AddRange(GetDataFromRecords(frame.Authority));
            if (frame.FrameHeader.AdditionalCount > 0)
                sequence.AddRange(GetDataFromRecords(frame.Additional));
            return sequence.ToArray();

        }
        
        private static IEnumerable<byte> GetDataFromRecords(Resource resource)
        {
            var sequence = new List<byte>();
            foreach (var record in resource.Records)
            {
                sequence.AddRange(GetBytes(record.CompressionPointer));
                sequence.AddRange(GetBytes(record.QType));
                sequence.AddRange(GetBytes(record.QClass));
                sequence.AddRange(BitConverter.GetBytes(record.Ttl).Reverse());
                sequence.AddRange(GetBytes(record.DataLenght));
                sequence.AddRange(record.Data);
            }

            return sequence;
        }

        private static byte[] GetHeaderAsByte(Header frameHeader)
        {
            var sequence = new List<byte>();
            sequence.AddRange(GetBytes(frameHeader.Id));
            sequence.AddRange(GetBytesFrom(frameHeader.FrameFlags));
            sequence.AddRange(GetBytes(frameHeader.QuestionCount));
            sequence.AddRange(GetBytes(frameHeader.AnswerCount));
            sequence.AddRange(GetBytes(frameHeader.NameServerCount));
            sequence.AddRange(GetBytes(frameHeader.AdditionalCount));
            return sequence.ToArray();
        }

        private static IEnumerable<byte> GetBytes(ushort data)
        {
            var res = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
                return res.Reverse();
            return res;

        }
        
        private static IEnumerable<byte> GetBytesFrom(Flasg flags)
        {
            ushort data = (ushort)(Convert.ToInt32(flags.isResponse) * 32768);
            data += (ushort)(flags.Opcode * 2048);
            data += (ushort)(Convert.ToInt32(flags.IsAuthoritativeAnswer) * 1024);
            data += (ushort)(Convert.ToInt32(flags.IsTrunCation) * 512);
            data += (ushort)(Convert.ToInt32(flags.RecursionDesired) * 256);
            data += (ushort)(Convert.ToInt32(flags.RecursionAvailable) * 128);
            data += flags.Rcode;
            if (BitConverter.IsLittleEndian)
                return BitConverter.GetBytes(data).Reverse();
            return BitConverter.GetBytes(data);
        }
    }
}        