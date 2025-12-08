using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using HackerNewsApi.Application.Cache;
using HackerNewsApi.Api.Services;
using HackerNewsApi.Api.Core.Models;

namespace HackerNewsApi.Application.Tests.Stories
{
    public class StoriesServiceTests
    {
        private StoriesService CreateService(
            IHackerNewsApiCache cache,
            int retryDelay = 10,
            int maxAttempts = 3)
        {
            var logger = Mock.Of<ILogger<StoriesService>>();

            var inMemorySettings = new Dictionary<string, string>
        {
            { "Cache:RefreshMiliseconds", retryDelay.ToString() },
            { "Cache:MaxSize", maxAttempts.ToString() }
        };

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            return new StoriesService(cache, logger, config);
        }

        [Fact]
        public async Task ReturnsStories_WhenCacheIsAvailable()
        {
            var stories = new List<Story>
        {
            new Story { id = 1, score = 100 },
            new Story { id = 2, score = 99 }
        };

            var cache = new Mock<IHackerNewsApiCache>();
            cache.Setup(c => c.Get()).Returns(stories);

            var service = CreateService(cache.Object);

            var result = await service.GetBestStoriesAsync(1, CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(100, result.First().score);
        }

        [Fact]
        public async Task WaitsForCache_WhenInitiallyNull()
        {
            var stories = new List<Story>
        {
            new Story { id = 1, score = 10 }
        };

            int calls = 0;

            var cache = new Mock<IHackerNewsApiCache>();
            cache.Setup(c => c.Get()).Returns(() =>
            {
                calls++;
                return calls >= 2 ? stories : null;
            });

            var service = CreateService(cache.Object);

            var result = await service.GetBestStoriesAsync(5, CancellationToken.None);

            Assert.Equal(stories, result);
            Assert.Equal(2, calls);
        }

        [Fact]
        public async Task Throws_WhenCacheNeverPopulates()
        {
            var cache = new Mock<IHackerNewsApiCache>();
            cache.Setup(c => c.Get()).Returns(() => null);

            var service = CreateService(cache.Object, retryDelay: 1, maxAttempts: 2);

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await service.GetBestStoriesAsync(3, CancellationToken.None);
            });
        }

        [Fact]
        public async Task ReturnsFaultTask_WhenExceptionThrown()
        {
            var cache = new Mock<IHackerNewsApiCache>();
            cache.Setup(c => c.Get()).Throws(new InvalidOperationException("cache failure"));

            var service = CreateService(cache.Object);

            var result = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.GetBestStoriesAsync(3, CancellationToken.None);
            });
        }
    }
}