// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using FeedManager.Shared;
using FeedManager.WebClient.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace FeedManager.WebClient;

public static class Program
{
    public const string ServiceName = "FeedManager.WebClient";

    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                            .Enrich.FromLogContext()
                            .WriteTo.Console()
                            .WriteTo.Debug()
                            .CreateBootstrapLogger();

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Logging.AddOpenTelemetryWithSharedConfiguration(ServiceName, builder.Configuration);

        builder.Services.AddSerilog();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddOpenTelemetryWithSharedConfiguration(ServiceName, builder.Configuration);
        builder.Services.AddRazorPages();
        builder.Services.AddHttpClient<FeedService>()
                        .ConfigureHttpClient(client => {
                            var uri = builder.Configuration["FeedService:BaseUrl"];
                            client.BaseAddress = new Uri(uri);

                            client.DefaultRequestVersion = System.Net.HttpVersion.Version30;
                            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                        })
                        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                        {
                            ClientCertificateOptions = ClientCertificateOption.Manual,
                            ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                            {
                                return true;
                            }
                        });
        builder.Services.AddHealthChecks();
        builder.Services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumers(typeof(Program).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(
                    host: builder.Configuration.GetValue<string>("RABBITMQ_URL"),
                    port: builder.Configuration.GetValue<ushort>("RABBITMQ_PORT"),
                    virtualHost: builder.Configuration.GetValue<string>("RABBITMQ_VIRTUALHOST"),
                    configure: c =>
                    {
                        c.Username(builder.Configuration.GetValue<string>("RABBITMQ_USER"));
                        c.Password(builder.Configuration.GetValue<string>("RABBITMQ_PASSWORD"));
                    });

                cfg.ConfigureEndpoints(context);
            });
        });

        builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console());

        var app = builder.Build();

        app.UseSerilogRequestLogging();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseApplicationLifetimeLinkedCancellationToken();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapHealthChecks("/healthCheck");

        app.MapRazorPages();

        app.Run();
    }
}
