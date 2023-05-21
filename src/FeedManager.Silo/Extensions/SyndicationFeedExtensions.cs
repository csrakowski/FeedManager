using System.ServiceModel.Syndication;

namespace FeedManager.Silo.Extensions
{
    public static class SyndicationFeedExtensions
    {
        public static SyndicationItem ToSyndicationItem(this FeedItem feedItem)
        {
            return new SyndicationItem(feedItem.Title, feedItem.Content, feedItem.ItemAlternateLink, feedItem.Id, feedItem.LastUpdatedTime);
        }

        public static SyndicationFeed ToSyndicationFeed(this IEnumerable<FeedItem> feedItems)
        {
            var syndicationItems = feedItems.Select(fi => fi.ToSyndicationItem()).ToList();
            var resultFeed = new SyndicationFeed(syndicationItems);

            return resultFeed;
        }
    }
}