using Etl.Infrastructure;
using Etl.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddExtraction(builder.Configuration);
builder.Services.AddHostedService<Worker>();

builder.Build().Run();
