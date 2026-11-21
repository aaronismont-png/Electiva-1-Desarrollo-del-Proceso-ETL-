namespace Etl.Domain.Entities;

/// <summary>Encuesta interna (fuente CSV).</summary>
public class SurveyResponse
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
