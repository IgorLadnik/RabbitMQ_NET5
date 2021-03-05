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
            var properties = Channel.CreateBasicProperties();
            //properties.AppId = "AppId";
            //properties.ContentType = "application/json";
            properties.DeliveryMode = 1; // Doesn't persist to disk
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            Channel.BasicPublish(exchange: Options.Exchange,
                                 routingKey: Options.RoutingKey,
                                 basicProperties: properties,
                                 body: bytes);
        }
    }
}
