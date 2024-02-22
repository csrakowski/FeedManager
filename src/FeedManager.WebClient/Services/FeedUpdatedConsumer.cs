// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using MassTransit;
using FeedManager.Abstractions;

namespace FeedManager.WebClient.Services;

public class FeedUpdatedConsumer : IConsumer<FeedUpdatedMessage>
{
    private readonly ILogger<FeedUpdatedConsumer> _logger;

    public FeedUpdatedConsumer(ILogger<FeedUpdatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<FeedUpdatedMessage> context)
    {
        var message = context.Message;

        _logger.LogDebug("Feed {FeedId} was updated with: {NotificationMessage}", message.FeedId, message.NotificationMessage);

        return Task.CompletedTask;
    }
}
