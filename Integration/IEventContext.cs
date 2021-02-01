using System;

namespace Integration.Core
{
    public interface IEventContext
    {
        public ReadOnlyMemory<byte> Event { get; }
        public string EventName { get;  }

        public void Complete();
        public void SendToDeadLetter();
    }
}
