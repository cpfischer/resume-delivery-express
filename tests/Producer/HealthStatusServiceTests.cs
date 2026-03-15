using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Application.Producer.Services;

[TestClass]
public sealed class HealthStatusServiceTests
{
    [TestMethod]
    public async Task GetStatusAsync_ReturnsHealthy_WhenRabbitMqAndConsumerPortsReachable()
    {
        using var rabbitListener = CreateListener();
        using var consumerListener = CreateListener();

        var rabbitPort = ((IPEndPoint)rabbitListener.LocalEndpoint).Port;
        var consumerPort = ((IPEndPoint)consumerListener.LocalEndpoint).Port;

        Environment.SetEnvironmentVariable("RABBITMQ_HOST", "127.0.0.1");
        Environment.SetEnvironmentVariable("RABBITMQ_PORT", rabbitPort.ToString());
        Environment.SetEnvironmentVariable("CONSUMER_RESULTS_HOST", "127.0.0.1");
        Environment.SetEnvironmentVariable("CONSUMER_RESULTS_PORT", consumerPort.ToString());

        var service = new HealthStatusService();

        var result = await service.GetStatusAsync(CancellationToken.None);

        Assert.AreEqual("healthy", result.Status);
        Assert.IsTrue(result.AllConnectionsWorking);
        Assert.AreEqual("reachable", result.Checks.RabbitMq);
        Assert.AreEqual("reachable", result.Checks.ConsumerResultsApi);
    }

    [TestMethod]
    public async Task GetStatusAsync_ReturnsDegraded_WhenDependenciesUnavailable()
    {
        Environment.SetEnvironmentVariable("RABBITMQ_HOST", "127.0.0.1");
        Environment.SetEnvironmentVariable("RABBITMQ_PORT", "65001");
        Environment.SetEnvironmentVariable("CONSUMER_RESULTS_HOST", "127.0.0.1");
        Environment.SetEnvironmentVariable("CONSUMER_RESULTS_PORT", "65002");

        var service = new HealthStatusService();

        var result = await service.GetStatusAsync(CancellationToken.None);

        Assert.AreEqual("degraded", result.Status);
        Assert.IsFalse(result.AllConnectionsWorking);
        Assert.AreEqual("unreachable", result.Checks.RabbitMq);
        Assert.AreEqual("unreachable", result.Checks.ConsumerResultsApi);
    }

    private static TcpListener CreateListener()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return listener;
    }
}
