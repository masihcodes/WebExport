namespace Models.DTOs.ArticleResponse;

public record ShippingDimensionsDto
{
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
}