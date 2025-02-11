// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;

namespace UdemyRabbitMQ.publisher;

public enum LogTypes
{
    Critical = 1,
    Error = 2,
    Warning = 3,
    Info = 4
}

class Program
{
    private const string ExchangeName = "logs-direct";
    static async Task Main(string[] args)
    {
        var factory = CreateFactory();

        await using var connection = await factory.CreateConnectionAsync();

        var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(ExchangeName, durable: true, type: ExchangeType.Direct); 
        
        await DeclareAndBindQueues(channel);
        await SendMessage(channel);
    }

    private static async Task DeclareAndBindQueues(IChannel channel)
    {
        foreach (var logType in Enum.GetNames(typeof(LogTypes)))
        {
            var queueName = $"direct-queue-{logType}";
            var routingKey = $"route-{logType}";
        
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
            await channel.QueueBindAsync(queueName, ExchangeName, routingKey);
        }
    }

    private static ConnectionFactory CreateFactory()
    {
        var factory = new ConnectionFactory();
        factory.Uri =
            new Uri("amqps://pprkqvbg:nrvsmOLyMLntI-KS-fmCvlGQUpLq6Pov@rattlesnake.rmq.cloudamqp.com/pprkqvbg");
        return factory;
    }

    private static async Task SendMessage(IChannel channel)
    {
        var random = new Random();
    
        for (int x = 1; x <= 60; x++)
        {
            var logType = (LogTypes)random.Next(1, 5);
            var message = $"Log type: {logType}";
            var routeKey = $"route-{logType}";
            var messageBody = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(ExchangeName, routeKey, false, messageBody);
            Console.WriteLine($"Log sent {logType}({x})");
        
            await Task.Delay(100); // Mesajlar çok hızlı gitmesin
        }
    }

}