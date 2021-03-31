using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Integration.Core
{
    public class EventContext<TEvent> : EventContext
    {
        private Lazy<TEvent> _eventLazy;

        public EventContext(string id, string eventName, ReadOnlyMemory<byte> @event, IDictionary<string, object> properties) : base(id, eventName, @event, properties)
        {
            _eventLazy = new Lazy<TEvent>(() => EventContextExtensions.Deserialize(this));
        }

        public TEvent Event => _eventLazy.Value;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                //if (disposing)
                //{
                //}

                _eventLazy = null;
            }
        }
    }


    public class EventContext : IDisposable
    {
        private ActionTypes _action;
        private bool _disposedValue;

        public EventContext(string id, string eventName, ReadOnlyMemory<byte> @event, IDictionary<string, object> properties)
        {
            RawEvent = @event;
            Properties = properties;
            Id = id;
            EventName = eventName;
        }

        public string EventName { get; }
        public bool DeadLetter => _action == ActionTypes.SendToDeadLetter;
        public string Id { get; }
        public bool Completed => _action == ActionTypes.Complete;
        public IDictionary<string, object> Properties { get; }
        public ReadOnlyMemory<byte> RawEvent { get; private set; }

        public void Complete()
        {
            _action = ActionTypes.Complete;
        }

        public void SendToDeadLetter()
        {
            _action = ActionTypes.SendToDeadLetter;
        }

        #region Disposeble
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                //if (disposing)
                //{
                //}

                RawEvent = null;
                _disposedValue = true;
            }
        }

        ~EventContext()
        {
            Dispose(disposing: false);
        }
        #endregion

    }
}
