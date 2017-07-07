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
    [StorageProvider(ProviderName = "MemoryStore")]
    public class FeedGrain : Grain<FeedGrainState>, IFeedGrain
    {
        private IDisposable _timerHandle;

        public override Task OnActivateAsync()
        {
            _timerHandle = RegisterTimer(PollFeedAsync, null, TimeSpan.FromSeconds(15), TimeSpan.FromMinutes(15));

            //Init state when needed
            if (this.State.FeedItems == null)
            {
                this.State.FeedItems = new Dictionary<string, SyndicationItem>();
            }

            if (this.State.Subscribers == null)
            {
                this.State.Subscribers = new HashSet<Guid>();
            }

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            _timerHandle.Dispose();

            return Task.CompletedTask;
        }

        public async Task<bool> SubscribeToUpdatesAsync(Guid aggregatedFeedId)
        {
            var newlyAdded = State.Subscribers.Add(aggregatedFeedId);

            await WriteStateAsync().ConfigureAwait(false);

            return newlyAdded;
        }

        public async Task<bool> UnsubscribeFromUpdatesAsync(Guid aggregatedFeedId)
        {
            var wasRemoved = State.Subscribers.Remove(aggregatedFeedId);

            await WriteStateAsync().ConfigureAwait(false);

            return wasRemoved;
        }

        private async Task PollFeedAsync(object arg)
        {
            var feedUrl = this.GetPrimaryKeyString();

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
                await SendUpdateAsync(newFeedItems).ConfigureAwait(false);
                await WriteStateAsync().ConfigureAwait(false);
            }
        }

        private async Task SendUpdateAsync(IEnumerable<SyndicationItem> feedItems)
        {
            foreach (var subscriberId in State.Subscribers)
            {
                var subscriber = this.GrainFactory.GetGrain<IAggregatedFeedGrain>(subscriberId);
                await subscriber.AddNewFeedItemsAsync(feedItems).ConfigureAwait(false);
            }
        }
    }
}
