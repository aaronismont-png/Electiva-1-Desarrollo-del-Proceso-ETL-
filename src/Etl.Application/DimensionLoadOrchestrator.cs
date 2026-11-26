using System.Diagnostics;
using Etl.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Etl.Application;

/// <summary>
/// Ejecuta los cargadores de dimensiones en el orden en que fueron registrados.
/// El orden importa: dim_evento referencia a dim_producto, por eso se carga después.
/// </summary>
public class DimensionLoadOrchestrator
{
    private readonly IEnumerable<IDimensionLoader> _loaders;
    private readonly ILogger<DimensionLoadOrchestrator> _logger;

    public DimensionLoadOrchestrator(IEnumerable<IDimensionLoader> loaders, ILogger<DimensionLoadOrchestrator> logger)
    {
        _loaders = loaders;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var total = Stopwatch.StartNew();
        _logger.LogInformation("Inicio de carga de {Count} dimensión(es).", _loaders.Count());

        foreach (var loader in _loaders)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var r = await loader.LoadAsync(ct);
                _logger.LogInformation("[{Dim}] {Ins} nuevas, {Upd} existentes actualizadas en {Ms} ms.",
                    r.Dimension, r.Inserted, r.Updated, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                // Un error en una dimensión no detiene la carga de las demás.
                _logger.LogError(ex, "[{Dim}] error durante la carga.", loader.DimensionName);
            }
        }

        _logger.LogInformation("Carga de dimensiones finalizada en {Ms} ms.", total.ElapsedMilliseconds);
    }
}
