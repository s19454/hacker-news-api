using HackerNewsApi.Api.Core.Models;
using HackerNewsApi.Application.Cache;
using HackerNewsApi.Application.Constants;
using HackerNewsApi.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HackerNewsApi.Api.Services
{
    public class StoriesService : IStoriesService
    {
        private readonly IHackerNewsApiCache _cache;
        private readonly ILogger<StoriesService> _logger;
        private readonly int _cacheRetryDelay;
        private readonly int _cacheMaxRetryAttempts;

        public StoriesService(
            IHackerNewsApiCache HackerNewsApiCache, 
            ILogger<StoriesService> logger,
            IConfiguration configuration) 
        {
            _cache = HackerNewsApiCache;
            _logger = logger;

            _cacheRetryDelay = configuration.GetValue<int>(CacheConstants.RefreshMiliseconds);
            _cacheMaxRetryAttempts = configuration.GetValue<int>(CacheConstants.MaxSize);
        }

        public async Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
        {
            try
            {
                int attempt = 0;
                var cache = _cache.Get();
                while(cache == null)
                {
                    cache = await AwaitCacheRefresh(cache, cancellationToken);
                    if (cache == null && attempt++ >= _cacheMaxRetryAttempts)
                        throw new Exception($"Exceeded max retry attempts {_cacheMaxRetryAttempts}.");
                }

                return cache.Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving best stories");
                return await Task.FromException<IEnumerable<Story>>(ex);
            }
        }

        private async Task<IEnumerable<Story>?> AwaitCacheRefresh(IEnumerable<Story>? cache, CancellationToken cancellationToken)
        {
            await Task.Delay(_cacheRetryDelay, cancellationToken);
            return _cache.Get();
        }
    }
}
