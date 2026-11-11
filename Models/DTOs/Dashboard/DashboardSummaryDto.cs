namespace Models.DTOs.Dashboard;

public record DashboardSummaryDto
{
    public int ArticleCount { get; set; }
    public int ArticleCountIncDeleted { get; set; }

    public int SkuCount { get; set; }
    public decimal TotalStock { get; set; }
    public int InStockSkuCount { get; set; }
    public int OutOfStockSkuCount { get; set; }

    public int DiscountedSkuCount { get; set; }
    public int NonDiscountedSkuCount { get; set; }

    public int OutOfStockAvailableNow { get; set; }
    public int OutOfStockAvailableSoonSkuCount { get; set; }
    public int OutOfStockNotAvailable { get; set; }
}
