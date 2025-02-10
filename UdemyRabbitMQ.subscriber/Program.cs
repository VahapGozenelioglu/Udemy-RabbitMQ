using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace UdemyRabbitMQ.subscriber;

class Program
{
    private const string ExchangeName = "logs-fanout";

    static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory();
        factory.Uri =
            new Uri("amqps://pprkqvbg:nrvsmOLyMLntI-KS-fmCvlGQUpLq6Pov@rattlesnake.rmq.cloudamqp.com/pprkqvbg");

        await using var coonection = await factory.CreateConnectionAsync();

        var channel = await coonection.CreateChannelAsync();

        var randomQueueName = channel.QueueDeclareAsync().Result.QueueName;
        await channel.QueueBindAsync(randomQueueName, ExchangeName, "");
        await channel.BasicQosAsync(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        await channel.BasicConsumeAsync(randomQueueName, false, consumer);

        Console.WriteLine("Waiting for messages...");
        
        consumer.ReceivedAsync += (object sender, BasicDeliverEventArgs e) =>
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            Thread.Sleep(2000);
            Console.WriteLine($"Message: {message}");

            channel.BasicAckAsync(e.DeliveryTag, false);
            return Task.CompletedTask;
        };

        Console.ReadLine();
    }
}