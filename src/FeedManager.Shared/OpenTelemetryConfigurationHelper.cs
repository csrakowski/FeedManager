// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FeedManager.Shared;

public static class OpenTelemetryConfigurationHelper
{
    public static IOpenTelemetryBuilder AddOpenTelemetryWithSharedConfiguration(this IServiceCollection services, string serviceName)
    {
        return services.AddOpenTelemetry()
                          .ConfigureResource(resource => resource.AddService(serviceName))
                          .WithTracing(tracing => tracing
                              .AddAspNetCoreInstrumentation()
                              .AddHttpClientInstrumentation()
                              .AddConsoleExporter())
                          .WithMetrics(metrics => metrics
                              .AddRuntimeInstrumentation()
                              .AddAspNetCoreInstrumentation()
                              .AddHttpClientInstrumentation()
                              .AddConsoleExporter());
    }
}
