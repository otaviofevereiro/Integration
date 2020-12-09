using Integration.Core;
using Integration.Test.Events;
using System.Threading.Tasks;

namespace Integration.Test.Handlers
{
    public class PersistPersonHandler : IEventHandler<PersonEvent>
    {
        public Task Handle(PersonEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}
