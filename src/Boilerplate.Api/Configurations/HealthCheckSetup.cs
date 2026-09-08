using Boilerplate.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Boilerplate.Api.Configurations;

public static class HealthCheckSetup
{
    /// <summary>
    /// Tag applied to checks that probe an external dependency (database, ...).
    /// The readiness endpoint only runs checks carrying this tag.
    /// </summary>
    public const string ReadinessTag = "ready";

    public static IServiceCollection AddHealthCheckSetup(this IServiceCollection services)
    {
        services.AddHealthChecks()
            // Validates that the application can actually reach its database.
            .AddDbContextCheck<ApplicationDbContext>(
                name: "database",
                tags: new[] { ReadinessTag });

        return services;
    }

    public static IEndpointRouteBuilder MapHealthCheckSetup(this IEndpointRouteBuilder endpoints)
    {
        // Deep check: every dependency must be healthy. Use for readinessProbe.
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadinessTag),
            ResponseWriter = WriteResponse,
        }).DisableHttpMetrics();

        // Dummy check: only confirms the process is up and serving. Use for livenessProbe.
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
        }).DisableHttpMetrics();

        return endpoints;
    }

    private static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                durationMs = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                error = e.Value.Exception?.Message,
            }),
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
