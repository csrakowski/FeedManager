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
using Microsoft.Extensions.Logging;
using CSRakowski.Parallel;
using FeedManager.Grains.Metrics;

namespace FeedManager.Grains
{
    internal class FeedGrain : Grain, IFeedGrain
    {
        private readonly ILogger<FeedGrain> _logger;
        private readonly IPersistentState<FeedGrainState> _state;
        private readonly FeedCounter _feedCounter;

        private IGrainTimer? _timerHandle;

        public FeedGrain(
            ILogger<FeedGrain> logger,
            [PersistentState(
            stateName: "Feed",
            storageName: "feedmanager")]
            IPersistentState<FeedGrainState> state,
            FeedCounter feedCounter)
        {
            _logger = logger;
            _state = state;
            _feedCounter = feedCounter;
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _timerHandle = this.RegisterGrainTimer(callback: PollFeedAsync,
                                    options: new GrainTimerCreationOptions
                                    {
                                        DueTime = TimeSpan.FromSeconds(15),
                                        Period = TimeSpan.FromMinutes(15),
                                        KeepAlive = true
                                    });

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            _timerHandle?.Dispose();

            return Task.CompletedTask;
        }

        public async Task<bool> SubscribeToUpdatesAsync(string aggregatedFeedId)
        {
            var feedUrl = this.GetPrimaryKeyString();

            _logger?.LogDebug("{method}: {aggregatedFeedId}, {feedUrl}", nameof(SubscribeToUpdatesAsync), aggregatedFeedId, feedUrl);

            var newlyAdded = _state.State.Subscribers.Add(aggregatedFeedId);

            await _state.WriteStateAsync();

            await SendExistingItemsAsync(aggregatedFeedId);

            _logger?.LogDebug("{method}: {aggregatedFeedId}, {feedUrl}. Result: {newlyAdded}", nameof(SubscribeToUpdatesAsync), aggregatedFeedId, feedUrl, newlyAdded);

            if (newlyAdded)
            {
                _feedCounter.CountNewSubscriber(feedUrl, aggregatedFeedId);
            }

            return newlyAdded;
        }

        public async Task<bool> UnsubscribeFromUpdatesAsync(string aggregatedFeedId)
        {
            var feedUrl = this.GetPrimaryKeyString();

            _logger?.LogDebug("{method}: {aggregatedFeedId}, {feedUrl}", nameof(UnsubscribeFromUpdatesAsync), aggregatedFeedId, feedUrl);

            var wasRemoved = _state.State.Subscribers.Remove(aggregatedFeedId);

            await _state.WriteStateAsync();

            _logger?.LogDebug("{method}: {aggregatedFeedId}, {feedUrl}. Result: {wasRemoved}", nameof(SubscribeToUpdatesAsync), aggregatedFeedId, feedUrl, wasRemoved);

            if (wasRemoved)
            {
                _feedCounter.CountRemoveSubscriber(feedUrl, aggregatedFeedId);
            }

            return wasRemoved;
        }

        public Task TriggerFeedRefresh()
        {
            _logger?.LogDebug("{method}", nameof(TriggerFeedRefresh));

            return PollFeedAsync(CancellationToken.None);
        }

        private async Task PollFeedAsync(CancellationToken cancellationToken)
        {
            var feedUrl = this.GetPrimaryKeyString();

            _logger?.LogDebug("{method}: {feedUrl}", nameof(PollFeedAsync), feedUrl);

            if (cancellationToken.IsCancellationRequested)
            {
                _logger?.LogDebug("{method}: {feedUrl} was cancelled", nameof(PollFeedAsync), feedUrl);
                return;
            }

            _feedCounter.CountFeedRefresh(feedUrl);

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

                        var feedItem = FeedItem.FromSyndicationItem(syndicationItems, feedUrl);
                        newFeedItems.Add(feedItem);
                        _state.State.FeedItems.Add(feedKey, feedItem);
                    }
                }
            }

            _logger?.LogDebug("{method}: {feedUrl} resulted in {numberOfNewItems} new items", nameof(PollFeedAsync), feedUrl, newFeedItems.Count);

            if (newFeedItems.Count > 0)
            {
                await SendUpdateAsync(newFeedItems);
                await _state.WriteStateAsync(cancellationToken);
            }
        }

        private Task SendUpdateAsync(IEnumerable<FeedItem> feedItems)
        {
            var feedUrl = this.GetPrimaryKeyString();

            return ParallelAsync.ForEachAsync(_state.State.Subscribers, async subscriberId => {
                _logger?.LogDebug("{method}: {subscriberId}, {feedUrl}", nameof(SendUpdateAsync), subscriberId, feedUrl);

                var subscriber = GrainFactory.GetGrain<IAggregatedFeedGrain>(subscriberId);
                await subscriber.AddNewFeedItemsAsync(feedItems);

                _feedCounter.CountSubscriberNotify(feedUrl, subscriberId);
            }, allowOutOfOrderProcessing: true);
        }

        private Task SendExistingItemsAsync(string subscriberId)
        {
            _logger?.LogDebug("{method}: {subscriberId}", nameof(SendExistingItemsAsync), subscriberId);

            var feedItems = _state.State.FeedItems.Values;
            if (feedItems == null || feedItems.Count == 0)
            {
                return Task.CompletedTask;
            }

            var subscriber = GrainFactory.GetGrain<IAggregatedFeedGrain>(subscriberId);
            return subscriber.AddNewFeedItemsAsync(feedItems);
        }
    }
}