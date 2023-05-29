using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using Orleans;

namespace FeedManager.Abstractions
{
    [GenerateSerializer]
    public class AggregatedFeedGrainState
    {
        [Id(0)]
        public List<FeedItem> FeedItems { get; set; } = new List<FeedItem>();

        [Id(1)]
        public int PruneAfterDays { get; set; } = 14;
    }
}
