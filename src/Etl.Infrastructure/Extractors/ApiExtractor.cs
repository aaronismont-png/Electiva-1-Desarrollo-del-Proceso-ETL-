using System.Net.Http.Json;
using Etl.Application;
using Etl.Domain;
using Etl.Domain.Abstractions;
using Etl.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure.Extractors;

public class ApiExtractor : IExtractor
{
    public const string HttpClientName = "CommentsApi";

    private readonly IHttpClientFactory _factory;
    private readonly ApiSourceOptions _options;

    public ApiExtractor(IHttpClientFactory factory, IOptions<ExtractionOptions> options)
    {
        _factory = factory;
        _options = options.Value.Api;
    }

    public string SourceName => "API";

    public async Task<ExtractionResult> ExtractAsync(CancellationToken ct)
    {
        var client = _factory.CreateClient(HttpClientName);

        using var request = new HttpRequestMessage(HttpMethod.Get, _options.Endpoint);
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            request.Headers.Add("X-Api-Key", _options.ApiKey);

        using var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var comments = await response.Content.ReadFromJsonAsync<List<ApiComment>>(cancellationToken: ct)
                       ?? new List<ApiComment>();

        return new ExtractionResult(SourceName, comments.Cast<object>().ToList());
    }
}
