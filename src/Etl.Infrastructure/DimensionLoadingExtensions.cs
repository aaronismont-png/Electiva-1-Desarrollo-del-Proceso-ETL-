using Etl.Application;
using Etl.Domain.Abstractions;
using Etl.Infrastructure.DimensionLoaders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Etl.Infrastructure;

public static class DimensionLoadingExtensions
{
    /// <summary>El orden de registro es el orden de ejecución (dim_evento después de dim_producto).</summary>
    public static IServiceCollection AddDimensionLoading(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<DimensionLoadOptions>(config.GetSection(DimensionLoadOptions.SectionName));

        services.AddSingleton<IDimensionLoader, DimSentimientoLoader>();
        services.AddSingleton<IDimensionLoader, DimFuenteLoader>();
        services.AddSingleton<IDimensionLoader, DimProductoLoader>();
        services.AddSingleton<IDimensionLoader, DimClienteLoader>();
        services.AddSingleton<IDimensionLoader, DimPalabraClaveLoader>();
        services.AddSingleton<IDimensionLoader, DimEventoLoader>();

        services.AddSingleton<DimensionLoadOrchestrator>();
        return services;
    }
}
