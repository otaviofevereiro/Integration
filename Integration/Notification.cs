namespace Integration.Core
{
    public class Notification<TEvent>
         where TEvent : Event
    {
        public TEvent Event { get; init; }
    }
}
