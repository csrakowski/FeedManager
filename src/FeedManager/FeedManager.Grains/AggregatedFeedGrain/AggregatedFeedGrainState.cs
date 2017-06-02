using System.Collections.Generic;
using System.ServiceModel.Syndication;

namespace FeedManager.Grains.AggregatedFeedGrain
{
    public class AggregatedFeedGrainState
    {
        public List<SyndicationItem> SyndicationItems { get; set; }
    }
}