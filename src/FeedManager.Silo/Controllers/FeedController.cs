// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System.Net;
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
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
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
        public async Task<IActionResult> GetRssFeed(CancellationToken cancellationToken)
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
        public async Task<IActionResult> GetAtomFeed(CancellationToken cancellationToken)
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
        public async Task<IActionResult> GetHtmlFeed(CancellationToken cancellationToken)
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
        [ProducesResponseType(typeof(IEnumerable<FeedItem>), StatusCodes.Status200OK, "application/json")]
        public async Task<IActionResult> GetJsonFeed(CancellationToken cancellationToken)
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

        [HttpGet("subscriptions")]
        [ProducesResponseType(typeof(IEnumerable<FeedSubscription>), StatusCodes.Status200OK, "application/json")]
        public async Task<IActionResult> GetSubscriptions(CancellationToken cancellationToken)
        {
            var userId = TryGetUserId();

            if (String.IsNullOrEmpty(userId))
            {
                userId = "Chris";
                //return Unauthorized();
            }

            var subscriptions = await _aggregatedFeedService.GetSubscriptions(userId);
            return new JsonResult(subscriptions);
        }

        private string? TryGetUserId()
        {
            var claimId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!String.IsNullOrEmpty(claimId))
                return claimId;

            var authorization = Request.Headers.Authorization.FirstOrDefault(sv => sv?.StartsWith("UserId") == true);
            if (!String.IsNullOrEmpty(authorization))
            {
                var userId = authorization.Replace("UserId", "").Trim();
                return userId;
            }

            return null;
        }
    }
}
