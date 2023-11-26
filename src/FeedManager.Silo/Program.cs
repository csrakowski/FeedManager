using System;
using FeedManager.Silo.StartupTasks;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Development;
using Serilog;

namespace FeedManager.Silo
{
    public static class Program
    {
        public const string ServiceName = "FeedManager.Silo";

        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                            .Enrich.FromLogContext()
                            .WriteTo.Console()
                            .WriteTo.Debug()
                            .CreateBootstrapLogger();

            var hostBuilder = BuildHost(args);
            await hostBuilder.RunConsoleAsync();

            return 0;
        }

        private static IHostBuilder BuildHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                        .UseOrleans((context, builder) =>
                        {
                            builder.UseLocalhostClustering()
                                .AddMemoryGrainStorage("feedmanager")
                                .AddStartupTask<TestFeedsStartupTask>();

                            builder.ConfigureLogging(lb =>
                            {
                                lb.AddSerilog();
                                lb.AddOpenTelemetry(options =>
                                {
                                    options
                                        .SetResourceBuilder(
                                            ResourceBuilder.CreateDefault()
                                                .AddService(ServiceName))
                                        .AddConsoleExporter();
                                });
                            });

                            builder.UseInMemoryReminderService();

                            builder.UseDashboard();
                        })
                        .UseSerilog((context, services, configuration) =>
                            configuration
                                .ReadFrom.Configuration(context.Configuration)
                                .ReadFrom.Services(services)
                                .Enrich.FromLogContext()
                                .WriteTo.Console()
                        )
                        .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
        }
    }
}