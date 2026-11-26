namespace Etl.Application;

public class DimensionLoadOptions
{
    public const string SectionName = "DimensionLoad";

    /// <summary>Base transaccional (opiniones_clientes). Va en User Secrets.</summary>
    public string SourceConnectionString { get; set; } = string.Empty;

    /// <summary>Almacén de datos (dw_opiniones_clientes). Va en User Secrets.</summary>
    public string TargetConnectionString { get; set; } = string.Empty;

    public string EventosPath { get; set; } = "Data/eventos.json";
    public string PalabrasClavePath { get; set; } = "Data/palabras_clave.json";
}
