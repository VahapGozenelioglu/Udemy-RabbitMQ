﻿using RabbitMQ.Client;
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
    private const string ExchangeName = "header-exchange";

    static async Task Main()
    {
        var factory = CreateFactory();

        await using var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        var queueName = await SetupQueueAsync(channel);

        Console.WriteLine("Waiting for messages...");
        await StartConsumerAsync(channel, queueName);

        Console.ReadLine();
    }

    private static ConnectionFactory CreateFactory()
    {
        return new ConnectionFactory
        {
            Uri = new Uri("amqps://pprkqvbg:nrvsmOLyMLntI-KS-fmCvlGQUpLq6Pov@rattlesnake.rmq.cloudamqp.com/pprkqvbg")
        };
    }

    private static async Task<string> SetupQueueAsync(IChannel channel)
    {
        var queueResponse = await channel.QueueDeclareAsync();
        var queueName = queueResponse.QueueName;
        var headers = GetHeaders();

        await channel.QueueBindAsync(queueName, ExchangeName, string.Empty, headers!);
        await channel.BasicQosAsync(0, 1, false);

        Console.WriteLine($"Queue '{queueName}' is bound with headers: {string.Join(", ", headers)}");
        return queueName;
    }

    private static async Task StartConsumerAsync(IChannel channel, string queueName)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        await channel.BasicConsumeAsync(queueName, false, consumer);

        consumer.ReceivedAsync += async (_, e) =>
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"[Received] {message}");

            await channel.BasicAckAsync(e.DeliveryTag, false);
        };
    }

    private static Dictionary<string, object> GetHeaders()
    {
        return new Dictionary<string, object>
        {
            { "format", "pdf" },
            { "shape", "a4" },
            { "x-match", "any" }
        };
    }
}

