using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbiMQHelper;
using RabbitMQ.Client;

namespace RabbitSample
{
    [Serializable]
    public class Order
    { 
        public string Id { get; set; }
    }
    
    [Serializable]
    public class Customer
    {
        public List<Order> Orders;

        public string Name { get; set; }
    }

    class Program
    {
        static byte[] Serialize(object ob) 
        {
            byte[] data;
            var formatter = new BinaryFormatter();
            using var ms = new MemoryStream();
            formatter.Serialize(ms, ob);
            return ms.ToArray();   
        }

        static T Deserialize<T>(byte[] data)
        {
            var formatter = new BinaryFormatter();
            using var ms = new MemoryStream();
            ms.Write(data, 0, data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(ms);
        }

        static async Task Main(string[] args)
        {
            Customer customer = new()
            {
                Name = "The Customer",
                Orders = new()
                {
                    new() { Id = "order1" },
                    new() { Id = "order2" },
                }
            };


            ConnectionFactory factory = new()
            {
                //Uri = "amqp://user:pass@hostName:port/vhost";
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                RequestedHeartbeat = TimeSpan.FromSeconds(15),
            };

            const string exchange = "test_exchange";
            const string queuePrefix = "test_queue_";
            const string routingKey = "test.message";

            using var sub1 = (await RabbitMqSubscriber.CreateAsync(factory, new() { Exchange = exchange, Queue = queuePrefix + "1", RoutingKey = routingKey }))
                .Subscribe((bytes, isRedelivered) =>
                {
                    string message;
                    Customer customer;
                    if (bytes.Length < 20)
                    {
                        message = Encoding.UTF8.GetString(bytes);
                        Console.WriteLine($"1 -> {message}");
                    }
                    else
                    {
                        try
                        {
                            customer = Deserialize<Customer>(bytes);
                            Console.WriteLine($"1 -> {customer.Name}");
                        }
                        catch (Exception e)
                        {
                        }
                    }
                });

            using var sub2 = (await RabbitMqSubscriber.CreateAsync(factory, new() { Exchange = exchange, Queue = queuePrefix + "2", RoutingKey = routingKey }))
                .Subscribe((bytes, isRedelivered) => Handler(bytes, isRedelivered));

            using var pub = await RabbitMqPublisher.CreateAsync(factory, new() { Exchange = exchange, RoutingKey = routingKey });

            var count = 1;

            while (true) 
            {
                try
                {
                    pub.Publish(Encoding.UTF8.GetBytes($"Message {count}"));
                    pub.Publish(Serialize(customer));
                    count++;
                }
                catch (Exception e) 
                {
                }

                Thread.Sleep(5000);
            }

            //Console.ReadKey();
        }

        static void Handler(byte[] bytes, bool isRedelivered) 
        {
            string message;
            Customer customer;
            if (bytes.Length < 20)
            {
                message = Encoding.UTF8.GetString(bytes);
                Console.WriteLine($"2 -> {message}");
            }
            else
            {
                try
                {
                    customer = Deserialize<Customer>(bytes);
                    Console.WriteLine($"2 -> {customer.Name}");
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}
