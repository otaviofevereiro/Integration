using Integration.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integration.Test.Handlers
{
    public class CustomHandler : IEventHandler<CustomEvent>
    {
        public Task Handle(CustomEvent @event)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomEvent : Event
    { }
}
