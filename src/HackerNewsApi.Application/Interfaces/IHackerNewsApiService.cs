using HackerNewsApi.Api.Core.Models;

namespace HackerNewsApi.Api.Core.Interfaces
{
    public interface IHackerNewsApiService
    {
        Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken);

        Task<IEnumerable<int>> GetBestStoryIds(CancellationToken cancellationToken);
    }
}
