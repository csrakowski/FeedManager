using System;
using System.Threading.Tasks;
using Orleans;

namespace FeedManager.GrainInterfaces
{
    /// <summary>
    /// Grain interface IFeedGrain
    /// </summary>
    public interface IFeedGrain : IGrainWithStringKey
    {
        Task<bool> SubscribeToUpdatesAsync(Guid aggregatedFeedId);
        Task<bool> UnsubscribeFromUpdatesAsync(Guid aggregatedFeedId);
    }
}
