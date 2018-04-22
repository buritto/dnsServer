using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    public static class Converter
    {
        public  static byte[] DnsFrameToByte(DNSFrame frame)
        {
            var byteSequence = new List<byte>();
            byteSequence.AddRange(HeaderToBytes(frame.Header));
            byteSequence.AddRange(QuestionsToBytes(frame.Questions));
            if(frame.Answers != null)
                byteSequence.AddRange(RecordToBytes(frame.Answers));
            if (frame.AuthorityRecords != null)
                byteSequence.AddRange(RecordToBytes(frame.AuthorityRecords));
            if (frame.AdditionalRecords != null)
                byteSequence.AddRange(RecordToBytes(frame.AdditionalRecords));
            return byteSequence.ToArray();
        }

        private static IEnumerable<byte> QuestionsToBytes(ResourceRecords frameQuestions)
        {
            var byteSequence = new List<byte>();
            foreach (var record in frameQuestions.Records)
            {
                byteSequence.AddRange(GetDomainNameToByte(record.DomainName));
                byteSequence.AddRange(ReverseBytes(BitConverter.GetBytes(record.QType)));
                byteSequence.AddRange(BitConverter.GetBytes(record.QClass));
            }

            return byteSequence;    
        }

        private static IEnumerable<byte> HeaderToBytes(Header frameHeader)
        {
            var byteSequence = new List<byte>();
            byteSequence.AddRange(BitConverter.GetBytes(frameHeader.Id));
            byteSequence.Add((byte)((frameHeader.QR << 7) + (frameHeader.OpCode << 3)
                             + (Convert.ToByte(frameHeader.IsAutAns) << 2)
                             + (Convert.ToByte(frameHeader.TrunCation) << 1) 
                             + (Convert.ToByte(frameHeader.IsRecursion))));
            byteSequence.Add((byte)(Convert.ToByte(frameHeader.RecursionAvailable) << 7
                                    + frameHeader.RCODE));    
            byteSequence.AddRange(ReverseBytes(BitConverter.GetBytes(frameHeader.QDCOUNT)));
            byteSequence.AddRange(ReverseBytes(BitConverter.GetBytes(frameHeader.ANCOUNT)));
            byteSequence.AddRange(ReverseBytes(BitConverter.GetBytes(frameHeader.NSCOUNT)));
            byteSequence.AddRange(ReverseBytes(BitConverter.GetBytes(frameHeader.ANCOUNT)));
            return byteSequence;
        }

        private static IEnumerable<byte> ReverseBytes(IReadOnlyList<byte> bytes, int count = 2)
        {
            return bytes.Take(count).Reverse();
        }
        
            private static IEnumerable<byte> RecordToBytes(ExtensionResourceRecords records)
        {
            var byteSequence = new List<byte>();
            foreach (var record in records.Records)
            {
                byteSequence.AddRange(ReverseBytes(record.DomainNamePointer));
                byteSequence.AddRange(ReverseBytes(BitConverter.GetBytes(record.QType)));
                byteSequence.AddRange(BitConverter.GetBytes(record.QClass));
                byteSequence.AddRange(ReverseBytes(BitConverter.GetBytes(record.Ttl), 4));
                byteSequence.AddRange(ReverseBytes(BitConverter.GetBytes(record.RdLength)));
                byteSequence.AddRange(record.RData);
            }

            return byteSequence;
        }

        private static IEnumerable<byte> GetDomainNameToByte(string domainName)
        {
            var byteSequence = new List<byte>();
            foreach (var domain in domainName.Split(".").Where(d => d.Length > 0))
            {
                byteSequence.Add((byte)domain.Length);
                byteSequence.AddRange(domain.Select(Convert.ToByte));
            }
            
            byteSequence.Add(0);
            return byteSequence;



        }
    }
}