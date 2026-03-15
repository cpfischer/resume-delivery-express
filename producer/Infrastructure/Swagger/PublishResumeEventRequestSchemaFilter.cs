using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Producer.Api.Application.Contracts;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Producer.Api.Infrastructure.Swagger;

public sealed class PublishResumeEventRequestSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(PublishResumeEventRequest))
        {
            return;
        }

        schema.Example = new OpenApiObject
        {
            ["id"] = new OpenApiString("00000000-0000-0000-0000-000000000000"),
            ["source"] = new OpenApiString("/producer/resume-events"),
            ["type"] = new OpenApiString("com.resume.submitted"),
            ["specversion"] = new OpenApiString("1.0"),
            ["time"] = new OpenApiString(DateTimeOffset.UtcNow.ToString("O")),
            ["datacontenttype"] = new OpenApiString("application/json"),
            ["data"] = new OpenApiObject
            {
                ["candidateName"] = new OpenApiString("Caleb Fischer"),
                ["targetRole"] = new OpenApiString("Software Engineer"),
                ["resumeText"] = new OpenApiString("Kubernetes RabbitMQ .NET AWS Grafana Microservices")
            }
        };
    }
}
