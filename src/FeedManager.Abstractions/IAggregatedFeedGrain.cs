using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Orleans;

namespace FeedManager.Abstractions
{
    /// <summary>
    /// Grain interface IAggregatedFeedGrain
    /// </summary>
    public interface IAggregatedFeedGrain : IGrainWithStringKey
    {
        Task<bool> RegisterNewFeedForAggregationAsync(string feedUrl);
        Task AddNewFeedItemsAsync(IEnumerable<FeedItem> feedItems);
        Task<IEnumerable<FeedItem>> GetAggregatedFeedAsync();
    }
}
