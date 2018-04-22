using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{


    public class QuestionSection : ResourceRecords
    {
        public QuestionSection(byte[] byteSequence, int countResourceRecords) 
            : base(byteSequence, countResourceRecords, true)
        {
        }
    }
}