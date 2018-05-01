using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DSNServer.Converter;
using Newtonsoft.Json;

namespace DSNServer.Handlers
{
    public class CasheValue : ICashe
    {
        public long Ttl { get; set; }
        public List<Record> Records;

        public CasheValue(List<Record> records)
        {
            Records = records;
            Ttl = Int64.MaxValue;
            foreach (var record in records)
            {
                Ttl = Math.Min(Ttl, record.Ttl);
            }
        }
    }
    
    public class AQueryHandler : Handler
    {
        
        public ConcurrentDictionary<string, CasheValue> Cache;
        private NSQueryHandler helper;
        public AQueryHandler(Socket server, NSQueryHandler helper, 
            ConcurrentDictionary<string, CasheValue> cashe = null) : base (server, 1)
        {
            this.helper = helper;
            Cache = GetCahseFromJsone<CasheValue>("AQeury.json")
                    ?? new ConcurrentDictionary<string, CasheValue>();
        }

        

        protected override async void ResolveQuestion(DNSFrame frame, EndPoint sender)
        {
            var answers = new List<Record>();
            foreach (var record in frame.Question.Records)
            {
                if (Cache.ContainsKey(record.Name))
                {

                    answers = Cache[record.Name].Records;
                    continue;
                }
                await RedirectQuestrionToMasterServer(frame);
                if (!Cache.ContainsKey(record.Name))
                    return;
                answers = Cache[record.Name].Records;
            }

            SendResponse(frame, answers, sender);
        }

        private void SendResponse(DNSFrame frame, List<Record> answers, EndPoint sender)
        {
            var responseFrame = new DNSFrame();
            responseFrame.Question = frame.Question;
            responseFrame.FrameHeader = frame.FrameHeader;
            responseFrame.FrameHeader.FrameFlags.isResponse = true;
            responseFrame.FrameHeader.AnswerCount = (ushort)answers.Count;
            responseFrame.FrameHeader.NameServerCount = 0;
            responseFrame.FrameHeader.AdditionalCount = 0;
            var offset = (ushort)frame.sizeOfHeader;
            responseFrame.Answer = new Resource();
            responseFrame.data = frame.data;
            for (int i = 0; i < answers.Count; i++)
            {
                responseFrame.Answer.Records.Add(answers[i]);  
            }

            socket.SendTo(DNSFrameConverter.ToByte(responseFrame), sender);
        }

   

        protected override void ResolveResponse(DNSFrame frame)
        {
            foreach (var record in frame.Question.Records)
            {
                if (frame.FrameHeader.AdditionalCount > 0 && frame.FrameHeader.NameServerCount > 0)
                {
                    helper.HelpResolveResponse(frame);
                }
                var dataForCache = new List<Record>();
                foreach (var answerRecord in frame.Answer.Records)
                {
                    if (answerRecord.Name == record.Name)
                    {
                        dataForCache.Add(answerRecord);
                    }
                }

                if (!Cache.ContainsKey(record.Name))
                {
                    Cache.TryAdd(record.Name, new CasheValue(dataForCache));
                }
                else
                {
                    Cache[record.Name] =  new CasheValue(dataForCache);
                }
            }
        }

        protected override void CheangeCasheData()
        {
            foreach (var pair in Cache)
            {
                pair.Value.Ttl--;
                if (pair.Value.Ttl <= 0)
                {
                    Cache.TryRemove(pair.Key, out _);
                }
            }
        }

        public override void SaveData()
        {
            string data = JsonConvert.SerializeObject(Cache);
            File.WriteAllText("AQeury.json", data);
        }
    }
}