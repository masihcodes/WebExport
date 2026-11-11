using WebExportAPI.Db;
using Models.DTOs.Dashboard;
using Microsoft.EntityFrameworkCore;


namespace WebExportAPI.src.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(short tenantNumber, CancellationToken ct);
    Task<List<DashboardBranchStockDto>> GetStockByBranchAsync(short tenantNumber, CancellationToken ct);
    Task<List<DashboardRecentArticleDto>> GetRecentArticlesAsync(short tenantNumber, CancellationToken ct);
    Task<List<DashboardTopCategoryDto>> GetTopCategoriesAsync(short tenantNumber, CancellationToken ct);
    Task<DashboardContentQualityDto> GetContentQualityAsync(short tenantNumber, CancellationToken ct);
}

public class DashboardService(AppDbContext db) : IDashboardServ‍ice
{
    public async Task<DashboardContentQualityDto> GetContentQualityAsync(short tenantNumber, CancellationToken ct)
    {
        var articlesQuery = db.Articles.Where(a => a.TenantNumber == tenantNumber && !a.IsDeleted);

        // int noDiscretion = await articlesQuery.Where(a => !a.ArticleImages.Any(ai => !ai.IsDeleted)).CountAsync(ct);
        // int noImage = await articlesQuery.Where(a => !a.ArticleTexts.Any(at => string.IsNullOrWhiteSpace(at.Text))).CountAsync(ct);
        // int noActiveCategory = await articlesQuery.Where(a => !a.ArticleCategories.Any(ac => ac.Category.IsActive && ac.Category.TenantNumber == tenantNumber)).CountAsync(ct);

        int withoutDescription = await articlesQuery.CountAsync(a => string.IsNullOrWhiteSpace(a.Description), ct);
        int withoutImage = await articlesQuery.CountAsync(a => !a.ArticleImages.Any(ai => !ai.IsDeleted), ct);
        int withoutActiveCategory = await articlesQuery.CountAsync(a => !a.ArticleCategories.Any(ac => ac.Category.IsActive && ac.Category.TenantNumber == tenantNumber), ct);


        var res = new DashboardContentQualityDto
        {
            ArticlesWithoutDescriptionCount = withoutDescription,
            ArticlesWithoutImageCount = withoutImage,
            ArticlesWithoutActiveCategoryCount = withoutActiveCategory
        };

        return res;
    }

    public Task<List<DashboardRecentArticleDto>> GetRecentArticlesAsync(short tenantNumber, CancellationToken ct)
    {
        return db.Articles
            .Where(a => a.TenantNumber == tenantNumber && !a.IsDeleted)
            .OrderByDescending(a => a.ModifiedAt)
            .ThenBy(a => a.ArticleNumber)
            .Take(5)
            .Select(a => new DashboardRecentArticleDto
            {
                ArticleNumber = a.ArticleNumber,
                Name = a.Name,
                ModifiedAt = a.ModifiedAt
            })
            .ToListAsync(ct);
    }

    public async Task<List<DashboardBranchStockDto>> GetStockByBranchAsync(short tenantNumber, CancellationToken ct)
    {
        var stockByBranch = await db.BranchStocks
        .Where(bs =>
                    bs.Branch.TenantNumber == tenantNumber &&
                    bs.Sku.Article.TenantNumber == tenantNumber &&
                    !bs.Sku.IsDeleted && !
                    bs.Sku.Article.IsDeleted)
        .GroupBy(bs => bs.BranchId)
        .Select(g => new DashboardBranchStockDto
        {
            BranchNumber = db.Branches.Where(b => b.Id == g.Key).Select(b => b.Number).First(),
            BranchName = db.Branches.First(b => b.Id == g.Key).Name,
            TotalStock = g.Sum(bs => bs.Stock),
            OutOfStockSkuCount = g.Count(bs => bs.Stock <= 0),
            InStockSkuCount = g.Count(bs => bs.Stock > 0),
        })
        .ToListAsync(ct);

        return stockByBranch;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(short tenantNumber, CancellationToken ct)
    {
        var articlesQuery = db.Articles.Where(a => a.TenantNumber == tenantNumber);
        var skusQuery = db.Skus.Where(s => s.Article.TenantNumber == tenantNumber && !s.IsDeleted && !s.Article.IsDeleted);


        var discountedSkuCount = await skusQuery.Where(s => s.SalesPriceReduced.HasValue).CountAsync(ct);
        int skuCount = await skusQuery.CountAsync(ct);

        return new DashboardSummaryDto
        {
            ArticleCount = await articlesQuery.Where(a => !a.IsDeleted).CountAsync(ct),
            ArticleCountIncDeleted = await articlesQuery.CountAsync(ct),
            SkuCount = skuCount,
            TotalStock = await skusQuery.SumAsync(x => x.Stock, ct),
            OutOfStockSkuCount = await skusQuery.CountAsync(s => s.Stock <= 0, ct),
            InStockSkuCount = await skusQuery.CountAsync(s => s.Stock > 0, ct),
            OutOfStockAvailableNow = await skusQuery.CountAsync(s => s.Stock <= 0 && s.QuantityAvailableNow > 0, ct),
            OutOfStockAvailableSoonSkuCount = await skusQuery.CountAsync(s => s.Stock <= 0 && s.QuantityAvailableSoon > 0, ct),
            OutOfStockNotAvailable = await skusQuery.CountAsync(s => s.Stock <= 0 && s.QuantityNotAvailable > 0, ct),
            DiscountedSkuCount = discountedSkuCount,
            NonDiscountedSkuCount = skuCount - discountedSkuCount
        };
    }

    public async Task<List<DashboardTopCategoryDto>> GetTopCategoriesAsync(short tenantNumber, CancellationToken ct)
    {
        return await db.ArticleCategories
                .Where(ac =>
                            ac.Article.TenantNumber == tenantNumber &&
                            ac.Category.TenantNumber == tenantNumber &&
                            !ac.Article.IsDeleted &&
                            ac.Category.IsActive &&
                            !ac.Category.Children.Any())
                .GroupBy(ac => new
                {
                    ac.CategoryId,
                    ac.Category.Number,
                    ac.Category.Name
                })
                .Select(g => new DashboardTopCategoryDto
                {
                    CategoryNumber = g.Key.Number,
                    CategoryName = g.Key.Name,
                    ArticleCount = g.Count()
                })
                .OrderByDescending(x => x.ArticleCount)
                .ThenBy(x => x.CategoryNumber)
                .Take(5)
                .ToListAsync(ct);
    }

}
