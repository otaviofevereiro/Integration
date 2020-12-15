using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;

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

        public void Connect()
        {
            var section = _configuration.GetSection(Name);

            section.Bind(_connectionFactory);

            _connection = _connectionFactory.CreateConnection();
            _model = _connection.CreateModel();
        }

        public void Close()
        {
            if (_connection.IsOpen)
                _connection.Close();

            if (_model.IsOpen)
                _model.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                    _connection.Dispose();
                    _model.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
