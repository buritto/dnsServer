using System;
using System.Linq;

namespace ConsoleApp1
{
    public class DNSFrame
    {
        //look https://www.ietf.org/rfc/rfc1035.txt from page 25
        public Header Header;
        public ResourceRecords Questions;
        public ExtensionResourceRecords Answers;
        public readonly ExtensionResourceRecords AuthorityRecords;
        public readonly ExtensionResourceRecords AdditionalRecords;
        public readonly byte[] FrameAsByte;
        private readonly int sizeHead = 12;
        private int framePointer;

        public DNSFrame()
        {
            
        }
        
        public DNSFrame(byte[] frameToByteArray)
        {
            if (!BitConverter.IsLittleEndian)
                frameToByteArray = frameToByteArray.Reverse().ToArray();
            FrameAsByte = frameToByteArray;
            Header = new Header(frameToByteArray.Take(sizeHead).ToArray());
            Questions = new ResourceRecords(frameToByteArray.Skip(sizeHead).ToArray(), Header.QDCOUNT,
                true);
            framePointer =  Questions.Pos + sizeHead - 1;
            if (Header.ANCOUNT > 0)
            {
                Answers = new ExtensionResourceRecords(frameToByteArray, framePointer ,Header.ANCOUNT);
                framePointer = Answers.Pointer;
            }

            if (Header.NSCOUNT > 0)
            {
                AuthorityRecords = new ExtensionResourceRecords(frameToByteArray, framePointer, Header.NSCOUNT);
                framePointer = AuthorityRecords.Pointer;
            }

            if (Header.ARCOUNT > 0)
            {
                AdditionalRecords = new ExtensionResourceRecords(frameToByteArray, framePointer, Header.ARCOUNT);
            }
        }
        
    }
}