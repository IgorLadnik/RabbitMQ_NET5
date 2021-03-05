using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RabbiMQHelper
{
    public class RabbitMqPublisher : RabbitMqBase
    {
        public static async Task<RabbitMqPublisher> CreateAsync(ConnectionFactory factory, RabbitMqOptions options) =>
            (await new RabbitMqPublisher(factory, options).Connect()) as RabbitMqPublisher;

        private RabbitMqPublisher(ConnectionFactory factory, RabbitMqOptions options)
            : base(factory, options)
        {
        }

        public void Publish(byte[] bytes) 
        {
            if (_channel == null)
                return;

            var properties = _channel.CreateBasicProperties();
            //properties.AppId = "AppId";
            //properties.ContentType = "application/json";
            properties.DeliveryMode = 1; // Doesn't persist to disk
            properties.Timestamp = new(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(exchange: _options.Exchange,
                                 routingKey: _options.RoutingKey,
                                 basicProperties: properties,
                                 body: bytes);
        }
    }
}
