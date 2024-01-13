// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace FeedManager.Silo.Services.HealthCheck;

internal interface IHealthCheckGrain : IGrainWithStringKey
{
    Task Ping();
}
