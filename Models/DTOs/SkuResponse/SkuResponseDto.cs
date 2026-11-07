using Models.DTOs.ArticleResponse;

namespace Models.DTOs.SkuResponse;

public record SkuResponseDto
{
    public required string Sku { get; init; }
    public required string ArticleNumber { get; init; }
    public required string ArticleName { get; init; }

    public required ColorDto Color { get; set; }

    public string? SupplierColor { get; init; }
    public required string Size { get; set; }
    public string? Fit { get; set; }
    public decimal SalesPrice { get; set; }
    public decimal? SalesPriceReduced { get; set; }
    public decimal Stock { get; set; }

    public QuantitiesDto Quantities { get; set; } = new();

    public bool IsLeadingColor { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }

    public List<BranchStockDto> BranchStock { get; set; } = [];
}



public record BranchStockDto
{
    public short Branch { get; init; }
    public required string BranchName { get; init; }
    public decimal Stock { get; init; }
}