namespace Etl.Domain.Abstractions;

/// <summary>Contrato común de todas las fuentes de datos.</summary>
public interface IExtractor
{
    string SourceName { get; }
    Task<ExtractionResult> ExtractAsync(CancellationToken cancellationToken);
}
