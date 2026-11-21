using System.Diagnostics;
using Etl.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Etl.Application;

/// <summary>Ejecuta todos los extractores en paralelo y guarda lo extraído en staging.</summary>
public class ExtractionOrchestrator
{
    private readonly IEnumerable<IExtractor> _extractors;
    private readonly IStagingWriter _writer;
    private readonly ILogger<ExtractionOrchestrator> _logger;

    public ExtractionOrchestrator(
        IEnumerable<IExtractor> extractors,
        IStagingWriter writer,
        ILogger<ExtractionOrchestrator> logger)
    {
        _extractors = extractors;
        _writer = writer;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var total = Stopwatch.StartNew();
        _logger.LogInformation("Inicio de extracción con {Count} fuente(s).", _extractors.Count());

        await Task.WhenAll(_extractors.Select(e => RunSingleAsync(e, ct)));

        _logger.LogInformation("Extracción finalizada en {Ms} ms.", total.ElapsedMilliseconds);
    }

    private async Task RunSingleAsync(IExtractor extractor, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await extractor.ExtractAsync(ct);
            await _writer.WriteAsync(result, ct);
            _logger.LogInformation("[{Source}] {Count} registros extraídos en {Ms} ms.",
                extractor.SourceName, result.Records.Count, sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[{Source}] extracción cancelada.", extractor.SourceName);
        }
        catch (Exception ex)
        {
            // Un fallo en una fuente no detiene a las demás.
            _logger.LogError(ex, "[{Source}] error durante la extracción.", extractor.SourceName);
        }
    }
}
