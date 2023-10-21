using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Runtime;
using FeedManager.Abstractions;
using Orleans;
using System.ServiceModel.Syndication;
using System.Xml;
using System;
using System.Reflection.Metadata;
using Microsoft.Extensions.Logging;
using CSRakowski.Parallel;

namespace FeedManager.Grains
{
    internal class FeedGrain : Grain, IFeedGrain
    {
        private readonly ILogger<FeedGrain> _logger;
        private readonly IPersistentState<FeedGrainState> _state;

        private IDisposable? _timerHandle;

        public FeedGrain(
            ILogger<FeedGrain> logger,
            [PersistentState(
            stateName: "Feed",
            storageName: "feedmanager")]
            IPersistentState<FeedGrainState> state)
        {
            _logger = logger;
            _state = state;
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _timerHandle = RegisterTimer(asyncCallback: PollFeedAsync,
                                         state: new Object(),
                                         dueTime: TimeSpan.FromSeconds(15),
                                         period: TimeSpan.FromMinutes(15));

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            _timerHandle?.Dispose();

            return Task.CompletedTask;
        }

        public async Task<bool> SubscribeToUpdatesAsync(string aggregatedFeedId)
        {
            _logger?.LogDebug("{method}: {aggregatedFeedId}", nameof(SubscribeToUpdatesAsync), aggregatedFeedId);

            var newlyAdded = _state.State.Subscribers.Add(aggregatedFeedId);

            await _state.WriteStateAsync();

            _logger?.LogDebug("{method}: {aggregatedFeedId}. Result: {newlyAdded}", nameof(SubscribeToUpdatesAsync), aggregatedFeedId, newlyAdded);

            return newlyAdded;
        }

        public async Task<bool> UnsubscribeFromUpdatesAsync(string aggregatedFeedId)
        {
            _logger?.LogDebug("{method}: {aggregatedFeedId}", nameof(UnsubscribeFromUpdatesAsync), aggregatedFeedId);

            var wasRemoved = _state.State.Subscribers.Remove(aggregatedFeedId);

            await _state.WriteStateAsync();

            _logger?.LogDebug("{method}: {aggregatedFeedId}. Result: {wasRemoved}", nameof(SubscribeToUpdatesAsync), aggregatedFeedId, wasRemoved);

            return wasRemoved;
        }

        public Task TriggerFeedRefresh()
        {
            _logger?.LogDebug("{method}", nameof(TriggerFeedRefresh));

            return PollFeedAsync(null!);
        }

        private async Task PollFeedAsync(object arg)
        {
            var feedUrl = this.GetPrimaryKeyString();

            _logger?.LogDebug("{method}: {feedUrl}", nameof(PollFeedAsync), feedUrl);

            var newFeedItems = new List<FeedItem>();

            using (var xmlReader = XmlReader.Create(feedUrl))
            {
                var syndicationFeed = SyndicationFeed.Load(xmlReader);
                xmlReader.Close();

                foreach (var syndicationItems in syndicationFeed.Items)
                {
                    var feedKey = syndicationItems.Id;
                    if (!_state.State.FeedItems.ContainsKey(feedKey))
                    {
                        _logger?.LogDebug("Found new SyndicationItem: {feedKey}", feedKey);

                        var feedItem = FeedItem.FromSyndicationItem(syndicationItems);
                        newFeedItems.Add(feedItem);
                        _state.State.FeedItems.Add(feedKey, feedItem);
                    }
                }
            }

            _logger?.LogDebug("{method}: {feedUrl} resulted in {numberOfNewItems} new items", nameof(PollFeedAsync), feedUrl, newFeedItems.Count);

            if (newFeedItems.Count > 0)
            {
                await SendUpdateAsync(newFeedItems);
                await _state.WriteStateAsync();
            }
        }

        private Task SendUpdateAsync(IEnumerable<FeedItem> feedItems)
        {
            return ParallelAsync.ForEachAsync(_state.State.Subscribers, async subscriberId => {
                _logger?.LogDebug("{method}: {subscriberId}", nameof(SendUpdateAsync), subscriberId);

                var subscriber = GrainFactory.GetGrain<IAggregatedFeedGrain>(subscriberId);
                await subscriber.AddNewFeedItemsAsync(feedItems);
            }, allowOutOfOrderProcessing: true);
        }
    }
}