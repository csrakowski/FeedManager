// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FeedManager.Shared;

public static class OpenTelemetryConfigurationHelper
{
    public static ILoggingBuilder AddOpenTelemetryWithSharedConfiguration(this ILoggingBuilder loggingBuilder, string serviceName, IConfiguration configuration)
    {
        return loggingBuilder.AddOpenTelemetry(options =>
        {
            var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName);
            options.SetResourceBuilder(resourceBuilder)
                    //.AddConsoleExporter()
                    .AddOtlpExporter(conf => {
                        ConfigureOtlpExporter(conf, configuration);
                    });
        });
    }

    public static IOpenTelemetryBuilder AddOpenTelemetryWithSharedConfiguration(this IServiceCollection services, string serviceName, IConfiguration configuration)
    {
        return services.AddOpenTelemetry()
                        .ConfigureResource(resource => resource.AddService(serviceName))
                        .WithTracing(tracing => tracing
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            //.AddConsoleExporter()
                            .AddOtlpExporter(conf => {
                                ConfigureOtlpExporter(conf, configuration);
                            })
                        )
                        .WithMetrics(metrics => metrics
                            .AddRuntimeInstrumentation()
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            //.AddConsoleExporter()
                            .AddOtlpExporter(conf => {
                                ConfigureOtlpExporter(conf, configuration);
                            })
                        );
    }

    private static void ConfigureOtlpExporter(OtlpExporterOptions otlpExporterOptions, IConfiguration configuration)
    {
        if (configuration is not null)
        {
            var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

            if (!String.IsNullOrEmpty(otlpEndpoint))
            {
                otlpExporterOptions.Endpoint = new Uri(otlpEndpoint);
            }
        }
    }
}
