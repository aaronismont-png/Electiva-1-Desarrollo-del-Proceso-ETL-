namespace Etl.Domain;

/// <summary>Resultado de cargar una dimensión: filas nuevas y filas existentes (actualizadas).</summary>
public record DimensionLoadResult(string Dimension, int Inserted, int Updated);
