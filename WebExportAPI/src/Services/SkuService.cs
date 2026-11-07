using Microsoft.EntityFrameworkCore;
using Models.DTOs.ArticleResponse;
using Models.DTOs.SkuResponse;
using WebExportAPI.Db;

namespace WebExportAPI.Services;


public interface ISkuService
{
    Task<SkuResponseDto?> GetSkuAsync(CancellationToken ct, short tenantNumber, string sku, short? branch = null);
    Task<bool> BranchExistsAsync(short tenantNumber, short branchNumber, CancellationToken ct);
}


public record SkuService(AppDbContext Db) : ISkuService
{
    public async Task<SkuResponseDto?> GetSkuAsync(CancellationToken ct, short tenantNumber, string sku, short? branch = null)
    {
        return await Db.Skus
        .Where(s => s.SkuNumber == sku && s.Article.TenantNumber == tenantNumber)
        .Select(s => new SkuResponseDto
        {
            Sku = s.SkuNumber,
            ArticleNumber = s.Article.ArticleNumber,
            ArticleName = s.Article.Name,
            Color = new ColorDto
            {
                Id = s.Color.Id,
                Name = s.Color.Name,
                Blue = s.Color.Blue,
                Green = s.Color.Green,
                Red = s.Color.Red
            },
            SupplierColor = s.SupplierColor,
            Size = s.Size.Name,
            Fit = s.Fit,
            SalesPrice = s.SalesPrice,
            SalesPriceReduced = s.SalesPriceReduced,

            Stock = branch.HasValue ?
                (
                    s.BranchStocks
                    .Where(bs => bs.Branch.TenantNumber == tenantNumber && bs.Branch.Number == branch.Value)
                    .Select(bs => (decimal?)bs.Stock)
                    .FirstOrDefault() ?? 0m
                )
                : s.Stock,

            Quantities = new QuantitiesDto
            {
                AvailableNow = s.QuantityAvailableNow,
                AvailableSoon = s.QuantityAvailableSoon,
                NotAvailable = s.QuantityNotAvailable
            },
            IsLeadingColor = s.IsLeadingColor,
            IsDeleted = s.IsDeleted,
            ModifiedAt = s.ModifiedAt,
            BranchStock = s.BranchStocks
                                        .Where(bs => bs.Branch.TenantNumber == tenantNumber && (!branch.HasValue || bs.Branch.Number == branch.Value))
                                        .Select(bs => new BranchStockDto
                                        {
                                            Branch = bs.Branch.Number,
                                            BranchName = bs.Branch.Name,
                                            Stock = bs.Stock
                                        }).ToList()

        }).FirstOrDefaultAsync(ct);
    }

    public Task<bool> BranchExistsAsync(short tenantNumber, short branchNumber, CancellationToken ct)
    {
        return Db.Branches.AnyAsync(b => b.TenantNumber == tenantNumber && b.Number == branchNumber, ct);
    }
}
