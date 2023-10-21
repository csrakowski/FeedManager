// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using FeedManager.Abstractions;
using Microsoft.Extensions.Http;

namespace FeedManager.WebClient.Services;

public class FeedService
{
    private readonly ILogger<FeedService> _logger;
    private readonly HttpClient _httpClient;

    public FeedService(ILogger<FeedService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<FeedItem[]> Get()
    {
        try
        {
            _logger.LogDebug("Calling feed...");

            var response = await _httpClient.GetAsync("feed/json");

            _logger.LogDebug("Got response with StatusCode: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Message: {ResultMessage}", content);
                return Error(content);
            }

            var feedItems = await response.Content.ReadFromJsonAsync<FeedItem[]>();

            if (feedItems != null)
            {
                _logger.LogDebug("Parsed into {NumberOfFeedItems} FeedItems", feedItems.Length);

                return feedItems;
            }
            else
            {
                _logger.LogWarning("Error parsing FeedItems");

                return Array.Empty<FeedItem>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception: {Exception}", ex);
            return Error(ex.ToString());
        }
    }

    private FeedItem[] Error(string errorMessage)
    {
        return new[] { new FeedItem("Error", "Error", errorMessage, new Uri("#", UriKind.Relative), DateTimeOffset.UtcNow, new List<string>(0)) };
    }
}
