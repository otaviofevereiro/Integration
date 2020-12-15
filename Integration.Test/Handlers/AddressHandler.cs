using Integration.Core;
using Integration.Test.Events;
using System.Threading.Tasks;

namespace Integration.Test.Handlers
{
    public class AddressHandler : IEventHandler<AddressEvent>
    {
        public Task Handle(AddressEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}
