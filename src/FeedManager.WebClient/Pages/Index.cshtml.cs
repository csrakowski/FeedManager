// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using FeedManager.Abstractions;
using FeedManager.WebClient.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FeedManager.WebClient.Pages;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly FeedService _feedService;

    public FeedItem[] FeedItems { get; private set; }

    public IndexModel(ILogger<IndexModel> logger, FeedService feedService)
    {
        _logger = logger;
        _feedService = feedService;
        FeedItems = new FeedItem[0];
    }

    public async Task OnGet()
    {
        FeedItems = await _feedService.Get();
    }
}
