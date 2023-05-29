// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace FeedManager.Silo.Services
{
    public abstract class BaseClusterService
    {
        protected readonly IClusterClient _client = null!;

        protected BaseClusterService(IClusterClient client)
        {
            _client = client;
        }

        protected TGrain GetGrain<TGrain>(string key)
            where TGrain : IGrainWithStringKey
        {
            return _client.GetGrain<TGrain>(key);
        }
    }
}