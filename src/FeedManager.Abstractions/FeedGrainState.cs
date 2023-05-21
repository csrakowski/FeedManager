using System.Collections.Generic;
using Orleans;

namespace FeedManager.Abstractions
{
    [GenerateSerializer]
    public class FeedGrainState
    {
        [Id(0)]
        public HashSet<string> Subscribers { get; set; } = new HashSet<string>();

        [Id(1)]
        public Dictionary<string, FeedItem> FeedItems { get; set; } = new Dictionary<string, FeedItem>();
    }
}
