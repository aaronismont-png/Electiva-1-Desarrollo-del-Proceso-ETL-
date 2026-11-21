namespace Etl.Domain;

/// <summary>Resultado de una extracción: nombre de la fuente y sus registros.</summary>
public record ExtractionResult(string Source, IReadOnlyCollection<object> Records);
