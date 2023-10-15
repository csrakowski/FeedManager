// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using FeedManager.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FeedManager.WebClient.Pages;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public FeedItem[] FeedItems { get; private set; }

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
        FeedItems = new FeedItem[0];
    }

    public async Task OnGet()
    {
        using var client = new HttpClient()
        {
            BaseAddress = new Uri("http://feedmanagersilo/")
        };

        _logger.LogDebug("Calling feed...");

        var response = await client.GetAsync("feed/json");

        _logger.LogDebug("Got response with StatusCode: {StatusCode}", response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Message: {ResultMessage}", content);
            return;
        }

        var feedItems = await response.Content.ReadFromJsonAsync<FeedItem[]>();

        if (feedItems != null)
        {
            _logger.LogDebug("Parsed into {NumberOfFeedItems} FeedItems", feedItems.Length);

            FeedItems = feedItems;
        }
        else
        {
            _logger.LogWarning("Error parsing FeedItems");
        }
    }
}
