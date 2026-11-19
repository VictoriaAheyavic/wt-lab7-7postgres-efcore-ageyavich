namespace wt_lab7_7postgres_efcore_ageyavich.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}