using Producer.Api.Application.Contracts;
using Producer.Api.Application.Services;
using Producer.Api.Infrastructure.Producer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IRabbitMqMessagePublisher, RabbitMqMessagePublisher>();
builder.Services.AddSingleton<IResumeEventApplicationService, ResumeEventApplicationService>();
builder.Services.AddSingleton<IResultQueryService, ResultQueryService>();
builder.Services.AddSingleton<IHealthStatusService, HealthStatusService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
