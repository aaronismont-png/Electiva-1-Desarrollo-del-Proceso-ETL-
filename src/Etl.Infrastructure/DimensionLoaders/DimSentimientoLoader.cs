using Etl.Application;
using Etl.Domain;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure.DimensionLoaders;

public class DimSentimientoLoader : DimensionLoaderBase
{
    private const int SinClasificarId = 4; // miembro especial para opiniones sin sentimiento

    public DimSentimientoLoader(IOptions<DimensionLoadOptions> options) : base(options) { }

    public override string DimensionName => "dim_sentimiento";

    private record Row(int Id, string Nombre, short? Valor);

    private const string SelectSql = "SELECT id_sentimiento, nombre FROM sentimiento";

    private const string UpsertSql = @"
UPDATE dim_sentimiento SET nombre = @nombre, valor_numerico = @valor WHERE id_sentimiento = @id;
IF @@ROWCOUNT = 0
BEGIN
    INSERT INTO dim_sentimiento (id_sentimiento, nombre, valor_numerico) VALUES (@id, @nombre, @valor);
    SELECT 1;
END
ELSE SELECT 0;";

    public override async Task<DimensionLoadResult> LoadAsync(CancellationToken ct)
    {
        var rows = await ReadSourceAsync(SelectSql,
            r => new Row(r.GetInt32(0), r.GetString(1).Trim(), ValorNumerico(r.GetString(1))), ct);

        if (rows.All(x => x.Id != SinClasificarId))
            rows.Add(new Row(SinClasificarId, "Sin clasificar", null));

        return await UpsertAllAsync(rows, UpsertSql, (p, x) =>
        {
            p.AddWithValue("@id", (byte)x.Id);
            p.AddWithValue("@nombre", x.Nombre);
            p.AddWithValue("@valor", Db(x.Valor));
        }, ct);
    }

    // Transformación: el valor numérico se deduce del nombre del sentimiento.
    private static short? ValorNumerico(string nombre)
    {
        var n = nombre.Trim().ToLowerInvariant();
        if (n.StartsWith("pos")) return 1;
        if (n.StartsWith("neu")) return 0;
        if (n.StartsWith("neg")) return -1;
        return null;
    }
}
