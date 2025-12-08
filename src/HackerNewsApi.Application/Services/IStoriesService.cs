using HackerNewsApi.Api.Core.Models;

namespace HackerNewsApi.Application.Services
{
    public interface IStoriesService
    {
        Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken token);
    }
}
