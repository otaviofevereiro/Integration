using System.Threading.Tasks;

namespace Integration.Core
{
    public interface IEventHandler<in TEvent> 
        where TEvent : Event
    {
        Task Handle(TEvent @event);
    }
}
