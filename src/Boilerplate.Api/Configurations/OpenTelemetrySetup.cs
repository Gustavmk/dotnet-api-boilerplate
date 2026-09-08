using Boilerplate.Application.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;

namespace Boilerplate.Api.Configurations;

public static class OpenTelemetrySetup
{
    public static void AddOpenTemeletrySetup(this WebApplicationBuilder builder)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;


        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource((rb) => rb
                .AddService(serviceName: OpenTelemetryExtensions.ServiceName,
                    serviceVersion: OpenTelemetryExtensions.ServiceVersion)
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector())
            .WithTracing(telemetry =>
        {
            telemetry
                .AddSource(OpenTelemetryExtensions.ServiceName)
                .AddEntityFrameworkCoreInstrumentation()
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
                    // Drop Swagger / health-check requests from traces.
                    o.Filter = ctx => TelemetryFilter.ShouldRecord(ctx.Request.Path);
                })
                .AddHttpClientInstrumentation(o =>
                {
                    o.RecordException = true;
                })
                // Captures every SQL command issued to PostgreSQL (Npgsql ActivitySource).
                .AddNpgsql()
                .AddOtlpExporter();
        })
            .WithMetrics(telemetry =>
            {
                telemetry
                    .AddMeter(OpenTelemetryExtensions.ServiceName)
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddView("http-server-request-duration",
                        new ExplicitBucketHistogramConfiguration
                        {
                            Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05,
                                0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
                        })
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
            });
    }
}