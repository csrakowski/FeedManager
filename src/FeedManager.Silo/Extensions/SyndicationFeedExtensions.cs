using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using FeedManager.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace FeedManager.Silo.Extensions
{
    public static class SyndicationFeedExtensions
    {
        public static SyndicationItem ToSyndicationItem(this FeedItem feedItem)
        {
            return new SyndicationItem(feedItem.Title, feedItem.Content, feedItem.ItemAlternateLink, feedItem.Id, feedItem.PublishDate);
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

        public static ContentResult ToHtmlContentResult(this IEnumerable<FeedItem> feedItems)
        {
            var sb = new StringBuilder(@"<!DOCTYPE html><html lang=""en"">
<head>
<link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN"" crossorigin=""anonymous"">
<style>
    img { max-width: 100%; }
    .card { margin: 1em 0; }
</style>
</head>
<body>
<div class=""container"">
    <header>
        <h2 class=""text-center"">Page generated at: ")
                .AppendFormat("{0:yyyy-MM-dd HH:mm} UTC", DateTimeOffset.UtcNow)
                .Append(@"</h2>
    </header>
");

            foreach (var item in feedItems)
            {
                sb.WriteFeedItem(item);
            }

            sb.Append(@"</div>
<script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"" integrity=""sha384-C6RzsynM9kWDrMNeT87bh95OGNyZPhcTNXj1NW7RuBCsyN/o0jlpcV8Qyq46cDfL"" crossorigin=""anonymous""></script>
</body></html>");

            var htmlResult = sb.ToString();

            return new ContentResult
            {
                Content = htmlResult,
                ContentType = "text/html"
            };
        }

        private static StringBuilder WriteFeedItem(this StringBuilder stringBuilder, FeedItem feedItem)
        {
            var encodedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(feedItem.Id));

            stringBuilder.AppendFormat(@"<div class=""row"">
        <div class=""col-8 offset-2"">
            <article id=""{0}"" class=""card"">
                <header class=""card-header"">
                    <a class=""card-title"" href=""{1}""><h2>{2}</h2></a>
                </header>
                <div class=""card-body"">", encodedId, feedItem.ItemAlternateLink, feedItem.Title)
                    .AppendFormat("<div class=\"card-text\">{0}</div></div>", feedItem.Content)
                    .AppendFormat("<footer class=\"card-footer text-muted text-right\"><small>Published: {0}</small></footer>", feedItem.PublishDate)
                    .Append("</article></div></div>");

            return stringBuilder;
        }
    }
}
