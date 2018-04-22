using System;
using System.Net;
using DnsServer;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.logingPath = "server.log";
            ServerControler.CreateDnsServer("10.113.226.187");
            ServerControler.StartServer();
            while (true)
            {
                var command = Console.ReadLine();
                if (command != ServerControler.CommandQuite) continue;
                Console.WriteLine(ServerControler.CommandQuite);
                break;
            }
            ServerControler.TurnOffServer();    
        }
    }
}