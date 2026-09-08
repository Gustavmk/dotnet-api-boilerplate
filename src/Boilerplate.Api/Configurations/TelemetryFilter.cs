using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;

namespace Boilerplate.Api.Configurations;

/// <summary>
/// Shared rules to keep noisy infrastructure endpoints (Swagger, health checks)
/// out of traces and metrics.
/// </summary>
public static class TelemetryFilter
{
    private static readonly string[] IgnoredPrefixes =
    {
        "/health",
        "/swagger",
        "/api-docs",
    };

    /// <summary>
    /// True when the request targets an endpoint that should not be recorded by
    /// OpenTelemetry (used as the AspNetCore/HttpClient instrumentation filter).
    /// </summary>
    public static bool ShouldRecord(PathString path)
    {
        foreach (var prefix in IgnoredPrefixes)
        {
            if (path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Disables the built-in ASP.NET Core HTTP server metrics for ignored
    /// endpoints. Tracing is handled by the instrumentation filter; the metrics
    /// pipeline has no equivalent hook, so it is switched off per-request here.
    /// </summary>
    public static IApplicationBuilder UseTelemetryFilter(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if (!ShouldRecord(context.Request.Path))
            {
                var metricsFeature = context.Features.Get<IHttpMetricsTagsFeature>();
                if (metricsFeature is not null)
                {
                    metricsFeature.MetricsDisabled = true;
                }
            }

            await next(context);
        });
    }
}
