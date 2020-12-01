using System.Threading.Tasks;

namespace Integration
{
    public interface IEventHandler<in TEvent>
        where TEvent : Event
    {
        Task Handle(TEvent @event);
    }
}
