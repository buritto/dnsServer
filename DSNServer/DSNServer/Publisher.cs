using System.Collections.Generic;
using System.Net;

namespace DSNServer
{
    public interface IPublisher
    {
        List<ISubscriber> MySubscribers { get; set; }
        void Notify(DNSFrame frame, EndPoint client);
        void Subscribe(ISubscriber subscriber);
        void Unsubscribe(ISubscriber subscriber);
    }
}