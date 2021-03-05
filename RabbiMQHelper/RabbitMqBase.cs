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
        protected RabbitMqOptions Options { get; private set; }
        protected IModel Channel { get; private set; }
        protected IConnection Connection { get; private set; }

        protected ConnectionFactory Factory { get; private set; }

        protected RabbitMqBase(ConnectionFactory factory, RabbitMqOptions options)
        {
            Factory = factory;
            Options = options;
        }

        protected Task<RabbitMqBase> Connect() =>
            Task.Run(() =>
            {
                try
                {
                    Connection = Factory.CreateConnection();
                    Channel = Connection.CreateModel();

                    Channel.ExchangeDeclare(
                        exchange: Options.Exchange,
                        type: Options.ExchangeType,
                        durable: Options.Durable,
                        autoDelete: Options.AutoDelete);

                    Channel.QueueDeclare(
                        queue: Options.Queue,
                        durable: Options.Durable,
                        exclusive: Options.Exclusive,
                        autoDelete: Options.AutoDelete,
                        arguments: Options.Arguments);

                    Channel.QueueBind(
                        queue: Options.Queue,
                        exchange: Options.Exchange,
                        routingKey: Options.RoutingKey);

                    Channel.BasicQos(0, 1, false);
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
                Channel?.Dispose();
                Channel = null;

                Connection?.Dispose();
                Connection = null;
            }
            catch (Exception ex)
            {
            }
        }
    }
}
