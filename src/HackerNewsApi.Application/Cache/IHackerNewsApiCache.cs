using HackerNewsApi.Api.Core.Models;

namespace HackerNewsApi.Application.Cache
{
    public interface IHackerNewsApiCache
    {
        void Refresh(IEnumerable<Story> stories);

        IEnumerable<Story>? Get();
    }
}
