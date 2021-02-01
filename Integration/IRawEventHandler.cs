using System.Threading.Tasks;

namespace Integration.Core
{
    public interface IRawEventHandler<in TEvent>
    {
        Task Handle(TEvent @event, IEventContext eventContext);
    }
}
