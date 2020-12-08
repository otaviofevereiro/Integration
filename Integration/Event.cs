using Newtonsoft.Json;
using System;

namespace Integration.Core
{
    public class Event
    {
        public Event()
        {
            EventId = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        [JsonConstructor]
        public Event(string eventId, DateTime creationDate)
        {
            EventId = new Guid(eventId);
            CreationDate = creationDate;
        }

        [JsonProperty]
        public Guid EventId { get; private set; }

        [JsonProperty]
        public DateTime CreationDate { get; private set; }
    }
}
