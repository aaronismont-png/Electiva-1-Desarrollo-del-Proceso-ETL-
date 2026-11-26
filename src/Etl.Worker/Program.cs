using Etl.Application;
using Etl.Infrastructure;
using Etl.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddExtraction(builder.Configuration);
builder.Services.AddDimensionLoading(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// dotnet run -- --load-dimensions  → carga las dimensiones una sola vez y termina.
if (args.Contains("--load-dimensions"))
{
    var orchestrator = host.Services.GetRequiredService<DimensionLoadOrchestrator>();
    await orchestrator.RunAsync(CancellationToken.None);
    return;
}

host.Run();
