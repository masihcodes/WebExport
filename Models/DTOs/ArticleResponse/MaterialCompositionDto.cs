namespace Models.DTOs.ArticleResponse;

public record MaterialCompositionDto
{
    public required string MaterialType { get; set; }
    public required string Material { get; set; }
    public decimal Percentage { get; set; }
}
