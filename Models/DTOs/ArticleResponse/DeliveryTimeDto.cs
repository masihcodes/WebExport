namespace Models.DTOs.ArticleResponse;

public record DeliveryTimeDto
{
    public string? Text { get; set; }
    public short? MinDays { get; set; }
    public short? MaxDays { get; set; }
}