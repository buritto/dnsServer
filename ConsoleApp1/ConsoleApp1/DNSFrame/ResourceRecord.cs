using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace ConsoleApp1
{

    public class Record
    {
        public readonly string DomainName;
        public readonly ushort QType;
        public readonly ushort QClass;
        public readonly uint Ttl;
        public readonly ushort RdLength;
        public readonly byte[] RData;
        public readonly byte[] DomainNamePointer;

        public Record(string domainName, ushort qType, ushort qClass,
            uint ttl = 0, ushort rdLength = 0, byte[] rData = null, byte[] orgOffset = null)
        {
            DomainName = domainName;
            QType = qType;
            QClass = qClass;
            Ttl = ttl;
            RdLength = rdLength;
            RData = rData;
            DomainNamePointer = orgOffset;
        }
        public Record(Record question, Responce responce )
        {
            DomainName = question.DomainName;
            QType = question.QType;
            QClass = question.QClass;
            Ttl = responce.Ttl;
            DomainNamePointer = responce.DataNameOffset;
            RdLength = (ushort)responce.Rdata.Length;
            RData = responce.Rdata;
        }
    }

    public class ExtensionResourceRecords
    {
        public readonly List<Record> Records;
        public int Pointer { get; private set; }

        public ExtensionResourceRecords()
        {
           Records = new List<Record>();
        }
        public ExtensionResourceRecords(byte[] frame, int pointer, int countResourceRecords)
        {
            Pointer = pointer;
            Records = GetRecords(frame, countResourceRecords);
        }

        private List<Record> GetRecords(byte[] frame, int countResourceRecords)
        {
            var records = new List<Record>();
            for (int j = 0; j < countResourceRecords; j++)
            {
                int offset = BitConverter.ToUInt16(ReversBytes(frame, Pointer), 0);
                var origanlOffset = ReversBytes(frame, Pointer);
                offset = offset & 16383;
                var name = GetName(frame, offset);
                MovePointer(2);
                var qType = BitConverter.ToUInt16(ReversBytes(frame, Pointer), 0);
                MovePointer(2);
                var qClass = BitConverter.ToUInt16(frame, Pointer);
                MovePointer(2);
                var ttl = BitConverter.ToUInt32(ReversBytes(frame, Pointer, 4) , 0);
                MovePointer(4);
                var rdLength = BitConverter.ToUInt16(ReversBytes(frame,Pointer), 0);
                MovePointer(2);
                var rData = frame.Skip(Pointer).Take(rdLength).ToArray();
                MovePointer(rdLength);
                records.Add(new Record(name, qType, qClass, ttl, rdLength, rData, origanlOffset));
            }
            return records;
        }

        private void MovePointer(int offset)
        {
            Pointer += offset;
        }
        
        private static byte[] ReversBytes(IReadOnlyList<byte> frame, int ponter, int count = 2)
        {
            var reversesSequence = new byte[count];
            for (int i = 0; i < count; i++)
            {
                reversesSequence[i] = frame[ponter + count - (i + 1)];
            }
            return reversesSequence;
        }
        
        private static string GetName(IReadOnlyList<byte> frame, int pointer)
        {
            //Console.WriteLine(pointer);
            var name = new StringBuilder();
            while (frame[pointer] != 0)
            {
                var offset = frame[pointer];
                for (int i = pointer + 1; i <= pointer + offset; i++)
                {
                    name.Append(Convert.ToChar(frame[i]));
                }

                pointer += offset + 1;
                name.Append('.');
            }

            return name.ToString();
        }
    }
    
    public class ResourceRecords
    {
        public List<Record> Records;
        public int Pos { get; private set; }

        public ResourceRecords()
        {
            Records = new List<Record>();
            Pos = 0;
        }
        
        public ResourceRecords(byte[] byteSequence, int countResourceRecords, bool isQuestion)
        {
            Records = GetRecords(byteSequence, countResourceRecords, isQuestion);
        }
        
        private List<Record> GetRecords(byte[] byteSequence, int countResourceRecords, bool shortRecord)
        {
            var records = new List<Record>();
            for (int j = 0; j < countResourceRecords; j++)
            {
                var name = new StringBuilder();
                while (byteSequence[Pos] != 0)
                {
                    var offset = byteSequence[Pos];
                    for (int i = Pos + 1; i <= Pos + offset; i++)
                    {
                        name.Append(Convert.ToChar(byteSequence[i]));
                    }

                    Pos += offset + 1;
                    name.Append('.');
                }

                Pos++;
                var qType = BitConverter.ToUInt16(new byte[]{byteSequence[Pos+1], byteSequence[Pos]},0);
                Pos += 2;
                var qClass = BitConverter.ToUInt16(byteSequence, Pos);
                Pos += 3;
                records.Add(new Record(name.ToString(), qType, qClass));

            }

            return records;

        }
    }
}