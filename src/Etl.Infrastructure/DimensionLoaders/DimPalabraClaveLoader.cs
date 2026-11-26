using System.Text.Json;
using Etl.Application;
using Etl.Domain;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure.DimensionLoaders;

/// <summary>Catálogo de palabras clave: no existe en la base transaccional, se lee de Data/palabras_clave.json.</summary>
public class DimPalabraClaveLoader : DimensionLoaderBase
{
    public DimPalabraClaveLoader(IOptions<DimensionLoadOptions> options) : base(options) { }

    public override string DimensionName => "dim_palabra_clave";

    private const string UpsertSql = @"
IF EXISTS (SELECT 1 FROM dim_palabra_clave WHERE palabra = @palabra)
    SELECT 0;
ELSE
BEGIN
    INSERT INTO dim_palabra_clave (palabra) VALUES (@palabra);
    SELECT 1;
END";

    public override async Task<DimensionLoadResult> LoadAsync(CancellationToken ct)
    {
        var json = await File.ReadAllTextAsync(Options.PalabrasClavePath, ct);
        var palabras = (JsonSerializer.Deserialize<List<string>>(json) ?? new())
            .Select(p => p.Trim().ToLowerInvariant())
            .Where(p => p.Length > 0)
            .Distinct()
            .ToList();

        return await UpsertAllAsync(palabras, UpsertSql, (p, x) => p.AddWithValue("@palabra", x), ct);
    }
}
