using Consumer.Api.Infrastructure.Consumer;
using Consumer.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<ResultStore>();
builder.Services.AddHostedService<ResumeEventConsumer>();

var app = builder.Build();

app.MapControllers();

app.Run();
