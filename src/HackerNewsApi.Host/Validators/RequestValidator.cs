using HackerNewsApi.Application.Constants;

namespace HackerNewsApi.Api.Filters
{
    public class RequestValidator : IEndpointFilter
    {
        private readonly int _maxSize;

        public RequestValidator(IConfiguration configuration) 
        {
            _maxSize = configuration.GetValue<int>(CacheConstants.MaxSize);
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var arg = context.Arguments.SingleOrDefault(a => a?.GetType() == typeof(int));
            if (arg == null)
                return Results.BadRequest("Best stories count should be a number");

            if (!ValidateCount((int)arg))
                return Results.BadRequest("Best stories count should be between 1 and max of {_cacheMaxSize}");

            return await next(context);
        }

        private bool ValidateCount(int count) => count != 0 && count <= _maxSize;
    }
}
