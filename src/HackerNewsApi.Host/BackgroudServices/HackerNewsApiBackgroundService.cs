using HackerNewsApi.Api.Core.Interfaces;
using HackerNewsApi.Api.Core.Models;
using HackerNewsApi.Application.Cache;
using HackerNewsApi.Application.Constants;

namespace HackerNewsApi.Api.Services
{
    public class HackerNewsApiBackgroundService : BackgroundService
    {

        private readonly IHackerNewsApiCache _cache;
        private readonly IHackerNewsApiService _HackerNewsApiApiService;
        private readonly ILogger<HackerNewsApiBackgroundService> _logger;

        private readonly int _maxSize;
        private readonly int _refreshMiliseconds;

        public HackerNewsApiBackgroundService(
            IHackerNewsApiCache HackerNewsApiCache,
            IHackerNewsApiService HackerNewsApiApiService, 
            ILogger<HackerNewsApiBackgroundService> logger,
            IConfiguration configuration)
        {
            _cache = HackerNewsApiCache;
            _HackerNewsApiApiService = HackerNewsApiApiService;
            _logger = logger;

            _maxSize = configuration.GetValue<int>(CacheConstants.MaxSize);
            _refreshMiliseconds = configuration.GetValue<int>(CacheConstants.RefreshMiliseconds);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) 
            {
                try
                {
                    var ids = await _HackerNewsApiApiService.GetBestStoryIds(cancellationToken).ConfigureAwait(false);
                    var stories = await GetStories(ids, cancellationToken).ConfigureAwait(false);
                    _cache.Refresh(stories.OrderByDescending(s => s.score).Take(_maxSize).ToList());
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, $"Error refreshing cache.");
                }
                await Task.Delay(_refreshMiliseconds, cancellationToken);
            }
        }

        private async Task<IEnumerable<Story>> GetStories(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            var storyTasks = ids.Select(id => _HackerNewsApiApiService.GetStoryAsync(id, cancellationToken)).ToList();
            return await Task.WhenAll(storyTasks);
        }
    }
}
