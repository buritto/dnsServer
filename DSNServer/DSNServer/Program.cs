using System;
using DSNServer.Handlers;

namespace DSNServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Controller.CreateServer("192.168.0.105");
            Controller.StartServer();
        }
    }
}