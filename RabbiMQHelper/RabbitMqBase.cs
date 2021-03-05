using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RabbiMQHelper
{
    public class RabbitMqBase : IDisposable
    {
        protected readonly RabbitMqOptions _options;
        protected IModel _channel;

        private readonly ConnectionFactory _factory;
        private IConnection _connection;

        protected bool _isReconnect = false;
        
        protected RabbitMqBase(ConnectionFactory factory, RabbitMqOptions options)
        {
            _factory = factory;
            _options = options;
        }

        protected Task<RabbitMqBase> Connect() =>
            Task.Run(async () =>
            {
                try
                {
                    _connection = _factory.CreateConnection();
                    _connection.ConnectionShutdown += async (sender, ea) => await Reconnect();

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
                    await Reconnect();
                }

                return this;
            });

        private async Task Reconnect()
        {
            if (_isReconnect) 
                return;

            _isReconnect = true;

            Dispose();

            var ev = new ManualResetEventSlim(false);

            while (!ev.Wait(_factory.RequestedHeartbeat))
            {
                try
                {
                    await Connect();
                    if (IsOK)
                    {
                        ev.Set();
                        OnReconnect();
                        _isReconnect = false;
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Reconnect failed!");
                }
            }
        }

        protected virtual void OnReconnect() { }

        private bool IsOK => (bool)_channel?.IsOpen && (bool)_connection?.IsOpen;

        public void Dispose()
        {
            try
            {
                if ((bool)_channel?.IsOpen)
                {
                    _channel?.Close();
                    _channel = null;
                }

                if ((bool)_connection?.IsOpen)
                {
                    _connection?.Close();
                    _connection = null;
                }
            }

            catch (Exception ex)
            {
                // Close() may throw an IOException if connection
                // dies - but that's ok (handled by reconnect)
            }
        }
    }
}
