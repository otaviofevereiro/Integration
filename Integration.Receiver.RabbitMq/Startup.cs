using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.RabbitMq
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly Dictionary<string, ConnectionFactory> rabbitConfigurations = new Dictionary<string, ConnectionFactory>();

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void AddRabbit(params string[] rabbitConfigurationsSectionsNames)
        {
            foreach (var sectionName in rabbitConfigurationsSectionsNames)
            {
                //TODO: converter para connection factory
                var rabbitConfiguration = configuration.GetSection(sectionName);

                var connectionFactory = new ConnectionFactory();

                rabbitConfigurations.Add(sectionName, rabbitConfiguration);
            }
        }

        public class Provider<THeathCheck> 
        { 

        }

    }
}
