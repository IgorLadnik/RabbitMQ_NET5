using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbiMQHelper
{
    public class RabbitMqSubscriber : RabbitMqBase
    {
        private EventingBasicConsumer _consumer;
        private Action<byte[], bool> _action;

        public static async Task<RabbitMqSubscriber> CreateAsync(ConnectionFactory factory, RabbitMqOptions options) => 
            (await new RabbitMqSubscriber(factory, options).Connect()) as RabbitMqSubscriber; 

        private RabbitMqSubscriber(ConnectionFactory factory, RabbitMqOptions options)
            : base(factory, options)
        {
        }

        public RabbitMqSubscriber Subscribe(Action<byte[], bool> action) 
        {
            if (action == null)
                throw new Exception("Error: null Action");

            if (!_isReconnect && _consumer != null)
                throw new Exception("Error: already subscribed");

            _action = action;
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (sender, ea) =>
                {
                    try
                    {
                        _action(ea.Body.ToArray(), ea.Redelivered);
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception e)
                    {
                        _channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true);
                    }
                };
            _channel.BasicConsume(queue: _options.Queue,
                                  autoAck: _options.AutoAck,
                                  consumer: _consumer);
            return this;
        }

        protected override void OnReconnect() => Subscribe(_action);
    }
}


