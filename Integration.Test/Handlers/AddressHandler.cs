using Integration.Core;
using Integration.Test.Events;
using System.Threading.Tasks;

namespace Integration.Test.Handlers
{
    public class AddressHandler : IEventHandler<AddressEvent>
    {
        public Task Handle(EventContext<AddressEvent> eventContext)
        {
            return Task.CompletedTask;
        }
    }
}
