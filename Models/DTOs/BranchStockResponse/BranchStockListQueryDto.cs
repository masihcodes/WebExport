namespace Models.DTOs.BranchStockResponse;

public record BranchStockListQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 1000;
    public string? Sku { get; set; }
    public string? ArticleNumber { get; set; }
    public short? Branch { get; set; }
    public DateTimeOffset? ModifiedSince { get; set; }
}
