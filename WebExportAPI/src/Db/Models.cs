using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Models.Enums;

namespace WebExportAPI.Db;

[Table("tenant")]
public class Tenant
{
    [Key]
    [Column("number")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short TenantNumber { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public ICollection<Branch>? Branches { get; set; } = []; // navigation property

    public ICollection<Article>? Articles { get; set; } = []; // navigation property>

    public ICollection<Category>? Categories { get; set; } = []; // navigation property

    public ICollection<ApiClient> ApiClients { get; set; } = []; // navigation property
}


[Table("color")]
[Index(nameof(Name), IsUnique = true)]
public class Color
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public required string Name { get; set; }

    [Range(0, 255)]
    [Column("red")]
    public short Red { get; set; }

    [Range(0, 255)]
    [Column("green")]
    public short Green { get; set; }

    [Range(0, 255)]
    [Column("blue")]
    public short Blue { get; set; }

    public ICollection<ArticleImage> ArticleImages { get; set; } = []; // navigation property
}

[Table("size")]
[Index(nameof(Name), IsUnique = true)]
public class Size
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short Id { get; set; }

    [Required]
    [MaxLength(10)]
    [Column("name")]
    public required string Name { get; set; }

    [Required]
    [MaxLength(30)]
    [Column("size_range")]
    public required string SizeRange { get; set; }

    [Column("sort_order")]
    public short SortOrder { get; set; }

    public ICollection<SizeChart> SizeCharts { get; set; } = []; // navigation property
}

[Table("branch")]
[Index(nameof(TenantNumber), nameof(Number), IsUnique = true)]
public class Branch
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("tenant_number")]
    [ForeignKey(nameof(Tenant))]
    public short TenantNumber { get; set; }

    public required Tenant Tenant { get; set; } // navigation property

    [Column("number")]
    public short Number { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("city")]
    public required string City { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public ICollection<BranchStock> BranchStocks { get; set; } = []; // navigation property
}

[Table("delivery_status_text")]
public class DeliveryStatusText
{
    [Key]
    [Column("status", TypeName = "delivery_status")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public DeliveryStatus Status { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("text")]
    public required string Text { get; set; }
}

[Table("material_type")]
[Index(nameof(Name), IsUnique = true)]
public class MaterialType
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public required string Name { get; set; }

    public ICollection<MaterialComposition> MaterialCompositions
    { get; set; } = []; // navigation property
}

[Table("material")]
[Index(nameof(Name), IsUnique = true)]
public class Material
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public required string Name { get; set; }

    public ICollection<MaterialComposition> MaterialCompositions { get; set; } = []; // navigation property
}

