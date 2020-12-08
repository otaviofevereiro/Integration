using Integration.Core;

namespace Integration.Extensions
{
    public interface IEventBusFactory
    {
        IConfigurableEventBus Create(string configurationName);
    }
}