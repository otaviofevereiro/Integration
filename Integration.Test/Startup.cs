using Integration.RabbitMq;
using Integration.Test.Events;
using Integration.Test.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Mime;

namespace Integration.Test
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddEventBus("Amqp", builder => builder.AddEventHandler<PersonEvent, PersistPersonHandler>()
                                                           .AddEventHandler<PersonEvent, PersonHandler>()
                                                           .AddAsHostedService());

            //services.AddEventBus("Amqp2", builder => builder.AddEventHandler<AddressEvent, AddressHandler>());

            services.AddHealthChecks()
                    .AddCheck<RabbitMQHealthCheck>("Amqp");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseHealthChecks("/healtcheck/readiness")
               .UseHealthChecks("/healtcheck/liveness");

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
