using System.Globalization;
using CsvHelper;
using Etl.Application;
using Etl.Domain;
using Etl.Domain.Abstractions;
using Etl.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure.Extractors;

public class CsvExtractor : IExtractor
{
    private readonly CsvSourceOptions _options;
    public CsvExtractor(IOptions<ExtractionOptions> options) => _options = options.Value.Csv;

    public string SourceName => "CSV";

    public async Task<ExtractionResult> ExtractAsync(CancellationToken ct)
    {
        if (!File.Exists(_options.Path))
            throw new FileNotFoundException("No se encontró el archivo CSV.", _options.Path);

        var records = new List<object>();
        using var reader = new StreamReader(_options.Path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await foreach (var row in csv.GetRecordsAsync<SurveyResponse>(ct))
        {
            if (row.Rating is < 1 or > 5) continue; // validación básica
            records.Add(row);
        }
        return new ExtractionResult(SourceName, records);
    }
}
