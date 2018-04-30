using System.Linq;

namespace DSNServer
{
    public class DNSFrame
    {
        public Header FrameHeader;
        public Resource Question;
        public Resource Answer;
        public Resource Authority;
        public Resource Additional;
        public byte[] data;
        public readonly int sizeOfHeader = 12;

        public DNSFrame()
        {
            
        }
        
        public DNSFrame(byte[] data, int countData)
        {
            if (countData > 100)
            {
                
                //12
            }
            this.data = data.Take(countData).ToArray();
            var currentPointer = 0;
            FrameHeader = new Header(data);
            currentPointer = sizeOfHeader;
            Question = new Resource(data, currentPointer, FrameHeader.QuestionCount, true);
            currentPointer = Question.RightBoundResource;
            if (FrameHeader.AnswerCount != 0)
            {
                Answer = new Resource(data, currentPointer, FrameHeader.AnswerCount);
                currentPointer = Answer.RightBoundResource;
            }

            if (FrameHeader.NameServerCount != 0)
            {
                Authority = new Resource(data, currentPointer, FrameHeader.NameServerCount);
                currentPointer = Authority.RightBoundResource;
            }

            if (FrameHeader.AdditionalCount != 0)
            {
                Additional = new Resource(data, currentPointer, FrameHeader.AdditionalCount );
            }
        }
    }
}