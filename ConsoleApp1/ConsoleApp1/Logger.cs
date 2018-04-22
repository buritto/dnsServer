using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ConsoleApp1
{
    public static class Logger
    {
        public static string logingPath {private get; set;}
        
        public static void Log(DNSFrame dnsFrame)
        {
            using (var stream = File.AppendText(logingPath))
            {
                var line = $"Query from client, {dnsFrame.Header.Id}, {dnsFrame.Questions.Records.First().DomainName}";
                var time = DateTime.Now;
                stream.WriteLine($"{time.Year}:{time.Month}:{time.Day}:{time.Hour}:{time.Minute}:{time.Second} - {line}\n");
            }
        }

        public static void LogError(string error, DNSFrame dnsFrame)
        {
            using (var stream = File.AppendText(logingPath))
            {
                var line = $"Exception : {error} in {dnsFrame.Header.Id},{dnsFrame.Questions.Records.First().DomainName} ";
                var time = DateTime.Now;
                stream.WriteLine($"{time.Month}:{time.Day}:{time.Hour}:{time.Minute}:{time.Second} - {line}\n");
            }
        }
    }
}