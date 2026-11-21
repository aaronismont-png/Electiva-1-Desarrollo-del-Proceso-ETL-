namespace Etl.Domain.Entities;

/// <summary>Reseña de cliente (fuente BD relacional).</summary>
public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
