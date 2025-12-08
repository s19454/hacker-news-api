using HackerNewsApi.Api.Core.Interfaces;
using HackerNewsApi.Api.Core.Models;
using System.Text.Json;

namespace HackerNewsApi.Host.Services
{
    public class HackerNewsApiService : IHackerNewsApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HackerNewsApiService> _logger;

        public const string HackerNews = "HackerNews";
        private const string BestStoriesEndpoint = "beststories.json";

        public HackerNewsApiService(IHttpClientFactory httpClientFactory, ILogger<HackerNewsApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<int>> GetBestStoryIds(CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _httpClientFactory.CreateClient(HackerNews).GetAsync(BestStoriesEndpoint, cancellationToken);
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var ids = await JsonSerializer.DeserializeAsync<IEnumerable<int>>(stream, JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false);
                return ids ?? throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when retrieving stories");
                return await Task.FromException<IEnumerable<int>>(ex);
            }
        }

        public async Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _httpClientFactory.CreateClient(HackerNews).GetAsync($"item/{id}.json", cancellationToken);
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var details = await JsonSerializer.DeserializeAsync<Story>(stream, JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false);
                return details ?? throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                string message = $"Error when retrieving details of story with id {id}";
                _logger.LogError(ex, message);
                return await Task.FromException<Story>(ex);
            }
        }
    }
}