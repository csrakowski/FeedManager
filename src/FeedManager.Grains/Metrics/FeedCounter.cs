// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace FeedManager.Grains.Metrics;

public class FeedCounter : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _feedCounter;
    private readonly Counter<long> _feedRefreshCounter;
    private readonly Counter<long> _feedSubscriberNotifications;
    private readonly Counter<long> _feedSubscriberCounter;
    private readonly Counter<long> _aggregateFeedSubscriberListingsCounter;
    private readonly Counter<long> _aggregateFeedFeedListingsCounter;

    public FeedCounter()
    {
        _meter = new Meter("FeedManager.Silo.FeedCounter", "1.0");

        _feedCounter = _meter.CreateCounter<long>("feeds.count", description: "Number of active feeds");
        _feedRefreshCounter = _meter.CreateCounter<long>("feeds.refreshes.count", description: "Number of feed refreshes");
        _feedSubscriberNotifications = _meter.CreateCounter<long>("feeds.notifications.count", description: "Number of feed update notifications");
        _feedSubscriberCounter = _meter.CreateCounter<long>("feeds.subscribers.count", description: "Number of feed subscribers");

        _aggregateFeedSubscriberListingsCounter = _meter.CreateCounter<long>("feeds.aggregate.subscribers.listing.count", description: "Number of calls to get AggregatedFeed Subscribers");
        _aggregateFeedFeedListingsCounter = _meter.CreateCounter<long>("feeds.aggregate.feed.listing.count", description: "Number of calls to get AggregatedFeed Feed");
    }

    public void AddFeed(string feedName)
    {
        _feedCounter.Add(1, Tag("feed_name", feedName));
    }

    public void CountFeedRefresh(string feedName)
    {
        _feedRefreshCounter.Add(1, Tag("feed_name", feedName));
    }

    public void CountSubscriberNotify(string feedName, string subscriberId)
    {
        _feedSubscriberNotifications.Add(1, Tag("feed_name", feedName), Tag("subscriber", subscriberId));
    }

    public void CountNewSubscriber(string feedName, string subscriberId)
    {
        _feedSubscriberCounter.Add(1, Tag("feed_name", feedName), Tag("subscriber", subscriberId));
    }

    public void CountRemoveSubscriber(string feedName, string subscriberId)
    {
        _feedSubscriberCounter.Add(-1, Tag("feed_name", feedName), Tag("subscriber", subscriberId));
    }

    public void CountAggregatedFeedSubscriberListing(string feedName)
    {
        _aggregateFeedSubscriberListingsCounter.Add(1, Tag("feed_name", feedName));
    }

    public void CountAggregatedFeedListing(string feedName)
    {
        _aggregateFeedFeedListingsCounter.Add(1, Tag("feed_name", feedName));
    }

    private static KeyValuePair<string, object?> Tag(string tagKey, object? tagValue)
    {
        return new KeyValuePair<string, object?>(tagKey, tagValue);
    }

    #region IDisposable

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _meter.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}
