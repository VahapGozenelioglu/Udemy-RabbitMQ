// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;

namespace UdemyRabbitMQ.publisher;

class Program
{
    static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory();
        factory.Uri =
            new Uri("amqps://pprkqvbg:nrvsmOLyMLntI-KS-fmCvlGQUpLq6Pov@rattlesnake.rmq.cloudamqp.com/pprkqvbg");

        using var coonection = await factory.CreateConnectionAsync();

        var channel = await coonection.CreateChannelAsync();

        await channel.QueueDeclareAsync("hello-queue", true, false, false);

        Enumerable.Range(1, 60).ToList().ForEach(x =>
        {
            string message = $"Message:{x}";
            var messageBody = Encoding.UTF8.GetBytes(message);
            channel.BasicPublishAsync(string.Empty, "hello-queue", false, messageBody);  
            Console.WriteLine("Message sent"); 
        });

    }
}