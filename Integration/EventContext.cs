using Integration.Core;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Integration.Core
{
    public class EventContext : IEventContext, IDisposable
    {
        public ReadOnlyMemory<byte> Event { get; private set; }
        public string EventName { get; }

        private readonly BlockingCollection<ActionTypes> actions = new BlockingCollection<ActionTypes>();
        private bool disposedValue;

        public bool IsAllComplete => !actions.Any() || actions.All(x => x == ActionTypes.Complete);
        public bool HasDeadLetter => actions.Any(x => x == ActionTypes.SendToDeadLetter);

        public EventContext(string eventName, ReadOnlyMemory<byte> @event)
        {
            Event = @event;
            EventName = eventName;
        }

        public void Complete()
        {
            actions.Add(ActionTypes.Complete);
        }

        public void SendToDeadLetter()
        {
            actions.Add(ActionTypes.SendToDeadLetter);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    actions.Dispose();
                }

                Event = null;
                disposedValue = true;
            }
        }

        ~EventContext()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
