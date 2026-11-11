namespace Models.DTOs.Dashboard;

public record DashboardBranchStockDto
{
    public int BranchNumber { get; set; }
    public required string BranchName { get; set; }
    public decimal TotalStock { get; set; }
    public int InStockSkuCount { get; set; }
    public int OutOfStockSkuCount { get; set; }
}