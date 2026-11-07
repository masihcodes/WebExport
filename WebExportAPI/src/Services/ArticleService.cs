using Microsoft.EntityFrameworkCore;
using Models.DTOs;
using Models.DTOs.ArticleResponse;
using WebExportAPI.Db;

namespace WebExportAPI.Services;



public interface IArticleService
{
    Task<PagedResponseDto<ArticleResponseDto>> GetArticlesAsync(short tenantNumber, ArticleListQueryDto query, CancellationToken ct);
    Task<ArticleResponseDto?> GetArticleAsync(CancellationToken ct, short tenantNumber, string articleNumber, short? branch = null);
    Task<bool> BranchExistsAsync(short tenantNumber, short branchNumber, CancellationToken ct);
}



public class ArticleService(AppDbContext db) : IArticleService
{
    public async Task<PagedResponseDto<ArticleResponseDto>> GetArticlesAsync(short tenantNumber, ArticleListQueryDto query, CancellationToken ct)
    {
        var articlesQuery = db.Articles.Where(article => article.TenantNumber == tenantNumber);

        if (!query.IncludeDeleted)
            articlesQuery = articlesQuery.Where(article => !article.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.ArticleNumber))
            articlesQuery = articlesQuery.Where(article => article.ArticleNumber.Contains(query.ArticleNumber));


        if (query.Category.HasValue)
            articlesQuery = articlesQuery
                .Where(article => article.ArticleCategories
                    .Any(ac => ac.Category.Number == query.Category && ac.Category.TenantNumber == tenantNumber));

        if (query.ModifiedSince.HasValue)
        {
            DateTimeOffset modifiedSinceUtc = query.ModifiedSince.Value.ToUniversalTime();
            articlesQuery = articlesQuery.Where(article => article.ModifiedAt >= modifiedSinceUtc);
        }

        var totalCount = await articlesQuery.CountAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var offset = ((long)query.Page - 1) * query.PageSize;

        var pageQuery = articlesQuery
                                    .OrderBy(article => article.ArticleNumber)
                                    .ThenBy(article => article.Id)
                                    .Skip((int)Math.Min(offset, int.MaxValue))
                                    .Take(query.PageSize);

        List<ArticleResponseDto> pageArticles =
            offset >= totalCount ? [] : await ProjectArticles(pageQuery, tenantNumber, includeDeletedSkus: query.IncludeDeleted, branch: query.Branch).ToListAsync(ct);

