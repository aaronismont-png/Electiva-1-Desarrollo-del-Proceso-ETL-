namespace Etl.Domain.Abstractions;

/// <summary>Contrato de cada cargador de dimensión del almacén de datos.</summary>
public interface IDimensionLoader
{
    string DimensionName { get; }
    Task<DimensionLoadResult> LoadAsync(CancellationToken cancellationToken);
}
