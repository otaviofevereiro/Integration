using Integration.Core;
using Integration.Test.Events;
using System.Threading.Tasks;

namespace Integration.Test.Handlers
{
    public class PersonHandler : IEventHandler<PersonEvent>
    {
        public Task Handle(PersonEvent @event, IEventContext eventContext)
        {
            return Task.CompletedTask;
        }
    }
}