        return new PagedResponseDto<ArticleResponseDto>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Data = pageArticles
        };

    }


    public async Task<ArticleResponseDto?> GetArticleAsync(CancellationToken ct, short tenantNumber, string articleNumber, short? branch = null)
    {
        IQueryable<Article> articleQuery = db.Articles.Where(article => article.TenantNumber == tenantNumber && article.ArticleNumber == articleNumber);

        IQueryable<ArticleResponseDto> res = ProjectArticles(articleQuery, tenantNumber, includeDeletedSkus: true, branch: branch);

        return await res.FirstOrDefaultAsync(ct);
    }


    public Task<bool> BranchExistsAsync(short tenantNumber, short branchNumber, CancellationToken ct)
    {
        return db.Branches.AnyAsync(b => b.TenantNumber == tenantNumber && b.Number == branchNumber, ct);
    }


    private IQueryable<ArticleResponseDto> ProjectArticles(IQueryable<Article> articlesQuery, short tenantNumber, bool includeDeletedSkus, short? branch)
    {
        articlesQuery = articlesQuery.AsSplitQuery();
        return articlesQuery.Select(article => new ArticleResponseDto
        {
            ArticleNumber = article.ArticleNumber,
            Name = article.Name,
            Description = article.Description,
            Label = article.Label,
            Manufacturer = article.Manufacturer,
            ManufacturerUrl = article.ManufacturerUrl,
            ManufacturerUrlText = article.ManufacturerUrlText,
            ValidFrom = article.ValidFrom,
            ValidTo = article.ValidTo,
            IsSearchable = article.IsSearchable,
            IsOrderable = article.IsOrderable,
            IsDiscountable = article.IsDiscountable,
            IsFreeShipping = article.IsFreeShipping,
            IsDeleted = article.IsDeleted,
            ModifiedAt = article.ModifiedAt,
            IsDiscontinued = article.IsDiscontinued,
            DeliveryTime = new DeliveryTimeDto
            {
                MaxDays = article.DeliveryTimeMaxDays,
                MinDays = article.DeliveryTimeMinDays,
                Text = article.DeliveryTimeText
            },
            ShippingDimensions = new ShippingDimensionsDto
            {
                LengthCm = article.LengthCm,
                WidthCm = article.WidthCm,
                HeightCm = article.HeightCm,
                WeightKg = article.WeightKg
            },
            Categories = article.ArticleCategories.Where(ac => ac.Category.TenantNumber == tenantNumber).Select(ac => new ArticleCategoryDto
            {
                Number = ac.Category.Number,
                Rank = ac.Rank
            }).ToList(),
            Texts = article.ArticleTexts.Select(at => new ArticleTextDto
            {
                Type = at.Type,
                SortOrder = at.SortOrder,
                Text = at.Text
            }).ToList(),
            Images = article.ArticleImages.Select(ai => new ArticleImageDto
            {
                ColorId = ai.ColorId,
                SortOrder = ai.SortOrder,
                Type = ai.Type,
                FileName = ai.FileName,
                IsDeleted = ai.IsDeleted,
            }).ToList(),
            MaterialComposition = article.MaterialCompositions.Select(ac => new MaterialCompositionDto
            {
                Material = ac.Material.Name,
                MaterialType = ac.MaterialType.Name,
                Percentage = ac.Percentage
            }).ToList(),
            SizeChart = article.SizeCharts.Select(sc => new SizeChartDto
            {
                Name = sc.Name,
                Size = sc.Size.Name,
                Value = sc.Value,
                Unit = sc.Unit
            }).ToList(),
            Attributes = article.ArticleAttributes.Select(aa => new ArticleAttributeDto
            {
                AttributeTypeName = aa.Attribute.AttributeType.Name,
                Name = aa.Attribute.Name,
                AttributeType = aa.AttributeTypeNumber,
                Number = aa.Attribute.Number,
            }).ToList(),

            Relations = article.OutgoingRelations.Where(or => or.TargetArticle.TenantNumber == tenantNumber).Select(or => new ArticleRelationDto
            {
                Type = or.Type,
                ArticleNumber = or.TargetArticle.ArticleNumber
            }).ToList(),
            Skus = article.Skus.Where(sku => includeDeletedSkus || !sku.IsDeleted).Select(sku => new SkuDto
            {
                Sku = sku.SkuNumber,
                Color = new ColorDto
                {
                    Name = sku.Color.Name,
                    Id = sku.Color.Id,
                    Blue = sku.Color.Blue,
                    Green = sku.Color.Green,
                    Red = sku.Color.Red
                },
                SupplierColor = sku.SupplierColor,
                Size = sku.Size.Name,
                Fit = sku.Fit,
                SalesPrice = sku.SalesPrice,
                SalesPriceReduced = sku.SalesPriceReduced,
                Stock = branch.HasValue ? (sku.BranchStocks
                            .Where(bs => bs.Branch.Number == branch.Value && bs.Branch.TenantNumber == tenantNumber)
                            .Select(bs => (decimal?)bs.Stock)
                            .FirstOrDefault() ?? 0m)
                        : sku.Stock,
                IsDeleted = sku.IsDeleted,
                IsLeadingColor = sku.IsLeadingColor,
                ModifiedAt = sku.ModifiedAt,
                Quantities = new QuantitiesDto
                {
                    AvailableNow = sku.QuantityAvailableNow,
                    AvailableSoon = sku.QuantityAvailableSoon,
                    NotAvailable = sku.QuantityNotAvailable
                }

            }).ToList()
        });
    }
}
