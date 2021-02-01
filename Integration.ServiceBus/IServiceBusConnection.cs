using Integration.Core;
using Microsoft.Azure.ServiceBus.Core;

namespace Integration.ServiceBus
{
    public interface IServiceBusConnection : IEventBusConnection
    {
        IReceiverClient GetReceiverClient(string eventName);
        ISenderClient GetSenderClient(string eventName);
    }
}