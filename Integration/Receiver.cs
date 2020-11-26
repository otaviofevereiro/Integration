using System;
using System.Threading.Tasks;

namespace Integration
{
    public abstract class Receiver<TConfiguration>
        where TConfiguration : IReceiverConfiguration
    {
        protected abstract void Configure(TConfiguration configuration);

        protected Task OnReceived(dynamic data)
        {
            throw new NotImplementedException();
        }
    }
}
