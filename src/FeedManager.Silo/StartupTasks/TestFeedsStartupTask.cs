﻿namespace FeedManager.Silo.StartupTasks
{
    public sealed class TestFeedsStartupTask : IStartupTask
    {
        private readonly IGrainFactory _grainFactory;
        private readonly ILogger<TestFeedsStartupTask> _logger;

        public TestFeedsStartupTask(IGrainFactory grainFactory, ILogger<TestFeedsStartupTask> logger)
        {
            _grainFactory = grainFactory;
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Getting aggregated feed...");

            var aggregatedFeed = _grainFactory.GetGrain<IAggregatedFeedGrain>("");

            _logger.LogDebug("Registering The Verge");
            await aggregatedFeed.RegisterNewFeedForAggregationAsync("https://www.theverge.com/rss/index.xml");

            _logger.LogDebug("Registering NOS");
            await aggregatedFeed.RegisterNewFeedForAggregationAsync("https://feeds.nos.nl/nosnieuwsalgemeen");

            _logger.LogDebug("Set up complete!");
        }
    }
}