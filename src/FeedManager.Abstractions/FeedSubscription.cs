﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orleans;

namespace FeedManager.Abstractions
{
    [DebuggerDisplay("{Url}")]
    [GenerateSerializer]
    public class FeedSubscription
    {
        [Id(0)]
        public string Url { get; }

        [Id(1)]
        public DateTimeOffset SubscribedOn { get; }

        public string EncodedId => EncodingHelper.EncodeId(Url);

        public FeedSubscription(string url, DateTimeOffset subscribedOn)
        {
            Url = url;
            SubscribedOn = subscribedOn;
        }

        public static FeedSubscription FromFeedUrl(string feedUrl)
        {
            return new FeedSubscription(feedUrl, DateTimeOffset.UtcNow);
        }
    }
}
