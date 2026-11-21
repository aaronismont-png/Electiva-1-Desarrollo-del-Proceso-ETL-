using Etl.Application;
using Microsoft.Extensions.Options;

namespace Etl.Worker;

public class Worker : BackgroundService
{
    private readonly ExtractionOrchestrator _orchestrator;
    private readonly ExtractionOptions _options;
    private readonly ILogger<Worker> _logger;

    public Worker(ExtractionOrchestrator orchestrator, IOptions<ExtractionOptions> options, ILogger<Worker> logger)
    {
        _orchestrator = orchestrator;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker ETL iniciado. Intervalo: {Min} min.", _options.IntervalMinutes);

        await _orchestrator.RunAsync(stoppingToken); // primera ejecución inmediata

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await _orchestrator.RunAsync(stoppingToken);
    }
}
