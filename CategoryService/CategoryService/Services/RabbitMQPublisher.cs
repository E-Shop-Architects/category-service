using Domain.Entities;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CategoryService.Services
{
    public class RabbitMQPublisher
    {
        private readonly IConfiguration _config;
        public RabbitMQPublisher(IConfiguration config) // ← DI container bunu verebilir
        {
            _config = config;
        }
        public void Publish(Category category) 
        {
            var factory = new RabbitMQ.Client.ConnectionFactory() { HostName = _config["RabbitMQ:Host"] ?? "localhost" };


            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: "orders",
                durable: false,
                exclusive: false,
                autoDelete: false
            );

            var json = JsonSerializer.Serialize(category);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(
                exchange: "",
                routingKey: "orders",
                body: body
            );
        }
    }
}
