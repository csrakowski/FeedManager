using System.ServiceModel.Syndication;
using FeedManager.Abstractions;
using FeedManager.Silo.Extensions;

namespace FeedManager.Silo.Services
{
    public sealed class AggregatedFeedService : BaseClusterService
    {
        public AggregatedFeedService(IClusterClient client)
            : base(client)
        {
        }

        public async Task<IEnumerable<FeedItem>> GetAllItemsAsync(string userId)
        {
            var aggregatedFeedGrain = GetGrain<IAggregatedFeedGrain>(userId);

            var feed = await aggregatedFeedGrain.GetAggregatedFeedAsync();

            return feed;
        }

        public async Task<SyndicationFeed> GetSyndicationFeedAsync(string userId)
        {
            var feedItems = await GetAllItemsAsync(userId);

            var syndicationFeed = feedItems.ToSyndicationFeed();

            return syndicationFeed;
        }

        public async Task<bool> RegisterNewFeedForAggregationAsync(string userId, string feedUrl)
        {
            var aggregatedFeedGrain = GetGrain<IAggregatedFeedGrain>(userId);

            var result = await aggregatedFeedGrain.RegisterNewFeedForAggregationAsync(feedUrl);

            return result;
        }

        public async Task<bool> DeregisterFeedFromAggregationAsync(string userId, string feedUrl)
        {
            var aggregatedFeedGrain = GetGrain<IAggregatedFeedGrain>(userId);

            var result = await aggregatedFeedGrain.DeregisterFeedFromAggregationAsync(feedUrl);

            return result;
        }

        public async Task<IEnumerable<FeedSubscription>> GetSubscriptions(string userId)
        {
            var aggregatedFeedGrain = GetGrain<IAggregatedFeedGrain>(userId);

            var result = await aggregatedFeedGrain.GetSubscriptions();

            return result;
        }

        public Task RefreshAggregatedFeedAsync(string userId)
        {
            var aggregatedFeedGrain = GetGrain<IAggregatedFeedGrain>(userId);

            return aggregatedFeedGrain.RefreshAggregatedFeedAsync();
        }
    }
}