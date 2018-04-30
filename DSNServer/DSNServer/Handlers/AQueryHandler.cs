using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DSNServer.Converter;

namespace DSNServer.Handlers
{
    public class AQueryHandler : Handler
    {
        public ConcurrentDictionary<string, List<Record>> Cache;
        public AQueryHandler(Socket server) : base (server, 1)
        {
            Cache = new ConcurrentDictionary<string, List<Record>>();
        }

        protected override async void ResolveQuestion(DNSFrame frame, EndPoint sender)
        {
            var answers = new List<Record>();
            foreach (var record in frame.Question.Records)
            {
                if (Cache.ContainsKey(record.Name))
                {
                    answers = Cache[record.Name];
                    continue;
                }
                await RedirectQuestrionToMasterServer(frame);
                if (!Cache.ContainsKey(record.Name))
                    return;
                answers = Cache[record.Name];
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
                    Cache.TryAdd(record.Name, dataForCache);
                }
                else
                {
                    Cache[record.Name] =  dataForCache;
                }
            }
        }
    }
}