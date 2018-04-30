using System.Collections.Generic;
using System.Linq;

namespace DSNServer
{
    public class Resource
    {
        public List<Record> Records;
        public int LenghtResource;
        public int RightBoundResource;

        public Resource()
        {
            Records = new List<Record>();
        }
        
        public Resource(byte[] data, int pointer, int countRecord, bool isQuestion = false)
        {
            Records = new List<Record>();
            for (int i = 0; i < countRecord; i++)
            {
                var record = new Record(data, pointer, isQuestion);
                Records.Add(record);
                LenghtResource += record.LengthRecord;
                RightBoundResource = record.Pointer;
                pointer = record.Pointer;
            }
        }
    }
}