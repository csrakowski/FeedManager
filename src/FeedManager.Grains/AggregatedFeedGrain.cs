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

namespace FeedManager.Grains
{
    internal class AggregatedFeedGrain : Grain, IAggregatedFeedGrain
    {
        private readonly ILogger<AggregatedFeedGrain> _logger;
        private readonly IPersistentState<AggregatedFeedGrainState> _state;

        public AggregatedFeedGrain(
            ILogger<AggregatedFeedGrain> logger,
            [PersistentState(
            stateName: "AggregatedFeed",
            storageName: "feedmanager")]
            IPersistentState<AggregatedFeedGrainState> state)
        {
            _logger = logger;
            _state = state;
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

            var feedGrain = GrainFactory.GetGrain<IFeedGrain>(feedUrl);
            var myKey = this.GetPrimaryKeyString();
            await feedGrain.SubscribeToUpdatesAsync(myKey);

            return true;
        }

        public async Task AddNewFeedItemsAsync(IEnumerable<FeedItem> feedItems)
        {
            _logger?.LogDebug("{method}", nameof(AddNewFeedItemsAsync));

            _state.State.FeedItems.AddRange(feedItems);

            var prePruneCount = _state.State.FeedItems.Count;

            _state.State.FeedItems = _state.State.FeedItems
                                        .Where(fi => DateTimeOffset.UtcNow.Subtract(fi.PublishDate).TotalDays < _state.State.PruneAfterDays)
                                        .OrderBy(fi => fi.PublishDate)
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
            _logger?.LogDebug("{method}", nameof(GetAggregatedFeedAsync));

            var feedItems = _state.State.FeedItems;

            return Task.FromResult(feedItems as IEnumerable<FeedItem>);
        }
    }
}