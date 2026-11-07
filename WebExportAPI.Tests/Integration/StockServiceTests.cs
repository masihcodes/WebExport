using Models.DTOs.BranchStockResponse;
using Models.DTOs.PricesStockResponse;
using WebExportAPI.Db;
using WebExportAPI.Services;
using WebExportAPI.Tests.Infrastructure;

namespace WebExportAPI.Tests.Integration;

[Collection("Database tests")]
public class StockServiceTests
{
    [Fact]
    public async Task GetPricesStockAsync_ReturnsOnlyRequestedTenantsPricesAndStock()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant1 = new Tenant { TenantNumber = 1, Name = "Testfirma 1" };

        var tenant2 = new Tenant { TenantNumber = 2, Name = "Testfirma 2" };

        var article1 = CreateArticle(tenant1, "A-001");
        var article2 = CreateArticle(tenant2, "B-001");

        var color = new Color { Id = 1, Name = "Black" };
        var size = new Size { Id = 1, Name = "M", SizeRange = "S-XL" };

        var sku1 = new Sku
        {
            SkuNumber = "SKU-A",
            Article = article1,
            Color = color,
            Size = size,
            SalesPrice = 99.95m,
            SalesPriceReduced = 79.95m,
            Stock = 12.375m,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        var sku2 = new Sku
        {
            SkuNumber = "SKU-B",
            Article = article2,
            Color = color,
            Size = size,
            SalesPrice = 49.95m,
            SalesPriceReduced = null,
            Stock = 4.5m,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        db.Tenants.AddRange(tenant1, tenant2);
        db.Articles.AddRange(article1, article2);
        db.Colors.Add(color);
        db.Sizes.Add(size);
        db.Skus.AddRange(sku1, sku2);

        await db.SaveChangesAsync();

        var service = new StockService(db);

        var query = new PricesStockListQueryDto { Page = 1, PageSize = 10 };

        // Tenant 1
        var tenant1Result = await service.GetPricesStockAsync(1, query, CancellationToken.None);
        Assert.Equal(1, tenant1Result.TotalCount);

        var tenant1Item = Assert.Single(tenant1Result.Data);
        Assert.Equal("SKU-A", tenant1Item.Sku);
        Assert.Equal("A-001", tenant1Item.ArticleNumber);
        Assert.Equal(99.95m, tenant1Item.SalesPrice);
        Assert.Equal(79.95m, tenant1Item.SalesPriceReduced);
        Assert.Equal(12.375m, tenant1Item.Stock);

        // Tenant 2
        var tenant2Result = await service.GetPricesStockAsync(2, query, CancellationToken.None);
        Assert.Equal(1, tenant2Result.TotalCount);

        var tenant2Item = Assert.Single(tenant2Result.Data);
        Assert.Equal("SKU-B", tenant2Item.Sku);
        Assert.Equal("B-001", tenant2Item.ArticleNumber);
        Assert.Equal(49.95m, tenant2Item.SalesPrice);
        Assert.Null(tenant2Item.SalesPriceReduced);
        Assert.Equal(4.5m, tenant2Item.Stock);
    }


    [Fact]
    public async Task GetBranchStockAsync_RespectsTenantAndBranchFilter()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant1 = new Tenant { TenantNumber = 1, Name = "Testfirma 1" };

        var tenant2 = new Tenant { TenantNumber = 2, Name = "Testfirma 2" };

        var article1 = CreateArticle(tenant1, "A-001");
        var article2 = CreateArticle(tenant2, "B-001");

        var color = new Color { Id = 1, Name = "Black" };
        var size = new Size { Id = 1, Name = "M", SizeRange = "S-XL" };

        var sku1 = new Sku
        {
            SkuNumber = "SKU-A",
            Article = article1,
            Color = color,
            Size = size,
            SalesPrice = 99.95m,
            SalesPriceReduced = 79.95m,
            Stock = 12.375m,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        var sku2 = new Sku
        {
            SkuNumber = "SKU-B",
            Article = article2,
            Color = color,
            Size = size,
            SalesPrice = 49.95m,
            SalesPriceReduced = null,
            Stock = 4.5m,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        var tenant1Branch10 = new Branch
        {
            Tenant = tenant1,
            TenantNumber = tenant1.TenantNumber,
            Number = 10,
            Name = "Branch 10",
            City = "Berlin"
        };

        var tenant1Branch20 = new Branch
        {
            Tenant = tenant1,
            TenantNumber = tenant1.TenantNumber,
            Number = 20,
            Name = "Branch 20",
            City = "Hamburg"
        };

        var tenant2Branch10 = new Branch
        {
            Tenant = tenant2,
            TenantNumber = tenant2.TenantNumber,
            Number = 10,
            Name = "Branch 10 - Tenant 2",
            City = "Munich"
        };


        db.Tenants.AddRange(tenant1, tenant2);
        db.Articles.AddRange(article1, article2);
        db.Colors.Add(color);
        db.Sizes.Add(size);
        db.Skus.AddRange(sku1, sku2);

        db.Branches.AddRange(tenant1Branch10, tenant1Branch20, tenant2Branch10);
        db.BranchStocks.AddRange(
            new BranchStock { Sku = sku1, Branch = tenant1Branch10, Stock = 7m, ModifiedAt = DateTimeOffset.UtcNow },
            new BranchStock { Sku = sku1, Branch = tenant1Branch20, Stock = 3m, ModifiedAt = DateTimeOffset.UtcNow },
            new BranchStock { Sku = sku2, Branch = tenant2Branch10, Stock = 50m, ModifiedAt = DateTimeOffset.UtcNow }
        );

        await db.SaveChangesAsync();

        var service = new StockService(db);


        var query = new BranchStockListQueryDto { Page = 1, PageSize = 10 };

        // Tenant 1: all branches
        var tenant1Result = await service.GetBranchStockAsync(1, query, CancellationToken.None);

        Assert.Equal(2, tenant1Result.TotalCount);
        Assert.Equal(["SKU-A", "SKU-A"], tenant1Result.Data.Select(item => item.Sku));
        Assert.Equal(new short[] { 10, 20 }, tenant1Result.Data.Select(item => item.Branch));
        Assert.Equal(new[] { 7m, 3m }, tenant1Result.Data.Select(item => item.Stock));

        // Tenant 1: branch 20
        query.Branch = 20;
        var branch20Result = await service.GetBranchStockAsync(1, query, CancellationToken.None);

        Assert.Equal(1, branch20Result.TotalCount);

        var branch20Item = Assert.Single(branch20Result.Data);
        Assert.Equal("SKU-A", branch20Item.Sku);
        Assert.Equal("A-001", branch20Item.ArticleNumber);
        Assert.Equal(20, branch20Item.Branch);
        Assert.Equal(3m, branch20Item.Stock);

        // Tenant 2: branch 10
        query.Branch = 10;

        var tenant2Result = await service.GetBranchStockAsync(2, query, CancellationToken.None);

        Assert.Equal(1, tenant2Result.TotalCount);

        var tenant2Item = Assert.Single(tenant2Result.Data);
        Assert.Equal("SKU-B", tenant2Item.Sku);
        Assert.Equal("B-001", tenant2Item.ArticleNumber);
        Assert.Equal(10, tenant2Item.Branch);
        Assert.Equal(50m, tenant2Item.Stock);
    }

    [Fact]
    public async Task GetPricesStockAsync_IncludeDeleted_ReturnsOnlyRequestedTenantsSkus()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant1 = new Tenant { TenantNumber = 1, Name = "Testfirma 1" };
        var tenant2 = new Tenant { TenantNumber = 2, Name = "Testfirma 2" };

        var article1 = CreateArticle(tenant1, "A-001");
        var article2 = CreateArticle(tenant2, "B-001");

        var color = new Color { Id = 1, Name = "Black" };
        var size = new Size { Id = 1, Name = "M", SizeRange = "S-XL" };
        var sizeL = new Size { Id = 2, Name = "L", SizeRange = "S-XL" };
        var sku1 = new Sku
        {
            SkuNumber = "SKU-A",
            Article = article1,
            Color = color,
            Size = size,
            SalesPrice = 99.95m,
            SalesPriceReduced = 79.95m,
            Stock = 12.375m,
            IsDeleted = false,
            ModifiedAt = DateTimeOffset.UtcNow
        };
        var sku2 = new Sku
        {
            SkuNumber = "SKU-B",
            Article = article2,
            Color = color,
            Size = size,
            SalesPrice = 49.95m,
            SalesPriceReduced = null,
            Stock = 4.5m,
            ModifiedAt = DateTimeOffset.UtcNow
        };
        var deletedSku = new Sku
        {
            SkuNumber = "SKU-DELETED",
            Article = article1,
            Color = color,
            Size = sizeL,
            SalesPrice = 89.95m,
            IsDeleted = true,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        db.Tenants.AddRange(tenant1, tenant2);
        db.Articles.AddRange(article1, article2);
        db.Colors.Add(color);
        db.Sizes.Add(size);
        db.Skus.AddRange(sku1, sku2, deletedSku);
        db.Sizes.Add(sizeL);

        // Deleted SKU belonging to Tenant 2
        sku2.IsDeleted = true;

        await db.SaveChangesAsync();

        var service = new StockService(db);

        var query = new PricesStockListQueryDto { Page = 1, PageSize = 10, IncludeDeleted = false };

        // Active SKUs only
        var activeOnlyResult = await service.GetPricesStockAsync(1, query, CancellationToken.None);

        Assert.Equal(1, activeOnlyResult.TotalCount);

        var activeItem = Assert.Single(activeOnlyResult.Data);
        Assert.Equal("SKU-A", activeItem.Sku);
        Assert.False(activeItem.IsDeleted);

        // Include deleted SKUs
        query.IncludeDeleted = true;

        var includingDeletedResult = await service.GetPricesStockAsync(1, query, CancellationToken.None);

        Assert.Equal(2, includingDeletedResult.TotalCount);

        Assert.Equal(["SKU-A", "SKU-DELETED"], includingDeletedResult.Data.Select(item => item.Sku));

        Assert.Equal(new[] { false, true }, includingDeletedResult.Data.Select(item => item.IsDeleted));


    }


    [Fact]
    public async Task GetPricesStockAsync_ModifiedSince_IncludesBoundaryAndNewerSkus()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var cutoff = new DateTimeOffset(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);

        var tenant = new Tenant { TenantNumber = 1, Name = "Testfirma 1" };

        var article = CreateArticle(tenant, "A-001");
        article.CreatedAt = cutoff.AddDays(-1);
        article.ModifiedAt = cutoff.AddHours(1);

        var color = new Color { Id = 1, Name = "Black" };

        var sizeM = new Size { Id = 1, Name = "M", SizeRange = "S-XL" };
        var sizeL = new Size { Id = 2, Name = "L", SizeRange = "S-XL" };
        var sizeXL = new Size { Id = 3, Name = "XL", SizeRange = "S-XL" };

        var beforeSku = new Sku
        {
            SkuNumber = "SKU-001",
            Article = article,
            Color = color,
            Size = sizeM,
            SalesPrice = 100m,
            ModifiedAt = cutoff.AddMinutes(-1)
        };

        var boundarySku = new Sku
        {
            SkuNumber = "SKU-002",
            Article = article,
            Color = color,
            Size = sizeL,
            SalesPrice = 100m,
            ModifiedAt = cutoff
        };

        var newerSku = new Sku
        {
            SkuNumber = "SKU-003",
            Article = article,
            Color = color,
            Size = sizeXL,
            SalesPrice = 100m,
            ModifiedAt = cutoff.AddMinutes(1)
        };

        db.Tenants.Add(tenant);
        db.Articles.Add(article);
        db.Colors.Add(color);
        db.Sizes.AddRange(sizeM, sizeL, sizeXL);
        db.Skus.AddRange(beforeSku, boundarySku, newerSku);

        await db.SaveChangesAsync();

        var service = new StockService(db);

        var query = new PricesStockListQueryDto { Page = 1, PageSize = 10, ModifiedSince = cutoff };

        // Act
        var result = await service.GetPricesStockAsync(1, query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(["SKU-002", "SKU-003"], result.Data.Select(item => item.Sku));
        Assert.Equal(new[] { cutoff, cutoff.AddMinutes(1) }, result.Data.Select(item => item.ModifiedAt));
    }


    [Fact]
    public async Task GetBranchStockAsync_ModifiedSince_IncludesBoundaryAndNewerStocks()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var cutoff = new DateTimeOffset(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);

        var tenant = new Tenant { TenantNumber = 1, Name = "Testfirma 1" };

        var article = CreateArticle(tenant, "A-001");
        article.CreatedAt = cutoff.AddDays(-1);
        article.ModifiedAt = cutoff.AddHours(1);

        var color = new Color { Id = 1, Name = "Black" };
        var size = new Size { Id = 1, Name = "M", SizeRange = "S-XL" };

        var sku = new Sku
        {
            SkuNumber = "SKU-001",
            Article = article,
            Color = color,
            Size = size,
            SalesPrice = 100m,
            Stock = 100m,
            ModifiedAt = cutoff.AddHours(1)
        };

        var branch10 = new Branch
        {
            Tenant = tenant,
            TenantNumber = tenant.TenantNumber,
            Number = 10,
            Name = "Branch 10",
            City = "Berlin"
        };

        var branch20 = new Branch
        {
            Tenant = tenant,
            TenantNumber = tenant.TenantNumber,
            Number = 20,
            Name = "Branch 20",
            City = "Hamburg"
        };

        var branch30 = new Branch
        {
            Tenant = tenant,
            TenantNumber = tenant.TenantNumber,
            Number = 30,
            Name = "Branch 30",
            City = "Munich"
        };

        db.Tenants.Add(tenant);
        db.Articles.Add(article);
        db.Colors.Add(color);
        db.Sizes.Add(size);
        db.Skus.Add(sku);
        db.Branches.AddRange(branch10, branch20, branch30);

        db.BranchStocks.AddRange(
            new BranchStock { Sku = sku, Branch = branch10, Stock = 5m, ModifiedAt = cutoff.AddMinutes(-1) },
            new BranchStock { Sku = sku, Branch = branch20, Stock = 7m, ModifiedAt = cutoff },
            new BranchStock { Sku = sku, Branch = branch30, Stock = 9m, ModifiedAt = cutoff.AddMinutes(1) });

        await db.SaveChangesAsync();

        var service = new StockService(db);

        var query = new BranchStockListQueryDto { Page = 1, PageSize = 10, ModifiedSince = cutoff };

        // Act
        var result = await service.GetBranchStockAsync(1, query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalCount);

        Assert.Equal(["SKU-001", "SKU-001"], result.Data.Select(item => item.Sku));
        Assert.Equal(new short[] { 20, 30 }, result.Data.Select(item => item.Branch));
        Assert.Equal(new[] { 7m, 9m }, result.Data.Select(item => item.Stock));
        Assert.Equal(new[] { cutoff, cutoff.AddMinutes(1) }, result.Data.Select(item => item.ModifiedAt));
    }



    private static Article CreateArticle(Tenant tenant, string articleNumber)
    {
        return new Article
        {
            Tenant = tenant,
            TenantNumber = tenant.TenantNumber,
            ArticleNumber = articleNumber,
            Name = $"Testartikel {articleNumber}",
            Manufacturer = "Testhersteller",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow
        };
    }
}