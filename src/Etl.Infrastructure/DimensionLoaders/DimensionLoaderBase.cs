using Etl.Application;
using Etl.Domain;
using Etl.Domain.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure.DimensionLoaders;

/// <summary>Lógica común: leer del origen y aplicar "actualizar si existe, insertar si no" en el almacén.</summary>
public abstract class DimensionLoaderBase : IDimensionLoader
{
    protected DimensionLoadOptions Options { get; }

    protected DimensionLoaderBase(IOptions<DimensionLoadOptions> options) => Options = options.Value;

    public abstract string DimensionName { get; }
    public abstract Task<DimensionLoadResult> LoadAsync(CancellationToken ct);

    /// <summary>Extrae filas de la base transaccional.</summary>
    protected async Task<List<T>> ReadSourceAsync<T>(string sql, Func<SqlDataReader, T> map, CancellationToken ct)
    {
        var rows = new List<T>();
        await using var conn = new SqlConnection(Options.SourceConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            rows.Add(map(reader));
        return rows;
    }

    /// <summary>
    /// Carga todas las filas en una sola transacción. El SQL de cada fila debe devolver
    /// 1 si insertó y 0 si actualizó. Si algo falla, no se guarda nada de esta dimensión.
    /// </summary>
    protected async Task<DimensionLoadResult> UpsertAllAsync<T>(
        IEnumerable<T> rows, string upsertSql, Action<SqlParameterCollection, T> bind, CancellationToken ct)
    {
        int inserted = 0, updated = 0;

        await using var conn = new SqlConnection(Options.TargetConnectionString);
        await conn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(ct);

        foreach (var row in rows)
        {
            await using var cmd = new SqlCommand(upsertSql, conn, tx);
            bind(cmd.Parameters, row);
            var result = await cmd.ExecuteScalarAsync(ct);
            if (Convert.ToInt32(result) == 1) inserted++; else updated++;
        }

        await tx.CommitAsync(ct);
        return new DimensionLoadResult(DimensionName, inserted, updated);
    }

    protected static object Db(object? value) => value ?? DBNull.Value;
}
