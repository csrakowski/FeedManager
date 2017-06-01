using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using FeedManager.GrainInterfaces;
using Orleans;

namespace FeedManager.Grains.AggregatedFeedGrain
{
    /// <summary>
    /// Grain implementation class AggregatedFeedGrain.
    /// </summary>
    public class AggregatedFeedGrain : Grain<AggregatedFeedGrainState>, IAggregatedFeedGrain
    {
        public Task AddNewFeedItemsAsync(IEnumerable<SyndicationItem> feedItems)
        {
            var logger = GetLogger();

            var sb = new StringBuilder();
            sb.Append($"There are {feedItems.Count()} new items in your feed:\n\n");

            foreach (var feedItem in feedItems)
            {
                sb.Append(feedItem.Title).Append("\n")
                    .Append(feedItem.Links)
                    .Append("\n\n");
            }

            var logMessage = sb.ToString();

            logger.Log(0, Orleans.Runtime.Severity.Warning, logMessage, null, null);

            return TaskDone.Done;
        }

        public Task<SyndicationFeed> GetAggregatedFeed()
        {
            return Task.FromResult(default(SyndicationFeed));
        }
    }
}
