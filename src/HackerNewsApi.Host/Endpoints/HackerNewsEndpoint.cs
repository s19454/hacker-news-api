using HackerNewsApi.Application.Services;

namespace HackerNewsApi.Api.Endpoints
{
    public static class HackerNewsEndpoint
    {
        public static async Task<IResult> GetBestStories(int count, IStoriesService HackerNewsApiService, CancellationToken token)
        {
            try
            {
                return Results.Ok(await HackerNewsApiService.GetBestStoriesAsync(count, token).ConfigureAwait(false));
            }
            catch (Exception)
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}