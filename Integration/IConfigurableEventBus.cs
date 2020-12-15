using System;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.Core
{
    public interface IConfigurableEventBus : IPublisher, ISubscriber, IDisposable
    {
        public string Name { get; }
        public Task Start(CancellationToken cancellationToken);
        public Task Stop(CancellationToken cancellationToken);
    }
}
