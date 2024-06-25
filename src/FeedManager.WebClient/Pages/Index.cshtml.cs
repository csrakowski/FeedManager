// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
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

    public async Task OnGet(CancellationToken cancellationToken)
    {
        FeedItems = await _feedService.Get("Chris", cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await _feedService.RefreshFeeds("Chris", cancellationToken);
        if (result.error)
        {
            return BadRequest(result.message);
        }
        else
        {
            return StatusCode(StatusCodes.Status202Accepted);
        }
    }
}
