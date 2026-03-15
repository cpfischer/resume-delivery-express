namespace Consumer.Api.Domain.Services;

public static class SkillDetector
{
    private static readonly string[] SkillKeywords =
    [
        "Kubernetes",
        "RabbitMQ",
        ".NET",
        "AWS",
        "Grafana",
        "Microservices"
    ];

    public static string[] Detect(string resumeText)
    {
        return SkillKeywords
            .Where(skill => resumeText.Contains(skill, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}
