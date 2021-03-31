using System.Threading.Tasks;

namespace Integration.Core
{
    public interface IEventHandler<TEvent>
        where TEvent : Event
    {
        Task Handle(EventContext<TEvent> eventContext);
    }
}
