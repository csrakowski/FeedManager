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
            await _state.WriteStateAsync();

            var sb = new StringBuilder();
            sb.Append("There are ")
                .Append(feedItems.Count())
                .Append(" new items in your feed:\n\n");

            foreach (var feedItem in feedItems)
            {
                sb.AppendLine(feedItem.Title)
                    .Append(feedItem.ItemAlternateLink)
                    .Append("\n\n");
            }

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