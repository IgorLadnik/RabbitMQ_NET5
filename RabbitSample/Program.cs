using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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
                Name = "1st Customer",
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
            };

            RabbitMqOptions options = new()
            {
                Exchange = "test_exchange",
                Queue = "test_queue_1",
                RoutingKey = "test.message",
            };

            using var sub1 = (await RabbitMqSubscriber.CreateAsync(factory, options)).Subscribe(a =>
            {
                string message;
                Customer customer;
                var data = a.ToArray();
                if (data.Length < 20)
                {
                    message = Encoding.UTF8.GetString(a.ToArray());
                    Console.WriteLine($"1 -> {message}");
                }
                else
                {
                    try
                    {
                        customer = Deserialize<Customer>(a.ToArray());
                        Console.WriteLine($"1 -> {customer.Name}");
                    }
                    catch (Exception e)
                    {
                    }
                }
            });

            options.Queue = "test_queue_2";
            using var sub2 = (await RabbitMqSubscriber.CreateAsync(factory, options)).Subscribe(a => Handler(a));

            using var pub = await RabbitMqPublisher.CreateAsync(factory, options);

            var count = 0;
            pub.Publish(Encoding.UTF8.GetBytes($"Message {++count}"));
            pub.Publish(Serialize(customer));           

            Console.ReadKey();
        }

        static void Handler(ReadOnlyMemory<byte> a) 
        {
            string message;
            Customer customer;
            var data = a.ToArray();
            if (data.Length < 20)
            {
                message = Encoding.UTF8.GetString(a.ToArray());
                Console.WriteLine($"2 -> {message}");
            }
            else
            {
                try
                {
                    customer = Deserialize<Customer>(a.ToArray());
                    Console.WriteLine($"2 -> {customer.Name}");
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}
