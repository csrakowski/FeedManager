// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FeedManager.WebClient.Services.HealthCheck;

internal class FeedServiceHealthCheck : IHealthCheck
{
    private readonly ILogger<FeedServiceHealthCheck> _logger;
    private readonly FeedService _feedService;

    public FeedServiceHealthCheck(ILogger<FeedServiceHealthCheck> logger, FeedService feedService)
    {
        _logger = logger;
        _feedService = feedService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var grain = await _feedService.Get("Chris", cancellationToken);

            return HealthCheckResult.Healthy("A healthy result.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured during FeedService HealthCheck");
        }

        return new HealthCheckResult(context.Registration.FailureStatus, "An unhealthy result.");
    }
}