using System;
using FeedManager.Silo.StartupTasks;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Development;

namespace FeedManager.Silo
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
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
                                //.UseInMemoryReminderService()
                                .AddStartupTask<TestFeedsStartupTask>();
                        })
                        .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
        }
    }
}