namespace Models.DTOs.ArticleResponse;

public record ArticleResponseDto
{
    public required string ArticleNumber { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public required string Manufacturer { get; set; }
    public string? Label { get; set; }
    public string? ManufacturerUrl { get; set; }
    public string? ManufacturerUrlText { get; set; }

    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }

    public bool IsSearchable { get; set; }
    public bool IsOrderable { get; set; }
    public bool IsDiscountable { get; set; }
    public bool IsDiscontinued { get; set; }
    public bool IsFreeShipping { get; set; }
    public bool IsDeleted { get; set; }

    public DateTimeOffset ModifiedAt { get; set; }

    public DeliveryTimeDto DeliveryTime { get; set; } = new();
    public ShippingDimensionsDto ShippingDimensions { get; set; } = new();
    public List<ArticleCategoryDto> Categories { get; set; } = [];
    public List<ArticleTextDto> Texts { get; set; } = [];
    public List<ArticleRelationDto> Relations { get; set; } = [];
    public List<ArticleImageDto> Images { get; set; } = [];
    public List<MaterialCompositionDto> MaterialComposition { get; set; } = [];
    public List<SizeChartDto> SizeChart { get; set; } = [];
    public List<ArticleAttributeDto> Attributes { get; set; } = [];
    public List<SkuDto> Skus { get; set; } = [];
}
