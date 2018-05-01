using System;
using System.Threading;
using System.Threading.Tasks;
using DSNServer.Handlers;

namespace DSNServer
{
    public static class Controller
    {
        private static Server server;
        private static string exitCommand = "exit";
        
        public static void CheangeCasheData()
        {
            while (true)
            {
                Thread.Sleep(1000);
                server.CheangeDataCashe();
            }
            
        }
        
        public static void CreateServer(string ipAddress)
        {
            if (server == null)
            {
                server = new Server(ipAddress);
                var nsHandler = new NSQueryHandler(server.MainSocket);
                var aHandler = new AQueryHandler(server.MainSocket, nsHandler);
                server.Subscribe(nsHandler, aHandler);
            }

        }

        public static void StartServer()
        {
            Task.Run(() => server.Listen());
            Task.Run(() => CheangeCasheData());
            while (true)
            {
                var command = Console.ReadLine();
                if (command == exitCommand)
                {
                    server.ServerTurnOff();
                    break;
                }
            }
        }
    }
}