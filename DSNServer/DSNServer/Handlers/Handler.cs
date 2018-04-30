using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DSNServer.Handlers
{
    public abstract class Handler : ISubscriber
    {
        protected readonly Socket socket;
        protected readonly int Timeout = 2000;
        protected readonly EndPoint RemoteServer = 
            new IPEndPoint(IPAddress.Parse("212.193.163.6"), 53) as EndPoint;

        protected readonly ushort QType;
        
        
        public Handler(Socket server, ushort qType)
        {
            socket = server;
            QType = qType;
        }
        
        public void Updata(DNSFrame frame, EndPoint sender)
        {
            
            foreach (var record in frame.Question.Records)
            {
                if (record.QType != QType)
                    return;
            }

            if (frame.FrameHeader.FrameFlags.isResponse)
            {
                ResolveResponse(frame);
                
            }
            else
            {
                ResolveQuestion(frame, sender);
            }
        }
        
        protected async Task RedirectQuestrionToMasterServer(DNSFrame frame)
        {
            socket.SendTo(frame.data, RemoteServer);
            await Task.Run(() =>
            {
                Thread.Sleep(Timeout);
            });
        }

        protected abstract void ResolveQuestion(DNSFrame frame, EndPoint sender);

        protected abstract void ResolveResponse(DNSFrame frame);
    }
    
}