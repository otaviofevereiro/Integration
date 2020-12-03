using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration
{
    public static class Service
    {
        public static void AddEventBus(this IServiceCollection services)
        {
            services.AddTransient<Subscriber>();
        }
    }
}
