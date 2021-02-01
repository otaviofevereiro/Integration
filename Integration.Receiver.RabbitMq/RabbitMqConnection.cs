using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace Integration.RabbitMq
{
    public class RabbitMqConnection : IRabbitMqConnection
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _model;
        private bool disposedValue;

        public RabbitMqConnection(string name, IConfiguration configuration)
        {
            Name = name;

            _configuration = configuration;
            _connectionFactory = new ConnectionFactory
            {
                DispatchConsumersAsync = true
            };
        }

        IModel IRabbitMqConnection.Model => _model;

        public string Name { get; }

        public Task Close()
        {
            if (_connection is not null && _connection.IsOpen)
                _connection.Close();

            if (_model is not null && _model.IsOpen)
                _model.Close();

            return Task.CompletedTask;
        }

        public Task Connect()
        {
            var section = _configuration.GetSection(Name);

            section.Bind(_connectionFactory);
            
            _connection = _connectionFactory.CreateConnection();
            _model = _connection.CreateModel();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                    _connection?.Dispose();
                    _model?.Dispose();
                }

                disposedValue = true;
            }
        }
    }
}
