using System.Threading.Tasks;

namespace Integration.Core
{
    public interface IEventHandler { }

    public interface IEventHandler<in TEvent> : IEventHandler
        where TEvent : Event
    {
        Task Handle(TEvent @event);
    }
}
