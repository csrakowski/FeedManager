// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace FeedManager.Abstractions;

public class FeedUpdatedMessage
{
    public required string FeedId { get; set; }

    public required string NotificationMessage { get; set; }
}
