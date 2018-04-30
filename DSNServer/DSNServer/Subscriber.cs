using System.Net;

namespace DSNServer
{
    public interface ISubscriber
    {
        void Updata(DNSFrame frame, EndPoint sender);
    }
}