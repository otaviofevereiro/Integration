using System;
using System.Threading.Tasks;

namespace Integration.Core
{
    public interface IConfigurableEventBus : IEventBus, IDisposable
    {
        public string Name { get; }
        public Task Initialize();
    }
}
