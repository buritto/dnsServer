using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DSNServer.Converter;
using Newtonsoft.Json;

namespace DSNServer.Handlers
{
    public class Pair : ICashe
    {
        public List<Record> Answer;
        public List<Record> Additional;
        public long Ttl { get; set; }

        public Pair(List<Record> answer, List<Record> additional)
        {
            Ttl = Int64.MaxValue;
            Answer = answer;
            Additional = additional;
            foreach (var record in answer)
            {
                Ttl = Math.Min(Ttl, record.Ttl);
            }

            foreach (var record in additional)
            {
                Ttl = Math.Min(Ttl, record.Ttl);
            }
        }
    }
    
    public class NSQueryHandler : Handler
    {
        public ConcurrentDictionary<string, Pair> Cashe;
        
        public NSQueryHandler(Socket server, ConcurrentDictionary<string, Pair> cashe = null) 
            : base(server, 2)
        {
            Cashe = GetCahseFromJsone<Pair>("NSQeury.json") 
                    ?? new ConcurrentDictionary<string, Pair>();
        }

        protected override async void ResolveQuestion(DNSFrame frame, EndPoint sender)
        {
            foreach (var question in frame.Question.Records)
            {
                if (Cashe.ContainsKey(question.Name))
                {
                    SendResponce(frame, Cashe[question.Name], sender, question);
                    continue;
                }
                await RedirectQuestrionToMasterServer(frame);
                if (!Cashe.ContainsKey(question.Name))
                    continue;
                 SendResponce(frame, Cashe[question.Name], sender, question);
            }
        }

        private void SendResponce(DNSFrame frame, Pair answer, EndPoint sender, Record question)
        {
            var newFrame = new DNSFrame();
            newFrame.Question = new Resource();
            newFrame.Question.LenghtResource = question.LengthRecord;
            newFrame.Question.Records = new List<Record>(){question};
            newFrame.FrameHeader = frame.FrameHeader;
            newFrame.FrameHeader.FrameFlags.isResponse = true;
            newFrame.Answer = new Resource {Records = answer.Answer};
            newFrame.Additional = new Resource {Records = answer.Additional};
            newFrame.FrameHeader.AnswerCount = (ushort)answer.Answer.Count;
            newFrame.FrameHeader.AdditionalCount = (ushort) answer.Additional.Count;
            newFrame.data = frame.data;
            socket.SendTo(DNSFrameConverter.ToByte(newFrame), sender);
        }

        private string GetName(int pointer, byte[] data)
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
                if (data[pointer] != 0)
                    name.Append(".");
            }

            if (data[pointer] == 0)
            {
                return name.ToString();
            }
            var offset = MyBitConverter.ToUInt16(data, pointer) & 16383;
            return name + GetName(offset, data);
        }

        protected IEnumerable<byte> ConvertData(string data)
        {
            var result = new List<byte>();
            var blocs = data.Split(".");
            foreach (var bloc in blocs)
            {
                result.Add((byte)bloc.Length);
                for (int i = 0; i < bloc.Length; i++)
                {
                    result.Add(Convert.ToByte(bloc[i]));
                }
            }
            result.Add(0);
            return result;
        }

        public void HelpResolveResponse(DNSFrame frame)
        {
            var newDns = new DNSFrame();
            newDns.Answer = frame.Authority;
            newDns.Additional = frame.Additional;
            newDns.data = frame.data;
            newDns.Question = frame.Question;
            ResolveResponse(newDns);
        }
        
        protected override void ResolveResponse(DNSFrame frame)
        {
            var questionPos = 12;
            foreach (var question in frame.Question.Records)
            {
                var answers = new List<Record>();
                var additionals = new List<Record>();
                if (Cashe.ContainsKey(question.Name))
                    continue;
                var pointer = frame.sizeOfHeader + question.LengthRecord + 49152;
                foreach (var answer in frame.Answer.Records)
                {
                    if (answer.Name != question.Name)
                        continue;
                    answers.Add(answer);
                    answer.CompressionPointer = (ushort)(questionPos + 49152);
                    var name = GetName(answer.PointerToData, frame.data);
                    answer.Data = ConvertData(name);
                    answer.LengthRecord -= answer.DataLenght;
                    answer.DataLenght = (ushort)(name.Count() + 2);
                    answer.LengthRecord += answer.DataLenght;
                    pointer += answer.LengthRecord;
                    foreach (var record in frame.Additional.Records)
                    {
                        if ( DeleteDot(name) != record.Name)
                            continue;
                        
                        record.CompressionPointer = (ushort)(pointer - answer.DataLenght);
                        additionals.Add(record);
                    }
                }

                Cashe.TryAdd(question.Name, new Pair(answers, additionals));
                questionPos += question.LengthRecord;
            }
            
        }

        protected override void CheangeCasheData()
        {
            foreach (var pair in Cashe)
            {
                pair.Value.Ttl--;
                if (pair.Value.Ttl <= 0)
                {
                    Cashe.TryRemove(pair.Key,out _ );
                }
            }
        }

        public override void SaveData()
        {
            string data = JsonConvert.SerializeObject(Cashe);
            File.WriteAllText("NSQeury.json", data);
        }

        private string DeleteDot(string name)
        {
            var newName = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] != '.')
                    newName.Append(name[i]);
            }
            return newName.ToString();
        }
    }
}