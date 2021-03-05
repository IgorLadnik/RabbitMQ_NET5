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

        public static async Task<RabbitMqSubscriber> Create(ConnectionFactory factory, RabbitMqOptions options) => 
            (await new RabbitMqSubscriber(factory, options).Connect()) as RabbitMqSubscriber; 

        private RabbitMqSubscriber(ConnectionFactory factory, RabbitMqOptions options)
            : base(factory, options)
        {
        }

        public void Subscribe(Action<ReadOnlyMemory<byte>> action) 
        {
            if (action == null)
                throw new Exception("Null Action");

            _action = action;
            _consumer = new EventingBasicConsumer(Channel);
            Channel.BasicConsume(queue: Options.Queue,
                                 autoAck: Options.AutoAck,
                                 consumer: _consumer);
            _consumer.Received += Consumer_Received;
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                _action(ea.Body);

                var props = ea.BasicProperties;
                var replyProps = Channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
                Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception e) 
            {
            }
        }
    }
}


