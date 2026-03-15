var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<ResultStore>();
builder.Services.AddHostedService<ResumeEventConsumer>();

var app = builder.Build();

app.MapControllers();

var url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5002";
app.Run(url);
