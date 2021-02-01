using System;
using System.Threading.Tasks;

namespace Integration.Core
{
    public interface IEventBusConnection : IDisposable
    {
        string Name { get; }

        Task Connect();
        Task Close();
    }
}