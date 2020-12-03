using System;
using System.Text.Json.Serialization;

namespace Integration
{
    public class Event
    {
        [JsonConstructor]
        public Event()
        {
            EventId = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        [JsonConstructor]
        public Event(Guid eventId, DateTime createDate)
        {
            EventId = eventId;
            CreationDate = createDate;
        }

        public Guid EventId { get; private set; }

        public DateTime CreationDate { get; private set; }
    }
}
