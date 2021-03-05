using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbiMQHelper
{
    public class RabbitMqSubscriber : RabbitMqBase
    {
        private EventingBasicConsumer _consumer;
        private Action<ReadOnlyMemory<byte>> _action;

        public static async Task<RabbitMqSubscriber> CreateAsync(ConnectionFactory factory, RabbitMqOptions options) => 
            (await new RabbitMqSubscriber(factory, options).Connect()) as RabbitMqSubscriber; 

        private RabbitMqSubscriber(ConnectionFactory factory, RabbitMqOptions options)
            : base(factory, options)
        {
        }

        public RabbitMqSubscriber Subscribe(Action<ReadOnlyMemory<byte>> action) 
        {
            if (action == null)
                throw new Exception("Error: null Action");

            if (_consumer != null)
                throw new Exception("Error: already subscribed");

            _action = action;
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (sender, ea) =>
                {
                    try
                    {
                        _action(ea.Body);

                        var props = ea.BasicProperties;
                        var replyProps = _channel.CreateBasicProperties();
                        replyProps.CorrelationId = props.CorrelationId;
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception e)
                    {
                    }
                };
            _channel.BasicConsume(queue: _options.Queue,
                                  autoAck: _options.AutoAck,
                                  consumer: _consumer);
            return this;
        }
    }
}


