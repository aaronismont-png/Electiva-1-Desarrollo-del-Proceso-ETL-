using System.Text.Json;
using Etl.Application;
using Etl.Domain;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure;

/// <summary>Guarda cada extracción como archivo JSON temporal (staging).</summary>
public class StagingWriter : IStagingWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _path;

    public StagingWriter(IOptions<ExtractionOptions> options) => _path = options.Value.StagingPath;

    public async Task WriteAsync(ExtractionResult result, CancellationToken ct)
    {
        Directory.CreateDirectory(_path);
        var file = Path.Combine(_path, $"{result.Source.ToLower()}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        await using var stream = File.Create(file);
        await JsonSerializer.SerializeAsync(stream, result.Records, JsonOptions, ct);
    }
}
