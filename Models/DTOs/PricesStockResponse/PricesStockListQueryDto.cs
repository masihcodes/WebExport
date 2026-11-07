namespace Models.DTOs.PricesStockResponse;

public record PricesStockListQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 1000;
    public string? ArticleNumber { get; set; }
    public short? Branch { get; set; }
    public DateTimeOffset? ModifiedSince { get; set; }
    public bool IncludeDeleted { get; set; } = false;
}
