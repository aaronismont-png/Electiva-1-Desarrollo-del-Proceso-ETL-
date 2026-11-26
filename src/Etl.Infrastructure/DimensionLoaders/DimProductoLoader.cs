using Etl.Application;
using Etl.Domain;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure.DimensionLoaders;

public class DimProductoLoader : DimensionLoaderBase
{
    public DimProductoLoader(IOptions<DimensionLoadOptions> options) : base(options) { }

    public override string DimensionName => "dim_producto";

    private record Row(int Origen, string Nombre, string Categoria, decimal? Precio);

    private const string SelectSql = @"
SELECT p.id_producto, p.nombre, c.nombre AS categoria, p.precio
FROM producto p
JOIN categoria c ON c.id_categoria = p.id_categoria";

    private const string UpsertSql = @"
UPDATE dim_producto SET nombre = @nombre, categoria = @categoria, precio = @precio
WHERE id_producto_origen = @origen;
IF @@ROWCOUNT = 0
BEGIN
    INSERT INTO dim_producto (id_producto_origen, nombre, categoria, precio)
    VALUES (@origen, @nombre, @categoria, @precio);
    SELECT 1;
END
ELSE SELECT 0;";

    public override async Task<DimensionLoadResult> LoadAsync(CancellationToken ct)
    {
        var rows = await ReadSourceAsync(SelectSql,
            r => new Row(r.GetInt32(0), r.GetString(1), r.GetString(2), r.IsDBNull(3) ? null : r.GetDecimal(3)), ct);

        return await UpsertAllAsync(rows, UpsertSql, (p, x) =>
        {
            p.AddWithValue("@origen", x.Origen);
            p.AddWithValue("@nombre", x.Nombre);
            p.AddWithValue("@categoria", x.Categoria);
            p.AddWithValue("@precio", Db(x.Precio));
        }, ct);
    }
}