[Table("attribute_type")]
public class AttributeType
{
    [Key]
    [Column("number")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short Number { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public required string Name { get; set; }

    [Column("is_webshop_relevant")]
    public bool IsWebShopRelevant { get; set; } = true;

    public ICollection<Attribute> Attributes { get; set; } = []; // navigation property
}

[Table("attribute")]
[PrimaryKey(nameof(AttributeTypeNumber), nameof(Number))]
public class Attribute
{
    [Column("attribute_type_number")]
    [ForeignKey(nameof(AttributeType))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short AttributeTypeNumber { get; set; }

    public required AttributeType AttributeType { get; set; } // navigation property

    [Column("number")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short Number { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    public ICollection<ArticleAttribute> ArticleAttributes { get; set; } = []; // navigation property
}

[Table("category")]
[Index(nameof(TenantNumber), nameof(Number), IsUnique = true)]
public class Category
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(Tenant))]
    [Column("tenant_number")]
    public short TenantNumber { get; set; }

    public required Tenant Tenant { get; set; } // navigation property

    [Column("number")]
    public int Number { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    public Category? Parent { get; set; }

    public ICollection<Category> Children { get; set; } = [];

    [Column("rank")]
    public short Rank { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    [MaxLength(500)]
    [Column("search_terms")]
    public string? SearchTerms { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("modified_at")]
    public DateTimeOffset ModifiedAt { get; set; }

    public ICollection<ArticleCategory> ArticleCategories { get; set; } = []; // navigation property
}

[Table("article")]
[Index(nameof(TenantNumber), nameof(ArticleNumber), IsUnique = true)]
public class Article
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("tenant_number")]
    [ForeignKey(nameof(Tenant))]
    public short TenantNumber { get; set; }

    public required Tenant Tenant { get; set; } // navigation property

    [Required]
    [MaxLength(20)]
    [Column("article_number")]
    public required string ArticleNumber { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public required string Name { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("manufacturer")]
    public required string Manufacturer { get; set; }

    [MaxLength(100)]
    [Column("label")]
    public string? Label { get; set; }

    [MaxLength(500)]
    [Column("manufacturer_url")]
    public string? ManufacturerUrl { get; set; }

    [MaxLength(100)]
    [Column("manufacturer_url_text")]
    public string? ManufacturerUrlText { get; set; }

    [MaxLength(50)]
    [Column("delivery_time_text")]
    public string? DeliveryTimeText { get; set; }

    [Column("delivery_time_min_days")]
    public short? DeliveryTimeMinDays { get; set; }

    [Column("delivery_time_max_days")]
    public short? DeliveryTimeMaxDays { get; set; }

    [Precision(8, 2)]
    [Column("length_cm")]
    public decimal? LengthCm { get; set; }

    [Precision(8, 2)]
    [Column("width_cm")]
    public decimal? WidthCm { get; set; }

    [Precision(8, 2)]
    [Column("height_cm")]
    public decimal? HeightCm { get; set; }

    [Precision(8, 3)]
    [Column("weight_kg")]
    public decimal? WeightKg { get; set; }

    [Column("valid_from")]
    public DateOnly? ValidFrom { get; set; }

    [Column("valid_to")]
    public DateOnly? ValidTo { get; set; }

    [Column("is_searchable")]
    public bool IsSearchable { get; set; } = true;

    [Column("is_orderable")]
    public bool IsOrderable { get; set; } = true;

    [Column("is_discountable")]
    public bool IsDiscountable { get; set; } = true;

    [Column("is_discontinued")]
    public bool IsDiscontinued { get; set; } = false;

    [Column("is_free_shipping")]
    public bool IsFreeShipping { get; set; } = false;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    public required DateTimeOffset CreatedAt { get; set; }

    [Column("modified_at")]
    public DateTimeOffset ModifiedAt { get; set; }


    public ICollection<Sku> Skus { get; set; } = []; // navigation property

    public ICollection<ArticleCategory> ArticleCategories { get; set; } = []; // navigation property

    public ICollection<ArticleText> ArticleTexts { get; set; } = []; // navigation property

    public ICollection<ArticleImage> ArticleImages { get; set; } = []; // navigation property

    public ICollection<MaterialComposition> MaterialCompositions { get; set; } = []; // navigation property

    public ICollection<SizeChart> SizeCharts { get; set; } = []; // navigation property

    public ICollection<ArticleAttribute> ArticleAttributes { get; set; } = []; // navigation property

    [InverseProperty(nameof(ArticleRelation.Article))]
    public ICollection<ArticleRelation> OutgoingRelations { get; set; } = [];

    [InverseProperty(nameof(ArticleRelation.TargetArticle))]
    public ICollection<ArticleRelation> IncomingRelations { get; set; } = [];
}

[Table("sku")]
[Index(nameof(SkuNumber), IsUnique = true)]
[Index(nameof(ArticleId), nameof(ColorId), nameof(SizeId), nameof(Fit), IsUnique = true)]
public class Sku
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(30)]
    [Column("sku")]
    public required string SkuNumber { get; set; }

    [Column("article_id")]
    [ForeignKey(nameof(Article))]
    public long ArticleId { get; set; }

    public required Article Article { get; set; } // navigation property

    [Column("color_id")]
    [ForeignKey(nameof(Color))]
    public short ColorId { get; set; }

    public required Color Color { get; set; } // navigation property

    [Column("size_id")]
    [ForeignKey(nameof(Size))]
    public short SizeId { get; set; }

    public required Size Size { get; set; } // navigation property

    [MaxLength(30)]
    [Column("fit")]
    public string? Fit { get; set; }

    [MaxLength(50)]
    [Column("supplier_color")]
    public string? SupplierColor { get; set; }

    [Precision(10, 2)]
    [Column("sales_price")]
    public decimal SalesPrice { get; set; }

    [Precision(10, 2)]
    [Column("sales_price_reduced")]
    public decimal? SalesPriceReduced { get; set; }

    [Precision(12, 3)]
    [Column("stock")]
    public decimal Stock { get; set; }

    [Precision(12, 3)]
    [Column("quantity_available_now")]
    public decimal QuantityAvailableNow { get; set; }

    [Precision(12, 3)]
    [Column("quantity_available_soon")]
    public decimal QuantityAvailableSoon { get; set; }

    [Precision(12, 3)]
    [Column("quantity_not_available")]
    public decimal QuantityNotAvailable { get; set; }

    [Column("is_leading_color")]
    public bool IsLeadingColor { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("modified_at")]
    public DateTimeOffset ModifiedAt { get; set; }

    public ICollection<BranchStock> BranchStocks { get; set; } = []; // navigation property
}

[Table("branch_stock")]
[PrimaryKey(nameof(SkuId), nameof(BranchId))]
public class BranchStock
{
    [Column("sku_id")]
    [ForeignKey(nameof(Sku))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long SkuId { get; set; }

    public required Sku Sku { get; set; } // navigation property

    [Column("branch_id")]
    [ForeignKey(nameof(Branch))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int BranchId { get; set; }

    public required Branch Branch { get; set; } // navigation property

    [Precision(12, 3)]
    [Column("stock")]
    public decimal Stock { get; set; }

    [Column("modified_at")]
    public DateTimeOffset ModifiedAt { get; set; }
}

[Table("article_category")]
[PrimaryKey(nameof(ArticleId), nameof(CategoryId))]
public class ArticleCategory
{
    [Column("article_id")]
    [ForeignKey(nameof(Article))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long ArticleId { get; set; }

    public required Article Article { get; set; } // navigation property

    [Column("category_id")]
    [ForeignKey(nameof(Category))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CategoryId { get; set; }

    public required Category Category { get; set; } // navigation property

    [Column("rank")]
    public short Rank { get; set; }
}

[Table("article_text")]
public class ArticleText
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("article_id")]
    [ForeignKey(nameof(Article))]
    public long ArticleId { get; set; }

    public required Article Article { get; set; } // navigation property

    [Column("type", TypeName = "text_type")]
    public TextType Type { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("text")]
    public required string Text { get; set; }

    [Column("sort_order")]
    public short SortOrder { get; set; }
}

[Table("article_relation")]
[PrimaryKey(nameof(ArticleId), nameof(TargetArticleId), nameof(Type))]
public class ArticleRelation
{
    [Column("article_id")]
    [ForeignKey(nameof(Article))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long ArticleId { get; set; }

    public required Article Article { get; set; } // navigation property

    [Column("target_article_id")]
    [ForeignKey(nameof(TargetArticle))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long TargetArticleId { get; set; }

    public required Article TargetArticle { get; set; } // navigation property

    [Column("type", TypeName = "relation_type")]
    public RelationType Type { get; set; }
}


[Table("article_image")]
public class ArticleImage
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("article_id")]
    [ForeignKey(nameof(Article))]
    public long ArticleId { get; set; }

    public required Article Article { get; set; } // navigation property

    [Column("color_id")]
    [ForeignKey(nameof(Color))]
    public short? ColorId { get; set; }

    public Color? Color { get; set; }  // navigation property

    [Required]
    [MaxLength(200)]
    [Column("file_name")]
    public required string FileName { get; set; }

    [Column("type", TypeName = "image_type")]
    public ImageType Type { get; set; }

    [Column("sort_order")]
    public short SortOrder { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("modified_at")]
    public DateTimeOffset ModifiedAt { get; set; }
}

[Table("material_composition")]
[PrimaryKey(nameof(ArticleId), nameof(MaterialTypeId), nameof(MaterialId))]
public class MaterialComposition
{
    [Column("article_id")]
    [ForeignKey(nameof(Article))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long ArticleId { get; set; }

    public required Article Article { get; set; } // navigation property

    [Column("material_type_id")]
    [ForeignKey(nameof(MaterialType))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short MaterialTypeId { get; set; }

    public required MaterialType MaterialType { get; set; } // navigation property

    [Column("material_id")]
    [ForeignKey(nameof(Material))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short MaterialId { get; set; }

    public required Material Material { get; set; } // navigation property

    [Column("percentage")]
    [Precision(5, 2)]
    [Range(typeof(decimal), "0", "100", MinimumIsExclusive = true)]
    public decimal Percentage { get; set; }
}

[Table("size_chart")]
public class SizeChart
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("article_id")]
    [ForeignKey(nameof(Article))]
    public long ArticleId { get; set; }

    public required Article Article { get; set; } // navigation property

    [Column("size_id")]
    [ForeignKey(nameof(Size))]
    public short SizeId { get; set; }

    public required Size Size { get; set; } // navigation property

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public required string Name { get; set; }

    [Precision(8, 2)]
    [Column("value")]
    public decimal Value { get; set; }

    [Column("unit", TypeName = "unit_of_measure")]
    public UnitOfMeasure Unit { get; set; }
}

[Table("article_attribute")]
[PrimaryKey(nameof(ArticleId), nameof(AttributeTypeNumber), nameof(AttributeNumber))]
public class ArticleAttribute
{
    [Column("article_id")]
    [ForeignKey(nameof(Article))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long ArticleId { get; set; }

    public required Article Article { get; set; } // navigation property

    [Column("attribute_type_number")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short AttributeTypeNumber { get; set; }

    [Column("attribute_number")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short AttributeNumber { get; set; }

    [ForeignKey(nameof(AttributeTypeNumber) + "," + nameof(AttributeNumber))]
    public required Attribute Attribute { get; set; } // navigation property
}

[Table("api_client")]
[Index(nameof(ClientId), IsUnique = true)]
public class ApiClient
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("tenant_number")]
    [ForeignKey(nameof(Tenant))]
    public short TenantNumber { get; set; }

    public required Tenant Tenant { get; set; } // navigation property

    [Required]
    [MaxLength(50)]
    [Column("client_id")]
    public required string ClientId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("secret_hash")]
    public required string SecretHash { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_at")]
    public required DateTimeOffset CreatedAt { get; set; }

    [Column("last_login_at")]
    public DateTimeOffset? LastLoginAt { get; set; }
}