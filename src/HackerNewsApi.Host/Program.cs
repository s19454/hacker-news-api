using HackerNewsApi.Api.Cache;
using HackerNewsApi.Api.Core.Interfaces;
using HackerNewsApi.Api.Endpoints;
using HackerNewsApi.Api.Filters;
using HackerNewsApi.Api.Services;
using HackerNewsApi.Application.Cache;
using HackerNewsApi.Application.Services;
using HackerNewsApi.Host.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHttpClient("HackerNews", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("HackerNewsApi:Url")!);
});

builder.Services.AddSingleton<IHackerNewsApiCache, HackerNewsApiSimpleCache>();
builder.Services.AddSingleton<IHackerNewsApiService, HackerNewsApiService>();
builder.Services.AddScoped<IStoriesService, StoriesService>();
builder.Services.AddHostedService<HackerNewsApiBackgroundService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HN API v1");
});

app.MapGet("hackernews/stories/best/{count:int}", HackerNewsEndpoint.GetBestStories).AddEndpointFilter<RequestValidator>();

app.Run();

internal partial class Program { }