// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace FeedManager.Abstractions;

public class FeedUpdatedMessage
{
    public string FeedId { get; set; }

    public string NotificationMessage { get; set; }
}
