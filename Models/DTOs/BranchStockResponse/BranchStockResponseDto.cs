namespace Models.DTOs.BranchStockResponse;

public record BranchStockResponseDto
{
    public required string Sku { get; init; }
    public required string ArticleNumber { get; init; }
    public short Branch { get; init; }
    public decimal Stock { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
