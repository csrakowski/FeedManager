// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;

namespace FeedManager.Shared;

public static class ApplicationLifetimeLinkedCancellationTokenMiddlewareExtensions
{
    public static IApplicationBuilder UseApplicationLifetimeLinkedCancellationToken(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApplicationLifetimeLinkedCancellationTokenMiddleware>();
    }
}