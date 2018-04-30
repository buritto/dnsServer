using System;
using DSNServer.Handlers;

namespace DSNServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server("10.113.226.187");
            var handler1 = new AQueryHandler(server.MainSocket);
            var handler2 = new NSQueryHandler(server.MainSocket);
            server.Subscribe(handler1);
            server.Subscribe(handler2);
            server.Listen();
        }
    }
}