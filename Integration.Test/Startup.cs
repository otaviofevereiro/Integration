using Integration.Test.Events;
using Integration.Test.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            //services.AddHealthChecks().AddCheck<HealthCheck>("");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

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
