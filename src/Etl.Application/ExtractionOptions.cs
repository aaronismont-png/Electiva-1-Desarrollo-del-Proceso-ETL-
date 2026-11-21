namespace Etl.Application;

public class ExtractionOptions
{
    public const string SectionName = "Extraction";

    public int IntervalMinutes { get; set; } = 30;
    public string StagingPath { get; set; } = "Staging";
    public CsvSourceOptions Csv { get; set; } = new();
    public DatabaseSourceOptions Database { get; set; } = new();
    public ApiSourceOptions Api { get; set; } = new();
}

public class CsvSourceOptions
{
    public bool Enabled { get; set; } = true;
    public string Path { get; set; } = "Data/encuestas.csv";
}

public class DatabaseSourceOptions
{
    public bool Enabled { get; set; } = true;
    public string ConnectionString { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
}

public class ApiSourceOptions
{
    public bool Enabled { get; set; } = true;
    public string BaseUrl { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
