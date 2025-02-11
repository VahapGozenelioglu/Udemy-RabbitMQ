// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared;

namespace UdemyRabbitMQ.publisher;

class Program
{
    private const string ExchangeName = "header-exchange";
    static async Task Main(string[] args)
    {
        var factory = CreateFactory();

        await using var connection = await factory.CreateConnectionAsync();

        var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(ExchangeName, durable: true, type: ExchangeType.Headers);

        var properties = new BasicProperties()
        {
            Headers = new Dictionary<string, object>()
            {
                {"format", "pdf"},
                {"shape2", "a4"}
            }!,
            Persistent = true
        };

        var jsonProduct = GetJsonProduct();

        await channel.BasicPublishAsync(ExchangeName, String.Empty, false, properties, Encoding.UTF8.GetBytes(jsonProduct));
    }

    private static string GetJsonProduct()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Pencil",
            Price = 0.5m,
            Stock = 100
        };
        
        return JsonSerializer.Serialize(product);
    }

    private static ConnectionFactory CreateFactory()
    {
        var factory = new ConnectionFactory();
        factory.Uri =
            new Uri("amqps://pprkqvbg:nrvsmOLyMLntI-KS-fmCvlGQUpLq6Pov@rattlesnake.rmq.cloudamqp.com/pprkqvbg");
        return factory;
    }
}