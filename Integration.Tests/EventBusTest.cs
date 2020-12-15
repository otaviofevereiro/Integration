using Integration.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Integration.Tests
{
    public class EventBusTest
    {
        private static TestEvent @event;

        [Fact]
        public void Test1()
        {
            var serviceProvider = GetServiceProvider();
            var eventBus = serviceProvider.GetService<TestEventBus>();
            var testEvent = new TestEvent();

            eventBus.Subscribe<TestEvent, TestEventHandler>().GetAwaiter().GetResult();
            eventBus.Publish(nameof(TestEvent), testEvent).GetAwaiter().GetResult();

            Assert.NotNull(@event);
            Assert.Equal(@event.EventId, testEvent.EventId);
            Assert.Equal(@event.CreationDate, testEvent.CreationDate);
        }


        protected IServiceProvider GetServiceProvider()
        {
            var services = new ServiceCollection();
            //services.AddEventBus();
            services.AddTransient<TestEventBus>();
            services.AddTransient<TestEventHandler>();

            return services.BuildServiceProvider();
        }

        private class TestEventHandler : IEventHandler<TestEvent>
        {

            public Task Handle(TestEvent @event)
            {
                EventBusTest.@event = @event;

                return Task.CompletedTask;
            }
        }

        private class TestEvent : Event
        {

        }

        private class TestEventBus : EventBus
        {
            public TestEventBus(SubscriberManager subscriber, IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(subscriber, serviceProvider, loggerFactory)
            {
            }

            public override async Task Publish(string eventName, object @event, CancellationToken cancellationToken = default)
            {
                var messageByte = JsonSerializer.SerializeToUtf8Bytes(@event);

                await Notify(eventName, messageByte);
            }

            protected override Task DoSubscribe(string eventName, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}
