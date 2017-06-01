using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;

namespace FeedManager.Grains
{
    public class FeedGrainState
    {
        public string FeedUrl { get; set; }
        public HashSet<Guid> Subscribers { get; set; }

        public Dictionary<string, SyndicationItem> FeedItems { get; set; }
    }
}