// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FeedManager.Silo.Services.HealthCheck;

internal class SiloHealthCheck : IHealthCheck
{
    private readonly ILogger<SiloHealthCheck> _logger;
    private readonly IClusterClient _client;

    public SiloHealthCheck(ILogger<SiloHealthCheck> logger, IClusterClient client)
    {
        _logger = logger;
        _client = client;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var isHealthy = false;

        try
        {
            var grain = _client.GetGrain<IHealthCheckGrain>(nameof(SiloHealthCheck));

            grain.Ping();

            isHealthy = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured during Silo HealthCheck");
        }

        if (isHealthy)
        {
            return Task.FromResult(
                HealthCheckResult.Healthy("A healthy result."));
        }

        return Task.FromResult(
            new HealthCheckResult(
                context.Registration.FailureStatus, "An unhealthy result."));
    }
}