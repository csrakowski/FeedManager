using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.AspNetCore.Mvc;

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

        public static ContentResult ToRssContentResult(this SyndicationFeed syndicationFeed)
        {
            return RenderFeed(
                syndicationFeed: syndicationFeed,
                contentType: "application/rss+xml",
                renderAction: (feed, writer) => feed.SaveAsRss20(writer)
            );
        }

        public static ContentResult ToAtomContentResult(this SyndicationFeed syndicationFeed)
        {
            return RenderFeed(
                syndicationFeed: syndicationFeed,
                contentType: "application/atom+xml",
                renderAction: (feed, writer) => feed.SaveAsAtom10(writer)
            );
        }

        private static ContentResult RenderFeed(SyndicationFeed syndicationFeed, string contentType, Action<SyndicationFeed, XmlWriter> renderAction)
        {
            var stringWriter = new System.IO.StringWriter();
            var xmlWriter = System.Xml.XmlWriter.Create(stringWriter);

            renderAction(syndicationFeed, xmlWriter);

            var feedString = stringWriter.ToString();

            return new ContentResult
            {
                Content = feedString,
                ContentType = contentType
            };
        }
    }
}