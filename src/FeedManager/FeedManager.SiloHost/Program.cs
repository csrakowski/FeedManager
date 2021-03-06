using System;
using System.Threading.Tasks;
using FeedManager.GrainInterfaces;
using Orleans;
using Orleans.Runtime.Configuration;

namespace FeedManager.SiloHost
{
    /// <summary>
    /// Orleans test silo host
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = args,
            });

            var config = ClientConfiguration.LocalhostSilo();
            GrainClient.Initialize(config);

            // TODO: once the previous call returns, the silo is up and running.
            //       This is the place your custom logic, for example calling client logic
            //       or initializing an HTTP front end for accepting incoming requests.

            Console.WriteLine("Orleans Silo is running.");

            MainAsync().Wait();

            Console.WriteLine("\nPress Enter to terminate...");
            Console.ReadLine();

            hostDomain.DoCallBack(ShutdownSilo);
        }


        private static async Task MainAsync()
        {
            Console.WriteLine("Getting aggregated feed...");

            var aggregatedFeed = GrainClient.GrainFactory.GetGrain<IAggregatedFeedGrain>(Guid.Empty);

            Console.WriteLine("Registering The Verge");
            await aggregatedFeed.RegisterNewFeedForAggregationAsync("https://www.theverge.com/rss/index.xml");

            Console.WriteLine("Registering NOS");
            await aggregatedFeed.RegisterNewFeedForAggregationAsync("http://feeds.nos.nl/nosnieuwsalgemeen");

            Console.WriteLine("Set up complete!");
        }

        private static void InitSilo(string[] args)
        {
            hostWrapper = new OrleansHostWrapper(args);

            if (!hostWrapper.Run())
            {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
        }

        private static void ShutdownSilo()
        {
            if (hostWrapper != null)
            {
                hostWrapper.Dispose();
                GC.SuppressFinalize(hostWrapper);
            }
        }

        private static OrleansHostWrapper hostWrapper;
    }
}
