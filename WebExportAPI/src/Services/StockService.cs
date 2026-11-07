using Microsoft.EntityFrameworkCore;
using Models.DTOs;
using Models.DTOs.ArticleResponse;
using Models.DTOs.BranchStockResponse;
using Models.DTOs.PricesStockResponse;
using WebExportAPI.Db;

namespace WebExportAPI.Services;

public interface IStockService
{
    Task<PagedResponseDto<PricesStockResponseDto>> GetPricesStockAsync(short tenantNumber, PricesStockListQueryDto query, CancellationToken ct);
    Task<PagedResponseDto<BranchStockResponseDto>> GetBranchStockAsync(short tenantNumber, BranchStockListQueryDto query, CancellationToken ct);
    Task<bool> BranchExistsAsync(short tenantNumber, short branchNumber, CancellationToken ct);
}

public class StockService(AppDbContext db) : IStockService
{
    public async Task<PagedResponseDto<BranchStockResponseDto>> GetBranchStockAsync(short tenantNumber, BranchStockListQueryDto query, CancellationToken ct)
    {
        var resQuery = db.BranchStocks.Where(bs => bs.Branch.TenantNumber == tenantNumber && bs.Sku.Article.TenantNumber == tenantNumber);

        if (!string.IsNullOrWhiteSpace(query.ArticleNumber))
        {
            resQuery = resQuery.Where(x => x.Sku.Article.ArticleNumber.Contains(query.ArticleNumber));
        }
        if (query.Branch is not null)
        {
            resQuery = resQuery.Where(x => x.Branch.Number == query.Branch);
        }
        if (!string.IsNullOrWhiteSpace(query.Sku))
        {
            resQuery = resQuery.Where(x => x.Sku.SkuNumber.Contains(query.Sku));
        }
        if (query.ModifiedSince is DateTimeOffset modifiedSince)
        {
            var modifiedSinceUtc = modifiedSince.ToUniversalTime();
            resQuery = resQuery.Where(x => x.ModifiedAt >= modifiedSinceUtc);
        }


        var totalCount = await resQuery.CountAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var offset = ((long)query.Page - 1) * query.PageSize;

        var pageQuery = resQuery
                                    .OrderBy(x => x.Sku.SkuNumber)
                                    .ThenBy(x => x.Branch.Number)
                                    .Skip((int)Math.Min(offset, int.MaxValue))
                                    .Take(query.PageSize);

        List<BranchStockResponseDto> pageItems = offset >= totalCount ? [] : await pageQuery.Select(bs => new BranchStockResponseDto
        {
            Sku = bs.Sku.SkuNumber,
            ArticleNumber = bs.Sku.Article.ArticleNumber,
            Branch = bs.Branch.Number,
            Stock = bs.Stock,
            ModifiedAt = bs.ModifiedAt
        }).ToListAsync(ct);

        return new PagedResponseDto<BranchStockResponseDto>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Data = pageItems
        };
    }


    public async Task<PagedResponseDto<PricesStockResponseDto>> GetPricesStockAsync(short tenantNumber, PricesStockListQueryDto query, CancellationToken ct)
    {
        var resQuery = db.Skus.Where(sku => sku.Article.TenantNumber == tenantNumber);

        if (!query.IncludeDeleted)
        {
            resQuery = resQuery.Where(sku => !sku.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(query.ArticleNumber))
        {
            resQuery = resQuery.Where(sku => sku.Article.ArticleNumber.Contains(query.ArticleNumber));
        }
        if (query.ModifiedSince is DateTimeOffset modifiedSince)
        {
            var modifiedSinceUtc = modifiedSince.ToUniversalTime();
            resQuery = resQuery.Where(sku => sku.ModifiedAt >= modifiedSinceUtc);
        }


        var totalCount = await resQuery.CountAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var offset = ((long)query.Page - 1) * query.PageSize;

        var pageQuery = resQuery
                                .OrderBy(sku => sku.SkuNumber)
                                .Skip((int)Math.Min(offset, int.MaxValue))
                                .Take(query.PageSize);

        List<PricesStockResponseDto> pageItems = offset >= totalCount ? [] : await pageQuery.Select(sku => new PricesStockResponseDto
        {
            Sku = sku.SkuNumber,
            ArticleNumber = sku.Article.ArticleNumber,
            SalesPrice = sku.SalesPrice,
            SalesPriceReduced = sku.SalesPriceReduced,
            Stock = query.Branch.HasValue ?
            (
                sku.BranchStocks
                .Where(bs => bs.Branch.TenantNumber == tenantNumber && bs.Branch.Number == query.Branch.Value)
                .Select(bs => (decimal?)bs.Stock).FirstOrDefault() ?? 0m
            ) : sku.Stock,

            Quantities = new QuantitiesDto
            {
                AvailableNow = sku.QuantityAvailableNow,
                AvailableSoon = sku.QuantityAvailableSoon,
                NotAvailable = sku.QuantityNotAvailable
            },
            ModifiedAt = sku.ModifiedAt,
            IsDeleted = sku.IsDeleted
        }).ToListAsync(ct);

        return new PagedResponseDto<PricesStockResponseDto>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Data = pageItems
        };
    }


    public Task<bool> BranchExistsAsync(short tenantNumber, short branchNumber, CancellationToken ct)
    {
        return db.Branches.AnyAsync(b => b.TenantNumber == tenantNumber && b.Number == branchNumber, ct);
    }
}
