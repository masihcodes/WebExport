using Models.Enums;

namespace Models.DTOs.ArticleResponse;

public record SizeChartDto
{
    public required string Size { get; set; }
    public required string Name { get; set; }
    public decimal Value { get; set; }
    public UnitOfMeasure Unit { get; set; }
}
