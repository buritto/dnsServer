using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DnsServer;
using Newtonsoft.Json;

namespace ConsoleApp1
{

    public static class ServerControler
    {

        public static string CommandQuite = "quite";
        private static string NameCollectEntryDomainToIp = "Domain.To.Ip.json";
        private static string NameCollectEntryIpToDomain = "Ip.To.Domain.json";
        private static DNSServer _server;


        public static void SetDnsServer(DNSServer server)
        {
            _server = server;
        }

        public static void CreateDnsServer(string ipAddressOfServer)
        {
            var domainToIp = GetDataFromJson(NameCollectEntryDomainToIp);
            var ipToDomain = GetDataFromJson(NameCollectEntryIpToDomain);
            _server =  new DNSServer(IPAddress.Parse(ipAddressOfServer), 53, ipToDomain, domainToIp);
        }

        private static ConcurrentDictionary<string, List< Responce>> 
            GetDataFromJson(string nameCollectEntry)
        {
            if (!File.Exists(nameCollectEntry))
                return null;
            ConcurrentDictionary<string, List<Responce>> items = null;
            var date = File.GetLastWriteTime(nameCollectEntry);
            using (StreamReader r = new StreamReader(nameCollectEntry))
            {
                string json = r.ReadToEnd();
                items = JsonConvert
                    .DeserializeObject<ConcurrentDictionary<string, List< Responce>> >(json);
            }

            if (items is null)
                return null;
            var differentTime = (uint)(DateTime.Now - date).TotalSeconds;
            foreach (var item in items)
            {
                foreach (var record in item.Value)
                {
                    if (record.Ttl >= differentTime)
                    {
                        record.Ttl -= differentTime;
                        continue;
                    }
                    items.TryRemove(item.Key, out _);
                }
            }

            return items;
        }

        public static async void StartServer()
        {
            if (_server is null)
                throw new NullReferenceException("Server has't created or set");
            
            await Task.Run(() =>
            {
                ReduceTtl();
                _server.ServerStart();
            });
        }


        private static void Reduce(ref ConcurrentDictionary<string,  List< Responce>> collectronResponces)
        {
            foreach (var record in collectronResponces)
            {
                var IsRecordWillDeleted = false;
                //const int i = 0;
                foreach (var respone in record.Value)
                {
                    // Console.WriteLine($"{i} - {respone.Ttl}");
                    respone.Ttl--;
                    if (respone.Ttl < 1)
                        IsRecordWillDeleted = true;

                }
                if (IsRecordWillDeleted)
                {
                    var list=  new List<Responce>();
                    collectronResponces.TryRemove(record.Key, out list);
                };
            }
        }
        
        private static async void ReduceTtl()
        {
            await Task.Run(() =>
            {
                while (_server.serverIsOn)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    Reduce(ref _server.DomainToIp);
                    Reduce(ref _server.IpToDomain);
                }
                
            });
        }

        public static void TurnOffServer()
        {
            if (_server is null)
            {
                throw new NullReferenceException("Server has't created or set");
            }

            _server.TurnOff();
            SaveData();
        }

        private static void SaveData()
        {
            string domainToIp = JsonConvert.SerializeObject(_server.DomainToIp);
            string ipToDomain = JsonConvert.SerializeObject(_server.IpToDomain);
            File.WriteAllText(NameCollectEntryDomainToIp, domainToIp);
            File.WriteAllText(NameCollectEntryIpToDomain, ipToDomain);
        }
    }
}