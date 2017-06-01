using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using FeedManager.GrainInterfaces;
using Orleans;
using Orleans.Providers;

namespace FeedManager.Grains
{
    /// <summary>
    /// The FeedGrain is responsible for managing the state of a single, external rss/atom feed.
    /// Depending on the endpoint, it will either subscribe to an event or just periodically poll the feed for new items
    /// </summary>
    [StorageProvider(ProviderName = "store1")]
    public class FeedGrain : Grain<FeedGrainState>, IFeedGrain
    {
        private IDisposable _timerHandle;

        public override Task OnActivateAsync()
        {
            _timerHandle = RegisterTimer(PollFeedAsync, null, TimeSpan.FromSeconds(15), TimeSpan.FromMinutes(15));

            return TaskDone.Done;
        }

        public override Task OnDeactivateAsync()
        {
            _timerHandle.Dispose();

            return TaskDone.Done;
        }

        public async Task<bool> RegisterForUpdatesAsync(Guid aggregatedFeedId)
        {
            var newlyAdded = State.Subscribers.Add(aggregatedFeedId);

            await WriteStateAsync();

            return newlyAdded;
        }

        public async Task<bool> SetOrUpdateFeedAsync(string feedUrl)
        {
            if (feedUrl == null)
                throw new ArgumentNullException(nameof(feedUrl));

            var isSame = (State.FeedUrl == feedUrl);

            State.FeedUrl = feedUrl;

            await WriteStateAsync();

            return isSame;
        }

        private async Task PollFeedAsync(object arg)
        {
            var feedUrl = State.FeedUrl;
            if (String.IsNullOrWhiteSpace(feedUrl))
            {
                //Should never happen, but better safe than really sorry!
                return;
            }

            var newFeedItems = new List<SyndicationItem>();

            var xmlReader = XmlReader.Create(feedUrl);
            var syndicationFeed = SyndicationFeed.Load(xmlReader);
            xmlReader.Close();
            foreach (SyndicationItem feedItem in syndicationFeed.Items)
            {
                var feedKey = feedItem.Id;
                if (!State.FeedItems.ContainsKey(feedKey))
                {
                    newFeedItems.Add(feedItem);
                    State.FeedItems.Add(feedKey, feedItem);
                }
            }

            if (newFeedItems.Count > 0)
            {
                await SendUpdateAsync(newFeedItems);
                await WriteStateAsync();
            }
        }

        private async Task SendUpdateAsync(IEnumerable<SyndicationItem> feedItems)
        {
            foreach (var subscriberId in State.Subscribers)
            {
                var subscriber = this.GrainFactory.GetGrain<IAggregatedFeedGrain>(subscriberId);
                await subscriber.AddNewFeedItemsAsync(feedItems);
            }
        }
    }
}
