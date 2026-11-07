using Models.DTOs.ArticleResponse;

namespace Models.DTOs.PricesStockResponse;

public record PricesStockResponseDto
{
    public required string Sku { get; init; }
    public required string ArticleNumber { get; init; }
    public decimal SalesPrice { get; set; }
    public decimal? SalesPriceReduced { get; set; }
    public decimal Stock { get; set; }

    public QuantitiesDto Quantities { get; set; } = new();

    public bool IsDeleted { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}

