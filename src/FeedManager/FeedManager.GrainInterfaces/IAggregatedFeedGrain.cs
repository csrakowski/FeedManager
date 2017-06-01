using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Orleans;

namespace FeedManager.GrainInterfaces
{
    /// <summary>
    /// Grain interface IAggregatedFeedGrain
    /// </summary>
    public interface IAggregatedFeedGrain : IGrainWithGuidKey
    {
        Task AddNewFeedItemsAsync(IEnumerable<SyndicationItem> feedItems);
        Task<SyndicationFeed> GetAggregatedFeed();
    }
}
