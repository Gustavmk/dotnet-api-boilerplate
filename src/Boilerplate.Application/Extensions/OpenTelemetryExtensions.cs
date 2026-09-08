using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Boilerplate.Application.Extensions;

public static class OpenTelemetryExtensions
{
    public static string ServiceName { get; }
    public static string ServiceVersion { get; }

    public static ActivitySource ActivitySource { get; }

    /// <summary>
    /// Meter used to publish application-level metrics (same name as the tracing source).
    /// </summary>
    public static Meter Meter { get; }

    /// <summary>
    /// Counts every Hero created through the API.
    /// </summary>
    public static Counter<long> HeroesCreatedCounter { get; }

    static OpenTelemetryExtensions()
    {
        ServiceName = typeof(OpenTelemetryExtensions).Assembly.GetName().Name!;
        ServiceVersion = typeof(OpenTelemetryExtensions).Assembly.GetName().Version!.ToString();
        ActivitySource = new ActivitySource(ServiceName, ServiceVersion);
        Meter = new Meter(ServiceName, ServiceVersion);
        HeroesCreatedCounter = Meter.CreateCounter<long>(
            "heroes.created",
            unit: "{hero}",
            description: "Number of heroes created through POST api/Hero.");
    }
}
