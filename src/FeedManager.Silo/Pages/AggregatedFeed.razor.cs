// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System.ServiceModel.Syndication;

namespace FeedManager.Silo.Pages
{
    public sealed partial class AggregatedFeed
    {
        private IEnumerable<FeedItem>? _feedItems;

        [Inject]
        public AggregatedFeedService AggregatedFeedService { get; set; } = null!;

        protected override Task OnInitializedAsync() => GetFeedItems();

        private Task GetFeedItems() =>
            InvokeAsync(async () =>
            {
                _feedItems = await AggregatedFeedService.GetAllItemsAsync();
                StateHasChanged();
            });

        private Task OnMarkAsRead(bool isRead)
        {
            return Task.CompletedTask;
        }
    }
}