using System.Text.Json;
using Etl.Application;
using Etl.Domain;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure.DimensionLoaders;

/// <summary>Catálogo de eventos: no existe en la base transaccional, se lee de Data/eventos.json.</summary>
public class DimEventoLoader : DimensionLoaderBase
{
    public DimEventoLoader(IOptions<DimensionLoadOptions> options) : base(options) { }

    public override string DimensionName => "dim_evento";

    private record EventoJson(string Nombre, string TipoEvento, DateTime FechaInicio, DateTime FechaFin, int? IdProductoOrigen);

    // El producto se resuelve por su id de origen contra dim_producto (por eso este loader va después de dim_producto).
    private const string UpsertSql = @"
UPDATE dim_evento
SET tipo_evento = @tipo, fecha_inicio = @inicio, fecha_fin = @fin,
    id_producto = (SELECT id_producto FROM dim_producto WHERE id_producto_origen = @prod)
WHERE nombre = @nombre;
IF @@ROWCOUNT = 0
BEGIN
    INSERT INTO dim_evento (nombre, tipo_evento, fecha_inicio, fecha_fin, id_producto)
    VALUES (@nombre, @tipo, @inicio, @fin,
            (SELECT id_producto FROM dim_producto WHERE id_producto_origen = @prod));
    SELECT 1;
END
ELSE SELECT 0;";

    public override async Task<DimensionLoadResult> LoadAsync(CancellationToken ct)
    {
        var json = await File.ReadAllTextAsync(Options.EventosPath, ct);
        var eventos = JsonSerializer.Deserialize<List<EventoJson>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        return await UpsertAllAsync(eventos, UpsertSql, (p, x) =>
        {
            p.AddWithValue("@nombre", x.Nombre);
            p.AddWithValue("@tipo", x.TipoEvento);
            p.AddWithValue("@inicio", x.FechaInicio.Date);
            p.AddWithValue("@fin", x.FechaFin.Date);
            p.AddWithValue("@prod", Db(x.IdProductoOrigen));
        }, ct);
    }
}
