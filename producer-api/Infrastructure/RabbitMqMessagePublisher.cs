using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public interface IRabbitMqMessagePublisher
{
    void Publish(ProducerCloudEvent cloudEvent);
}

public sealed class RabbitMqMessagePublisher : IRabbitMqMessagePublisher
{
    public void Publish(ProducerCloudEvent cloudEvent)
    {
        var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var rabbitPortRaw = Environment.GetEnvironmentVariable("RABBITMQ_PORT");
        var rabbitPort = int.TryParse(rabbitPortRaw, out var parsedPort) ? parsedPort : 5672;
        var queueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE") ?? "resume.events";

        var factory = new ConnectionFactory
        {
            HostName = rabbitHost,
            Port = rabbitPort
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cloudEvent));

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: null,
            body: body);
    }
}
