using Etl.Application;
using Etl.Domain;
using Etl.Domain.Abstractions;
using Etl.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure.Extractors;

public class DatabaseExtractor : IExtractor
{
    private readonly DatabaseSourceOptions _options;
    public DatabaseExtractor(IOptions<ExtractionOptions> options) => _options = options.Value.Database;

    public string SourceName => "DATABASE";

    public async Task<ExtractionResult> ExtractAsync(CancellationToken ct)
    {
        var records = new List<object>();

        await using var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(_options.Query, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        // El query debe devolver, en este orden:
        // Id, ProductId, CustomerId, Rating, Text, Date
        while (await reader.ReadAsync(ct))
        {
            records.Add(new Review
            {
                Id = reader.GetInt32(0),
                ProductId = reader.GetInt32(1),
                CustomerId = reader.GetInt32(2),
                Rating = reader.GetInt32(3),
                Text = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Date = reader.GetDateTime(5)
            });
        }
        return new ExtractionResult(SourceName, records);
    }
}
