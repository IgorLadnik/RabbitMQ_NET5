using System.Collections.Generic;

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
}
