using System.Text;
using System.Text.Json;
using Consumer.Api.Domain.Services;
using Consumer.Api.Domain.Services.Models;
using Consumer.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Events;

namespace Consumer.Api.Infrastructure.Consumer;

public sealed class ResumeEventConsumer(ResultStore resultStore, ILogger<ResumeEventConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE") ?? "resume.events";
        var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq";
        var retryCount = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory { HostName = rabbitHost };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(
                    queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (_, ea) => ProcessMessage(ea.Body.ToArray());

                channel.BasicConsume(
                    queue: queueName,
                    autoAck: true,
                    consumer: consumer);

                logger.LogInformation("Listening for events on queue {Queue}", queueName);
                retryCount = 0;

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (BrokerUnreachableException)
            {
                retryCount++;
                logger.LogWarning(
                    "RabbitMQ at {Host} is not ready yet. Retry {RetryCount} in 3 seconds.",
                    rabbitHost,
                    retryCount);
                await Task.Delay(3000, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to connect to RabbitMQ at {Host}. Retrying in 3 seconds.", rabbitHost);
                await Task.Delay(3000, stoppingToken);
            }
        }
    }

    private void ProcessMessage(byte[] body)
    {
        var json = Encoding.UTF8.GetString(body);

        var cloudEvent = JsonSerializer.Deserialize<CloudEventEnvelope>(json);
        if (cloudEvent?.Data is null)
        {
            return;
        }

        var detectedSkills = SkillDetector.Detect(cloudEvent.Data.ResumeText);

        var processedByPod = Environment.GetEnvironmentVariable("HOSTNAME") ?? "local-consumer";

        var result = new ResumeResult
        {
            EventId = cloudEvent.Id,
            CandidateName = cloudEvent.Data.CandidateName,
            DetectedSkills = detectedSkills,
            ProcessedAt = DateTimeOffset.UtcNow,
            ProcessedByPod = processedByPod
        };

        resultStore.Save(result);
        logger.LogInformation("Processed event {EventId} with {SkillCount} skill matches", result.EventId, detectedSkills.Length);
    }
}


