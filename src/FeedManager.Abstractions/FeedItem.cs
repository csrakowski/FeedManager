using System;
using System.Linq;
using System.ServiceModel.Syndication;
using Orleans;

namespace FeedManager.Abstractions
{
    [GenerateSerializer]
    public class FeedItem
    {
        [Id(0)]
        public string Id { get; }

        [Id(1)]
        public string Title { get; }

        [Id(2)]
        public string Content { get; }

        [Id(3)]
        public Uri ItemAlternateLink { get; }

        [Id(4)]
        public DateTimeOffset PublishDate { get; }

        public FeedItem(string id, string title, string content, Uri itemAlternateLink, DateTimeOffset publishDate)
        {
            Id = id;
            Title = title;
            Content = content;
            ItemAlternateLink = itemAlternateLink;
            PublishDate = publishDate;
        }

        public static FeedItem FromSyndicationItem(SyndicationItem item)
        {
            string contentText;

            if (item.Content is TextSyndicationContent content)
            {
                contentText = content.Text;
            }
            else
            {
                contentText = "";
            }

            var link = item.Links.FirstOrDefault();
            var linkUri = (link != null)
                            ? link.Uri
                            : item.BaseUri;

            return new FeedItem(item.Id, item.Title.Text, contentText, linkUri, item.PublishDate);
        }
    }
}
