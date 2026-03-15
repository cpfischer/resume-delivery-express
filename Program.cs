using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

var producerProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "producer-api", "producer-api.csproj");
if (!File.Exists(producerProjectPath))
{
    Console.Error.WriteLine("Could not find producer-api/producer-api.csproj.");
    return 1;
}

Console.WriteLine("Starting Producer API from repository root...");
Console.WriteLine("Producer endpoint: POST http://localhost:8080/publish-resume-event");
Console.WriteLine("Results endpoint: GET http://localhost:8080/results/{eventId}");
Console.WriteLine("Health endpoint: GET http://localhost:8080/health");
Console.WriteLine("Swagger UI: http://localhost:8080/swagger");
Console.WriteLine("Press Ctrl+C to stop.");

using var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = "run --project producer-api",
        WorkingDirectory = Directory.GetCurrentDirectory(),
        UseShellExecute = false
    }
};

process.StartInfo.EnvironmentVariables["ASPNETCORE_URLS"] = "http://localhost:8080";

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    if (!process.HasExited)
    {
        process.Kill(entireProcessTree: true);
    }
};

process.Start();
process.WaitForExit();

return process.ExitCode;

static bool IsPortAvailable(int port)
{
    try
    {
        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        listener.Stop();
        return true;
    }
    catch (SocketException)
    {
        return false;
    }
}
