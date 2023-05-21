// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace FeedManager.Silo.Services
{
    public abstract class BaseClusterService
    {
        protected readonly IHttpContextAccessor _httpContextAccessor = null!;
        protected readonly IClusterClient _client = null!;

        protected BaseClusterService(
            IHttpContextAccessor httpContextAccessor, IClusterClient client)
        {
            _httpContextAccessor = httpContextAccessor;
            _client = client;
        }

        protected string? TryGetUserId()
        {
            return _httpContextAccessor
                ?.HttpContext
                ?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);
        }

        protected TGrain GetGrain<TGrain>(string key)
            where TGrain : IGrainWithStringKey
        {
            return _client.GetGrain<TGrain>(key);
        }

        protected TGrain TryGetGrain<TGrain>()
            where TGrain : IGrainWithStringKey
        {
            var key = TryGetUserId();
            return _client.GetGrain<TGrain>(key);
        }
    }
}