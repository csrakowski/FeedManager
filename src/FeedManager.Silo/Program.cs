using System;
using FeedManager.Silo.StartupTasks;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Orleans;
using Orleans.Hosting;
using Orleans.Persistence;
using Orleans.Persistence.Redis;
using Orleans.Runtime;
using Orleans.Runtime.Development;
using Serilog;
using StackExchange.Redis;

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
                                .AddRedisGrainStorage("feedmanager", o =>
                                {
                                    var redisEndpoint = context.Configuration["REDIS_URL"];

                                    if (String.IsNullOrEmpty(redisEndpoint))
                                    {
                                        throw new ArgumentException("No REDIS_URL configuration found.");
                                    }

                                    Log.Logger?.Information("Using Redis GrainStorage at {redisEndpoint}.", redisEndpoint);

                                    o.ConfigurationOptions = new ConfigurationOptions()
                                    {
                                        EndPoints = { redisEndpoint },
                                        AbortOnConnectFail = false,
                                        SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
                                    };
                                })
                                .AddStartupTask<TestFeedsStartupTask>();

                            builder.ConfigureLogging(lb =>
                            {
                                lb.AddSerilog();
                                lb.AddOpenTelemetryWithSharedConfiguration(ServiceName, context.Configuration);
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