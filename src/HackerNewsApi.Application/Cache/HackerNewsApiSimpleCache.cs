using HackerNewsApi.Api.Core.Models;
using HackerNewsApi.Application.Cache;

namespace HackerNewsApi.Api.Cache
{
    public class HackerNewsApiSimpleCache : IHackerNewsApiCache
    {
        private readonly object _lock = new();
        private IEnumerable<Story>? _cache = null;

        public void Refresh(IEnumerable<Story> stories)
        {
            lock (_lock)
            {
                _cache = stories;
            }
        }

        public IEnumerable<Story>? Get() 
        {
            lock (_lock)
            {
                return _cache;
            }
        }
    }
}
