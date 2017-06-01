using System;
using System.Threading.Tasks;
using Orleans;

namespace FeedManager.GrainInterfaces
{
    /// <summary>
    /// Grain interface IFeedGrain
    /// </summary>
    public interface IFeedGrain : IGrainWithGuidKey
    {
        Task<bool> SetOrUpdateFeedAsync(string feedUrl);
        Task<bool> RegisterForUpdatesAsync(Guid aggregatedFeedId);
    }
}
