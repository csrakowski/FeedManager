// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System.ServiceModel.Syndication;
using FeedManager.Silo.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FeedManager.Silo.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("[controller]")]
    [ApiController]
    public class FeedController : ControllerBase
    {
        private readonly AggregatedFeedService _aggregatedFeedService;

        public FeedController(AggregatedFeedService aggregatedFeedService)
        {
            _aggregatedFeedService = aggregatedFeedService;
        }

        [HttpGet]
        [ProducesDefaultResponseType(typeof(IEnumerable<FeedItem>))]
        public async Task<IActionResult> Get()
        {
            var userId = TryGetUserId();

            if (String.IsNullOrEmpty(userId))
            {
                userId = "Chris";
                //return Unauthorized();
            }

            var feedItems = await _aggregatedFeedService.GetAllItemsAsync(userId);

            return Ok(feedItems);
        }

        [HttpGet("rss")]
        [ProducesDefaultResponseType(typeof(SyndicationFeed))]
        public async Task<IActionResult> GetRssFeed()
        {
            var userId = TryGetUserId();

            if (String.IsNullOrEmpty(userId))
            {
                userId = "Chris";
                //return Unauthorized();
            }

            var rssFeed = await _aggregatedFeedService.GetSyndicationFeedAsync(userId);

            var result = rssFeed.ToRssContentResult();

            return result;
        }

        [HttpGet("atom")]
        [ProducesDefaultResponseType(typeof(SyndicationFeed))]
        public async Task<IActionResult> GetAtomFeed()
        {
            var userId = TryGetUserId();

            if (String.IsNullOrEmpty(userId))
            {
                userId = "Chris";
                //return Unauthorized();
            }

            var rssFeed = await _aggregatedFeedService.GetSyndicationFeedAsync(userId);

            var result = rssFeed.ToAtomContentResult();

            return result;
        }

        [HttpGet("html")]
        [ProducesDefaultResponseType(typeof(SyndicationFeed))]
        public async Task<IActionResult> GetHtmlFeed()
        {
            var userId = TryGetUserId();

            if (String.IsNullOrEmpty(userId))
            {
                userId = "Chris";
                //return Unauthorized();
            }

            var feedItems = await _aggregatedFeedService.GetAllItemsAsync(userId);

            var result = feedItems.ToHtmlContentResult();

            return result;
        }

        [HttpGet("json")]
        [ProducesDefaultResponseType(typeof(SyndicationFeed))]
        public async Task<IActionResult> GetJsonFeed()
        {
            var userId = TryGetUserId();

            if (String.IsNullOrEmpty(userId))
            {
                userId = "Chris";
                //return Unauthorized();
            }

            var feedItems = await _aggregatedFeedService.GetAllItemsAsync(userId);
            return new JsonResult(feedItems);
        }

        private string? TryGetUserId()
        {
            return User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
