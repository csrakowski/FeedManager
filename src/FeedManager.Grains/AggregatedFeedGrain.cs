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
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FeedManager.Grains.Metrics;
using CSRakowski.Parallel;

namespace FeedManager.Grains
{
    internal class AggregatedFeedGrain : Grain, IAggregatedFeedGrain
    {
        private readonly ILogger<AggregatedFeedGrain> _logger;
        private readonly IPersistentState<AggregatedFeedGrainState> _state;
        private readonly FeedCounter _feedCounter;

        public AggregatedFeedGrain(
            ILogger<AggregatedFeedGrain> logger,
            [PersistentState(
            stateName: "AggregatedFeed",
            storageName: "feedmanager")]
            IPersistentState<AggregatedFeedGrainState> state,
            FeedCounter feedCounter)
        {
            _logger = logger;
            _state = state;
            _feedCounter = feedCounter;
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task<bool> RegisterNewFeedForAggregationAsync(string feedUrl)
        {
            if (!Uri.TryCreate(feedUrl, UriKind.Absolute, out var feedUri))
            {
                //Should never happen, but still better safe than really sorry!
                return false;
            }

            var myKey = this.GetPrimaryKeyString();

            var encodedUrl = EncodingHelper.EncodeId(feedUrl);
            var subscription = _state.State.SubscribedFeeds.Find(sub => String.Equals(sub.EncodedId, encodedUrl));
            if (subscription != null)
            {
                _logger?.LogDebug("User {UserId} is already subscribed to {FeedUrl}", myKey, feedUrl);
                return false;
            }

            var feedGrain = GrainFactory.GetGrain<IFeedGrain>(feedUrl);
            await feedGrain.SubscribeToUpdatesAsync(myKey);

            _state.State.SubscribedFeeds.Add(FeedSubscription.FromFeedUrl(feedUrl));

            await _state.WriteStateAsync();

            return true;
        }

        public async Task<bool> DeregisterFeedFromAggregationAsync(string feedUrl)
        {
            if (!Uri.TryCreate(feedUrl, UriKind.Absolute, out var feedUri))
            {
                //Should never happen, but still better safe than really sorry!
                return false;
            }

            var myKey = this.GetPrimaryKeyString();

            var encodedUrl = EncodingHelper.EncodeId(feedUrl);
            var subscription = _state.State.SubscribedFeeds.Find(sub => String.Equals(sub.EncodedId, encodedUrl));
            if (subscription == null)
            {
                _logger?.LogDebug("User {UserId} isn't subscribed to {FeedUrl} (anymore)", myKey, feedUrl);
                return false;
            }

            var feedGrain = GrainFactory.GetGrain<IFeedGrain>(feedUrl);
            await feedGrain.UnsubscribeFromUpdatesAsync(myKey);

            _state.State.SubscribedFeeds.Remove(subscription);

            _state.State.FeedItems = _state.State.FeedItems
                                        .Where(fi => fi.EncodedFeedId != encodedUrl)
                                        .OrderBy(fi => fi.PublishDate)
                                        .ToList();

            await _state.WriteStateAsync();

            return true;
        }

        public async Task AddNewFeedItemsAsync(IEnumerable<FeedItem> feedItems)
        {
            _logger?.LogDebug("{method}", nameof(AddNewFeedItemsAsync));

            _state.State.FeedItems.AddRange(feedItems);

            var prePruneCount = _state.State.FeedItems.Count;

            _state.State.FeedItems = _state.State.FeedItems
                                        .Where(fi => DateTimeOffset.UtcNow.Subtract(fi.PublishDate).TotalDays < _state.State.PruneAfterDays)
                                        .OrderBy(fi => fi.PublishDate.UtcDateTime)
                                        .ToList();

            await _state.WriteStateAsync();

            var sb = new StringBuilder();

            sb.Append("There are ")
                .Append(feedItems.Count())
                .AppendLine(" new items in your feed:\n");

            foreach (var feedItem in feedItems)
            {
                sb.AppendLine(feedItem.Title)
                    .AppendLine(feedItem.ItemAlternateLink.ToString())
                    .AppendLine();
            }

            sb.AppendFormat("Pruned {0} day old content. Started with {1}, ended with {2} items.\n", _state.State.PruneAfterDays, prePruneCount, _state.State.FeedItems.Count);

            var logMessage = sb.ToString();

            _logger?.LogDebug("{notificationMessage}", logMessage);
        }

        public Task<IEnumerable<FeedItem>> GetAggregatedFeedAsync()
        {
            var myKey = this.GetPrimaryKeyString();

            _logger?.LogDebug("{method} {aggregatedFeed}", nameof(GetAggregatedFeedAsync), myKey);

            var feedItems = _state.State.FeedItems;

            _feedCounter.CountAggregatedFeedListing(myKey);

            return Task.FromResult(feedItems as IEnumerable<FeedItem>);
        }

        public Task<IEnumerable<FeedSubscription>> GetSubscriptions()
        {
            var myKey = this.GetPrimaryKeyString();

            _logger?.LogDebug("{method} {aggregatedFeed}", nameof(GetSubscriptions), myKey);

            var subscriptions = _state.State.SubscribedFeeds;

            _feedCounter.CountAggregatedFeedSubscriberListing(myKey);

            return Task.FromResult(subscriptions as IEnumerable<FeedSubscription>);
        }

        public Task RefreshAggregatedFeedAsync()
        {
            var myKey = this.GetPrimaryKeyString();

            _logger?.LogDebug("{method} {aggregatedFeed}", nameof(RefreshAggregatedFeedAsync), myKey);

            var feeds = _state.State.SubscribedFeeds.Select(feed => feed.Url).Distinct().ToList();

            return ParallelAsync.ForEachAsync(feeds, async feedSubscription => {
                _logger?.LogDebug("{method}: {aggregatedFeed}, {feedUrl}", nameof(RefreshAggregatedFeedAsync), myKey, feedSubscription);

                var feed = GrainFactory.GetGrain<IFeedGrain>(feedSubscription);
                await feed.TriggerFeedRefresh();

            }, allowOutOfOrderProcessing: true);
        }
    }
}