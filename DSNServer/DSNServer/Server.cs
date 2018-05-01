using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DSNServer
{
    public class Server : IPublisher
    {
        public Socket MainSocket;
        private bool serverOn;
        public Server(string ipAddress)
        {
            var connected = new IPEndPoint(IPAddress.Parse(ipAddress), 53);
            MainSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            MainSocket.Bind(connected);
            MySubscribers = new List<ISubscriber>();
        }

        public void Listen()
        {
            serverOn = true;
            while (serverOn)
            {
                var buffer = new byte[1024];
                EndPoint client = new IPEndPoint(IPAddress.None, 53);
                var count = MainSocket.ReceiveFrom(buffer, ref client);
                var dnsFrame = new DNSFrame(buffer, count);
                Notify(dnsFrame, client);
            }
        }

        public List<ISubscriber> MySubscribers { get; set; }

        public void ServerTurnOff()
        {
            foreach (var subscriber in MySubscribers)
            {
                subscriber.SaveData();
            }
            serverOn = false;
        }
        
        public void Notify(DNSFrame frame, EndPoint client)
        {
            MySubscribers.ForEach(s => s.Updata(frame, client));
        }

        public void Subscribe(params ISubscriber[] subscriber)
        {
            MySubscribers.AddRange(subscriber);
        }

        public void Unsubscribe(ISubscriber subscriber)
        {
            MySubscribers.Remove(subscriber);
        }

        public void CheangeDataCashe()
        {
            foreach (var sub  in MySubscribers)
            {
                sub.CheangeCashe();
            }
        }
    }
}