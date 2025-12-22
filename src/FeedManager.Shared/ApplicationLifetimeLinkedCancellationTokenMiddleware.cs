// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace FeedManager.Shared;

public class ApplicationLifetimeLinkedCancellationTokenMiddleware
{
    private readonly RequestDelegate _next;

    public ApplicationLifetimeLinkedCancellationTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var hostLifetime = context.RequestServices.GetRequiredService<IHostApplicationLifetime>();
        var originalCt = context.RequestAborted;
        var combinedCt = CancellationTokenSource.CreateLinkedTokenSource(originalCt, hostLifetime.ApplicationStopping).Token;
        context.RequestAborted = combinedCt;

        return _next(context);
    }
}
