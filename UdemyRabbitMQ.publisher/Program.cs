// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;

namespace UdemyRabbitMQ.publisher;

class Program
{
    private const string ExchangeName = "logs-fanout";
    static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory();
        factory.Uri =
            new Uri("amqps://pprkqvbg:nrvsmOLyMLntI-KS-fmCvlGQUpLq6Pov@rattlesnake.rmq.cloudamqp.com/pprkqvbg");

        using var coonection = await factory.CreateConnectionAsync();

        var channel = await coonection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(ExchangeName, durable: true, type: ExchangeType.Fanout); 

        Enumerable.Range(1, 60).ToList().ForEach(x =>
        {
            string message = $"Log:{x}";
            var messageBody = Encoding.UTF8.GetBytes(message);
            channel.BasicPublishAsync(ExchangeName, "", false, messageBody);  
            Console.WriteLine($"Message{x} sent"); 
        });

    }
}