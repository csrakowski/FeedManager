using System.Threading.Tasks;
using Orleans;

namespace FeedManager.Abstractions
{
    /// <summary>
    /// Grain interface IFeedGrain
    /// </summary>
    public interface IFeedGrain : IGrainWithStringKey
    {
        Task<bool> SubscribeToUpdatesAsync(string aggregatedFeedId);
        Task<bool> UnsubscribeFromUpdatesAsync(string aggregatedFeedId);
        Task TriggerFeedRefresh();
    }
}
