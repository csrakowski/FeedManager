using FeedManager.GrainInterfaces;
using Orleans;

namespace FeedManager.Grains
{
    /// <summary>
    /// The FeedGrain is responsible for managing the state of a single, external rss/atom feed.
    /// Depending on the endpoint, it will either subscribe to an event or just periodically poll the feed for new items
    /// </summary>
    public class FeedGrain : Grain, IFeedGrain
    {

    }
}
