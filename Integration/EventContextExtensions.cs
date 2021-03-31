using Newtonsoft.Json;
using System;
using System.IO;

namespace Integration.Core
{
    public static class EventContextExtensions
    {
        //public static object Deserialize(this IEventContext<T> eventContext, Type type)
        //{
        //    using (var ms = new MemoryStream(eventContext.RawEvent.ToArray()))
        //    using (var sr = new StreamReader(ms))
        //    {
        //        string eventJson = sr.ReadToEnd();

        //        return JsonConvert.DeserializeObject(eventJson, type);
        //    }
        //}
        public static T Deserialize<T>(this EventContext<T> eventContext)
        {
            using (var ms = new MemoryStream(eventContext.RawEvent.ToArray()))
            using (var sr = new StreamReader(ms))
            {
                string eventJson = sr.ReadToEnd();

                return JsonConvert.DeserializeObject<T>(eventJson);
            }
        }
    }
}


