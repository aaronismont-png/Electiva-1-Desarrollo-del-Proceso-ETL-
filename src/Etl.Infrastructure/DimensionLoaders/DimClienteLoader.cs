using Etl.Application;
using Etl.Domain;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure.DimensionLoaders;

public class DimClienteLoader : DimensionLoaderBase
{
    private const string NoEspecificado = "No especificado";

    public DimClienteLoader(IOptions<DimensionLoadOptions> options) : base(options) { }

    public override string DimensionName => "dim_cliente";

    private record Row(int Origen, string Nombre, string? Correo, string Pais);

    private const string SelectSql = "SELECT id_cliente, nombre, apellido, correo, pais FROM cliente";

    // Al actualizar NO se tocan rango_edad ni tipo_cliente: la base transaccional no tiene esos datos
    // y no se deben pisar los valores que ya estén en el almacén.
    // El miembro "Cliente anónimo" (id 0, sin origen) nunca coincide, así que no se modifica.
    private const string UpsertSql = @"
UPDATE dim_cliente SET nombre = @nombre, correo = @correo, pais = @pais
WHERE id_cliente_origen = @origen;
IF @@ROWCOUNT = 0
BEGIN
    INSERT INTO dim_cliente (id_cliente_origen, nombre, correo, pais, rango_edad, tipo_cliente)
    VALUES (@origen, @nombre, @correo, @pais, @sinDato, @sinDato);
    SELECT 1;
END
ELSE SELECT 0;";

    public override async Task<DimensionLoadResult> LoadAsync(CancellationToken ct)
    {
        var rows = await ReadSourceAsync(SelectSql, r =>
        {
            var nombre = r.GetString(1);
            var apellido = r.IsDBNull(2) ? "" : r.GetString(2);
            var pais = r.IsDBNull(4) ? "" : r.GetString(4);
            return new Row(
                r.GetInt32(0),
                $"{nombre} {apellido}".Trim(),                       // transformación: nombre completo
                r.IsDBNull(3) ? null : r.GetString(3),
                string.IsNullOrWhiteSpace(pais) ? NoEspecificado : pais.Trim());
        }, ct);

        return await UpsertAllAsync(rows, UpsertSql, (p, x) =>
        {
            p.AddWithValue("@origen", x.Origen);
            p.AddWithValue("@nombre", x.Nombre);
            p.AddWithValue("@correo", Db(x.Correo));
            p.AddWithValue("@pais", x.Pais);
            p.AddWithValue("@sinDato", NoEspecificado);
        }, ct);
    }
}
