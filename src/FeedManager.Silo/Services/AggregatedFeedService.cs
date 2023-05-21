using System.ServiceModel.Syndication;
using FeedManager.Abstractions;
using FeedManager.Silo.Extensions;

namespace FeedManager.Silo.Services
{
    public sealed class AggregatedFeedService : BaseClusterService
    {
        public AggregatedFeedService(
            IHttpContextAccessor httpContextAccessor, IClusterClient client) :
            base(httpContextAccessor, client)
        {
        }

        public async Task<IEnumerable<FeedItem>> GetAllItemsAsync()
        {
            var key = TryGetUserId();

            if (String.IsNullOrEmpty(key))
            {
                return new List<FeedItem>();
            }

            var aggregatedFeedGrain = GetGrain<IAggregatedFeedGrain>(key);
            var feed = await aggregatedFeedGrain.GetAggregatedFeedAsync();

            return feed;
        }

        public async Task<SyndicationFeed> GetSyndicationFeedAsync()
        {
            var feedItems = await GetAllItemsAsync();

            var syndicationFeed = feedItems.ToSyndicationFeed();

            return syndicationFeed;
        }
    }
}