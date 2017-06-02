using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using FeedManager.GrainInterfaces;
using Orleans;
using Orleans.Providers;

namespace FeedManager.Grains.AggregatedFeedGrain
{
    /// <summary>
    /// Grain implementation class AggregatedFeedGrain.
    /// </summary>
    [StorageProvider(ProviderName = "MemoryStore")]
    public class AggregatedFeedGrain : Grain<AggregatedFeedGrainState>, IAggregatedFeedGrain
    {
        public override Task OnActivateAsync()
        {
            return TaskDone.Done;
        }

        public override Task OnDeactivateAsync()
        {
            return TaskDone.Done;
        }

        public async Task<bool> RegisterNewFeedForAggregationAsync(string feedUrl)
        {
            if (!Uri.TryCreate(feedUrl, UriKind.Absolute, out var feedUri))
            {
                //Should never happen, but still better safe than really sorry!
                return false;
            }

            var feedGrain = GrainFactory.GetGrain<IFeedGrain>(feedUrl);
            var myKey = this.GetPrimaryKey();
            await feedGrain.SubscribeToUpdatesAsync(myKey);

            return true;
        }

        public async Task AddNewFeedItemsAsync(IEnumerable<SyndicationItem> feedItems)
        {
            var logger = GetLogger();

            State.SyndicationItems.AddRange(feedItems);
            await WriteStateAsync();


            var sb = new StringBuilder();
            sb.Append($"There are {feedItems.Count()} new items in your feed:\n\n");

            foreach (var feedItem in feedItems)
            {
                sb.Append(feedItem.Title).Append("\n")
                    .Append(feedItem.Links)
                    .Append("\n\n");
            }

            var logMessage = sb.ToString();

            logger.Log(0, Orleans.Runtime.Severity.Warning, logMessage, null, null);
        }

        public Task<SyndicationFeed> GetAggregatedFeedAsync()
        {
            return Task.FromResult(default(SyndicationFeed));
        }
    }
}
