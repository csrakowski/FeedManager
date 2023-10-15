using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using Orleans;

namespace FeedManager.Abstractions
{
    [DebuggerDisplay("{Title}")]
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

        [Id(5)]
        public List<string> Authors { get; }

        public string EncodedId { get; }

        public FeedItem(string id, string title, string content, Uri itemAlternateLink, DateTimeOffset publishDate, List<string> authors)
        {
            Id = id;
            Title = title;
            Content = content;
            ItemAlternateLink = itemAlternateLink;
            PublishDate = publishDate;
            Authors = authors;

            EncodedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Id));
        }

        public static FeedItem FromSyndicationItem(SyndicationItem item)
        {
            string contentText;

            if (item.Content is TextSyndicationContent content)
            {
                contentText = content.Text;
            }
            else if(item.Summary is TextSyndicationContent summary)
            {
                contentText = summary.Text;
            }
            else
            {
                contentText = "";
            }

            var link = item.Links.FirstOrDefault();
            var linkUri = (link != null)
                            ? link.Uri
                            : item.BaseUri;

            var authors = new List<string>();
            if (item.Authors != null)
            {
                authors.AddRange(item.Authors.Select(a => a.Name));
            }
            if (item.Contributors != null)
            {
                authors.AddRange(item.Contributors.Select(a => a.Name));
            }

            return new FeedItem(item.Id, item.Title.Text, contentText, linkUri, item.PublishDate, authors);
        }
    }
}
