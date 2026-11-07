namespace Models.DTOs.ArticleResponse;


public record SkuDto
{
    public required string Sku { get; set; }

    public required ColorDto Color { get; set; }

    public string? SupplierColor { get; set; }
    public required string Size { get; set; }
    public string? Fit { get; set; }
    public decimal SalesPrice { get; set; }
    public decimal? SalesPriceReduced { get; set; }
    public decimal Stock { get; set; }

    public QuantitiesDto Quantities { get; set; } = new();

    public bool IsLeadingColor { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}


public record ColorDto
{
    public short Id { get; set; }
    public required string Name { get; set; }
    public short Red { get; set; }
    public short Green { get; set; }
    public short Blue { get; set; }
}

public record QuantitiesDto
{
    public decimal AvailableNow { get; set; }
    public decimal AvailableSoon { get; set; }
    public decimal NotAvailable { get; set; }
}