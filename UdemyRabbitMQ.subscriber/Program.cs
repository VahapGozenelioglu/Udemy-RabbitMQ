using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace UdemyRabbitMQ.subscriber;
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

        await StartConsumer(channel);
    }

    private static ConnectionFactory CreateFactory()
    {
        return new ConnectionFactory
        {
            Uri = new Uri("amqps://pprkqvbg:nrvsmOLyMLntI-KS-fmCvlGQUpLq6Pov@rattlesnake.rmq.cloudamqp.com/pprkqvbg")
        };
    }

    private static async Task StartConsumer(IChannel channel)
    {
        var logType = (LogTypes)new Random().Next(1, 5);
        var queueName = $"direct-queue-{logType}";

        await channel.BasicQosAsync(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        await channel.BasicConsumeAsync(queueName, false, consumer);

        Console.WriteLine($"Waiting for logs with type: {logType} ...");

        consumer.ReceivedAsync += async (sender, e) =>
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"Received: {message}");

            await SaveLogToFileAsync(logType, message);
            await channel.BasicAckAsync(e.DeliveryTag, false);
        };

        Console.ReadLine();
    }

    private static async Task SaveLogToFileAsync(LogTypes logType, string message)
    {
        var filePath = $"log-{logType.ToString().ToLower()}.txt";
        await File.AppendAllTextAsync(filePath, message + Environment.NewLine);
    }
}
