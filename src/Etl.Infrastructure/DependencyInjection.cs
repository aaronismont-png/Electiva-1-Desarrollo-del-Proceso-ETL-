using Etl.Application;
using Etl.Domain.Abstractions;
using Etl.Infrastructure.Extractors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Etl.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra solo las fuentes habilitadas en appsettings.json.</summary>
    public static IServiceCollection AddExtraction(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection(ExtractionOptions.SectionName);
        services.Configure<ExtractionOptions>(section);
        var opts = section.Get<ExtractionOptions>() ?? new ExtractionOptions();

        if (opts.Csv.Enabled)
            services.AddSingleton<IExtractor, CsvExtractor>();

        if (opts.Database.Enabled)
            services.AddSingleton<IExtractor, DatabaseExtractor>();

        if (opts.Api.Enabled)
        {
            services.AddHttpClient(ApiExtractor.HttpClientName, c =>
            {
                c.BaseAddress = new Uri(opts.Api.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddSingleton<IExtractor, ApiExtractor>();
        }

        services.AddSingleton<IStagingWriter, StagingWriter>();
        services.AddSingleton<ExtractionOrchestrator>();
        return services;
    }
}
