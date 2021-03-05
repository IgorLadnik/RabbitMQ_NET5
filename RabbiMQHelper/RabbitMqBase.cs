using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RabbiMQHelper
{
    public class RabbitMqOptions 
    {
        public string Exchange { get; set; }
        public string ExchangeType { get; set; } = "direct";
        public string Queue { get; set; }
        public string RoutingKey { get; set; }
        public bool AutoAck { get; set; } = false;
        public bool Durable { get; set; } = true;
        public bool Exclusive { get; set; } = false;
        public bool AutoDelete { get; set; } = false;
        public IDictionary<string, object> Arguments { get; set; }
    }

    public class RabbitMqBase : IDisposable
    {
        protected IModel _channel;
        protected RabbitMqOptions _options;

        private ConnectionFactory _factory;
        private IConnection _connection;

        protected RabbitMqBase(ConnectionFactory factory, RabbitMqOptions options)
        {
            _factory = factory;
            _options = options;
        }

        protected Task<RabbitMqBase> Connect() =>
            Task.Run(() =>
            {
                try
                {
                    _connection = _factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _channel.ExchangeDeclare(
                        exchange: _options.Exchange,
                        type: _options.ExchangeType,
                        durable: _options.Durable,
                        autoDelete: _options.AutoDelete);

                    _channel.QueueDeclare(
                        queue: _options.Queue,
                        durable: _options.Durable,
                        exclusive: _options.Exclusive,
                        autoDelete: _options.AutoDelete,
                        arguments: _options.Arguments);

                    _channel.QueueBind(
                        queue: _options.Queue,
                        exchange: _options.Exchange,
                        routingKey: _options.RoutingKey);

                    _channel.BasicQos(0, 1, false);
                }
                catch (Exception e)
                {
                }

                return this;
            });

        public void Dispose()
        {
            try
            {
                _channel?.Dispose();
                _channel = null;

                _connection?.Dispose();
                _connection = null;
            }
            catch (Exception ex)
            {
            }
        }
    }
}
