using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp1;

namespace DnsServer
{
    public class DNSServer
    {
        private readonly Socket socket;    
        private readonly int timeout;
        public bool serverIsOn = true;
        public  ConcurrentDictionary<string, List< Responce>> IpToDomain;
        public  ConcurrentDictionary<string, List<Responce>> DomainToIp;
        private readonly EndPoint remoteServer = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53) as EndPoint;
        public DNSServer(IPAddress address, int port, 
            ConcurrentDictionary<string, List<Responce>> ipToDomain = null, 
            ConcurrentDictionary<string, List<Responce>> domainToIp = null )
        {
            timeout = 2000;
            var connected = new IPEndPoint(address, port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(connected);
            if (domainToIp is null)
                Console.WriteLine("be null");
            IpToDomain = ipToDomain ?? new ConcurrentDictionary<string, List<Responce>>();
            DomainToIp = domainToIp ?? new ConcurrentDictionary<string, List<Responce>>();
        }

        public void ServerStart()
        {
            Listen();
        }
        
        private void Listen()
        {
            while (serverIsOn)
            {
                EndPoint clientIpPoint = new IPEndPoint(IPAddress.None, 53);
                var buffer = new byte[4096];
                var countReceiveData = socket.ReceiveFrom(buffer, ref clientIpPoint);
                var dnsFrame = new DNSFrame(buffer.Take(countReceiveData).ToArray());
                if (dnsFrame.Header.QR == 0)
                {
                   // Console.WriteLine("Query");                    
                    Logger.Log(dnsFrame);
                    ResolveQuestion(dnsFrame, clientIpPoint);
                    continue;
                }
                Logger.Log(dnsFrame);
                ResolveResponce(dnsFrame);
            }
        }

        private async void ResolveQuestion(DNSFrame dnsFrame, EndPoint ipAddress)
        {
            var answer = new ExtensionResourceRecords();
            foreach (var question in dnsFrame.Questions.Records)
            {
                var source = DomainToIp;
                //Console.WriteLine(question.QType + "<-QType");
                if (question.QType == 12)
                {
                    //Console.WriteLine("12");
                    source = IpToDomain;    
                }

                if (source.ContainsKey(question.DomainName))
                {
                    var responces = source[question.DomainName];
                    foreach (var responce in responces)
                    {
                        var dnsResponce = new Record(question, responce);    
                        answer.Records.Add(dnsResponce);
                    }

                }
                else
                {
                    //Если в кэше нет данных, то делать запрос к вышестоящиму серверу 
                    //делаьь это нужно ассинхронно, т.е awat void у этого метода т.к нам не 
                    //важен резульатат, и если таймаут истёк, ну шо поделать отправляем клиенту
                    //наши сожиления по этому поводу
                    await SendQueryAsync(dnsFrame);
                    if (!source.ContainsKey(question.DomainName))
                    {
                        SendBadResponse(dnsFrame, ipAddress);
                        break;
                    }

                    var responces = source[question.DomainName];
                    foreach (var responce in responces)
                    {

                        var dnsResponce = new Record(question, responce);
                        answer.Records.Add(dnsResponce);
                    }
                }

            }

            SendSuccessfulResponse(dnsFrame, answer, ipAddress);
        }

        private void SendSuccessfulResponse(DNSFrame dnsFrame, ExtensionResourceRecords answer, EndPoint ipAddress)
        {
            var responseFrame = dnsFrame;
            responseFrame.Header.QR = 1;
            responseFrame.Header.ANCOUNT = (ushort)answer.Records.Count;
            responseFrame.Answers = answer;
            var frameAsBytes = Converter.DnsFrameToByte(responseFrame);
            socket.SendTo(frameAsBytes, ipAddress);
        }

        private void SendBadResponse(DNSFrame dnsFrame, EndPoint ipAddress)
        {
            dnsFrame.Header.RCODE = 2;
            try
            {
                socket.SendTo(Converter.DnsFrameToByte(dnsFrame), ipAddress);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message, dnsFrame);
            }
        }

        private async Task SendQueryAsync(DNSFrame dnsFrame)
        {
            try
            {
                socket.SendTo(dnsFrame.FrameAsByte, remoteServer);  
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message, dnsFrame);
            }
            
            await Task.Run(() =>
            {
                Thread.Sleep(timeout);
            });
            
        }    

        private void ResolveResponce(DNSFrame dnsFrame)
        {
            var answers = dnsFrame.Answers != null ? dnsFrame.Answers.Records : dnsFrame.AuthorityRecords.Records;
            for (int i = 0 ; i < answers.Count; i++)
            {
                Console.WriteLine(answers[i].Ttl + "<--ttl");
                var answer = answers[i];
                if (answer.QType == 1)
                {
                    AddNewPairInDomainToIp(answer);
                    continue;
                }    

                if (answer.QType == 12)
                {
                    AddNewPairInIpToDomain(answer);
                    continue;
                }
                ResolveNsRecord(dnsFrame,answer, i);
            }
        }

        private void ResolveNsRecord(DNSFrame dnsFrame, Record answer, int numberAnswer)
        {
            var queryFrame = new DNSFrame {Header = dnsFrame.Header};
            queryFrame.Header.QR = 0;
            queryFrame.Questions = new ResourceRecords
            {
                Records = new List<Record>()
                {
                    new Record(answer.DomainName, dnsFrame.Questions.Records[numberAnswer].QType, answer.QClass,
                        answer.Ttl)
                }
            };
            ResolveQuestion(dnsFrame, socket.LocalEndPoint);
        }

        private void AddNewPairInIpToDomain(Record answer)
        {
            var responce = new Responce(answer.Ttl, answer.RData, answer.DomainNamePointer);
            if (IpToDomain.ContainsKey(answer.DomainName))
            {
                IpToDomain[answer.DomainName].Add(responce);
            }
            IpToDomain.TryAdd(answer.DomainName,new List<Responce>(){responce});
        }
        
        private void AddNewPairInDomainToIp(Record answer)
        {
            var responce = new Responce(answer.Ttl, answer.RData, answer.DomainNamePointer);
            if (DomainToIp.ContainsKey(answer.DomainName))
            {
                DomainToIp[answer.DomainName].Add(responce);
            }
            DomainToIp.TryAdd(answer.DomainName,new List<Responce>(){responce});
        }

        public void TurnOff()
        {
            serverIsOn = false;
        }
    }
}