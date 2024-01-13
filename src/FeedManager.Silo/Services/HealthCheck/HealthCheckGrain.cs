// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace FeedManager.Silo.Services.HealthCheck;

internal class HealthCheckGrain : Grain, IHealthCheckGrain
{
    private readonly ILogger<HealthCheckGrain> _logger;

    public HealthCheckGrain(ILogger<HealthCheckGrain> logger)
    {
        _logger = logger;
    }

    public Task Ping()
    {
        _logger?.LogDebug("In {Grain}", nameof(HealthCheckGrain));

        return Task.CompletedTask;
    }
}
