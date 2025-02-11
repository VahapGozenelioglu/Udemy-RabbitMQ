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

        await channel.BasicPublishAsync(ExchangeName, String.Empty, false, properties, Encoding.UTF8.GetBytes("Message with headers"));
            
        // await DeclareAndBindQueues(channel);
        // await SendMessage(channel);
    }

    private static ConnectionFactory CreateFactory()
    {
        var factory = new ConnectionFactory();
        factory.Uri =
            new Uri("amqps://pprkqvbg:nrvsmOLyMLntI-KS-fmCvlGQUpLq6Pov@rattlesnake.rmq.cloudamqp.com/pprkqvbg");
        return factory;
    }
    
    // For topic exchange this operations will be done in subscriber side
    // private static async Task DeclareAndBindQueues(IChannel channel)
    // {
    //     foreach (var logType in Enum.GetNames(typeof(LogTypes)))
    //     {
    //         var queueName = $"direct-queue-{logType}";
    //         var routingKey = $"route-{logType}";
    //     
    //         await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
    //         await channel.QueueBindAsync(queueName, ExchangeName, routingKey);
    //     }
    // }

    
    // For Headers exhange we dont need this method
    // private static async Task SendMessage(IChannel channel)
    // {
    //     var random = new Random();
    //
    //     for (int x = 1; x <= 60; x++)
    //     {
    //         var logType1 = (LogTypes)random.Next(1, 5);
    //         var logType2 = (LogTypes)random.Next(1, 5);
    //         var logType3 = (LogTypes)random.Next(1, 5);
    //         
    //         var message = $"Log type: {logType1}-{logType2}-{logType3}";
    //         var routeKey = $"{logType1}.{logType2}.{logType3}";
    //         
    //         var messageBody = Encoding.UTF8.GetBytes(message);
    //
    //         await channel.BasicPublishAsync(ExchangeName, routeKey, false, messageBody);
    //         Console.WriteLine($"Log sent {logType1}-{logType2}-{logType3}({x})");
    //     
    //         await Task.Delay(100);
    //     }
    // }

}