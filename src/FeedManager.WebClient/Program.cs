// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using FeedManager.WebClient.Services;
using Serilog;

namespace FeedManager.WebClient;

public static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                            .Enrich.FromLogContext()
                            .WriteTo.Console()
                            .WriteTo.Debug()
                            .CreateBootstrapLogger();

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddSerilog();
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

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapHealthChecks("/healthCheck");

        app.MapRazorPages();

        app.Run();
    }
}
