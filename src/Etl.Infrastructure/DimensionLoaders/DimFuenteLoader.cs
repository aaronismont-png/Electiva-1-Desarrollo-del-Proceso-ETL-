using Etl.Application;
using Etl.Domain;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure.DimensionLoaders;

public class DimFuenteLoader : DimensionLoaderBase
{
    public DimFuenteLoader(IOptions<DimensionLoadOptions> options) : base(options) { }

    public override string DimensionName => "dim_fuente";

    private record Row(string Tipo, string Canal);

    // El tipo de fuente se deduce de la tabla de detalle donde aparece cada opinión.
    private const string SelectSql = @"
SELECT DISTINCT
    CASE WHEN e.id_opinion IS NOT NULL THEN N'Encuesta'
         WHEN r.id_opinion IS NOT NULL THEN N'Reseña web'
         WHEN s.id_opinion IS NOT NULL THEN N'Red social'
         ELSE N'Otro' END AS tipo_fuente,
    o.canal
FROM opinion o
LEFT JOIN encuesta e          ON e.id_opinion = o.id_opinion
LEFT JOIN resena_web r        ON r.id_opinion = o.id_opinion
LEFT JOIN comentario_social s ON s.id_opinion = o.id_opinion";

    // La clave natural es (tipo_fuente, canal): si ya existe no hay nada que actualizar.
    private const string UpsertSql = @"
IF EXISTS (SELECT 1 FROM dim_fuente WHERE tipo_fuente = @tipo AND canal = @canal)
    SELECT 0;
ELSE
BEGIN
    INSERT INTO dim_fuente (tipo_fuente, canal) VALUES (@tipo, @canal);
    SELECT 1;
END";

    public override async Task<DimensionLoadResult> LoadAsync(CancellationToken ct)
    {
        var rows = await ReadSourceAsync(SelectSql, r => new Row(r.GetString(0), r.GetString(1).Trim()), ct);

        return await UpsertAllAsync(rows, UpsertSql, (p, x) =>
        {
            p.AddWithValue("@tipo", x.Tipo);
            p.AddWithValue("@canal", x.Canal);
        }, ct);
    }
}
