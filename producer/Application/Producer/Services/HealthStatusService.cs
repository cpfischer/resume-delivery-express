using System.Net;
using System.Net.Sockets;
using Application.Producer.Contracts;

namespace Application.Producer.Services;

public sealed class HealthStatusService : IHealthStatusService
{
    public async Task<HealthStatusResponse> GetStatusAsync(CancellationToken cancellationToken)
    {
        var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var rabbitPortRaw = Environment.GetEnvironmentVariable("RABBITMQ_PORT");
        var rabbitPort = int.TryParse(rabbitPortRaw, out var parsedRabbitPort) ? parsedRabbitPort : 5672;

        var consumerHost = Environment.GetEnvironmentVariable("CONSUMER_RESULTS_HOST") ?? "localhost";
        var consumerPortRaw = Environment.GetEnvironmentVariable("CONSUMER_RESULTS_PORT");
        var consumerPort = int.TryParse(consumerPortRaw, out var parsedConsumerPort) ? parsedConsumerPort : 5002;

        var rabbitMqReachable = await IsTcpEndpointReachableAsync(rabbitHost, rabbitPort, 800, cancellationToken);
        var consumerReachable = await IsTcpEndpointReachableAsync(consumerHost, consumerPort, 800, cancellationToken);

        var allConnectionsWorking = rabbitMqReachable && consumerReachable;

        return new HealthStatusResponse(
            allConnectionsWorking ? "healthy" : "degraded",
            allConnectionsWorking,
            new HealthChecksResponse(
                "reachable",
                rabbitMqReachable ? "reachable" : "unreachable",
                consumerReachable ? "reachable" : "unreachable"),
            new HealthEndpointsResponse(
                $"{rabbitHost}:{rabbitPort}",
                $"{consumerHost}:{consumerPort}"));
    }

    private static async Task<bool> IsTcpEndpointReachableAsync(string host, int port, int timeoutMs, CancellationToken cancellationToken)
    {
        try
        {
            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(host, port, cancellationToken).AsTask();
            var timeoutTask = Task.Delay(timeoutMs, cancellationToken);
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask != connectTask)
            {
                return false;
            }

            await connectTask;
            return tcpClient.Connected;
        }
        catch
        {
            return false;
        }
    }
}
