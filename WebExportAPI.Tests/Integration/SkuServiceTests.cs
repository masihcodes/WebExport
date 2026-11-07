using WebExportAPI.Db;
using WebExportAPI.Services;
using WebExportAPI.Tests.Infrastructure;

namespace WebExportAPI.Tests.Integration;

[Collection("Database tests")]
public class SkuServiceTests
{
    [Fact]
    public async Task GetSkuAsync_RespectsTenantAndReturnsNullForMissingSku()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant1 = new Tenant { TenantNumber = 1, Name = "Testfirma 1" };

        var tenant2 = new Tenant { TenantNumber = 2, Name = "Testfirma 2" };

        var article = new Article
        {
            Tenant = tenant2,
            TenantNumber = tenant2.TenantNumber,
            ArticleNumber = "B-001",
            Name = "Testartikel B-001",
            Manufacturer = "Testhersteller",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        var color = new Color { Id = 1, Name = "Black" };

        var size = new Size { Id = 1, Name = "M", SizeRange = "S-XL" };

        var sku = new Sku
        {
            SkuNumber = "SKU-001",
            Article = article,
            Color = color,
            Size = size,
            SalesPrice = 99.95m,
            SalesPriceReduced = 79.95m,
            Stock = 12.375m,
            IsDeleted = false,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        db.Tenants.AddRange(tenant1, tenant2);
        db.Articles.Add(article);
        db.Colors.Add(color);
        db.Sizes.Add(size);
        db.Skus.Add(sku);

        await db.SaveChangesAsync();

        var service = new SkuService(db);

        // Owner's SKU
        var ownerResult = await service.GetSkuAsync(ct: CancellationToken.None, tenantNumber: 2, sku: "SKU-001");

        Assert.NotNull(ownerResult);
        Assert.Equal("SKU-001", ownerResult.Sku);
        Assert.Equal("B-001", ownerResult.ArticleNumber);
        Assert.Equal("Black", ownerResult.Color.Name);
        Assert.Equal("M", ownerResult.Size);
        Assert.Equal(99.95m, ownerResult.SalesPrice);
        Assert.Equal(79.95m, ownerResult.SalesPriceReduced);
        Assert.Equal(12.375m, ownerResult.Stock);

        // Another tenant's request
        var otherTenantResult = await service.GetSkuAsync(ct: CancellationToken.None, tenantNumber: 1, sku: "SKU-001");
        Assert.Null(otherTenantResult);

        // Unknown SKU
        var missingResult = await service.GetSkuAsync(ct: CancellationToken.None, tenantNumber: 2, sku: "SKU-UNKNOWN");
        Assert.Null(missingResult);
    }

    [Fact]
    public async Task GetSkuAsync_Branch_ReturnsExpectedStockAndBranchStocks()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant2 = new Tenant { TenantNumber = 2, Name = "Testfirma 2" };

        var article = new Article
        {
            Tenant = tenant2,
            TenantNumber = tenant2.TenantNumber,
            ArticleNumber = "B-001",
            Name = "Testartikel B-001",
            Manufacturer = "Testhersteller",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        var color = new Color { Id = 1, Name = "Black" };

        var size = new Size { Id = 1, Name = "M", SizeRange = "S-XL" };

        var sku = new Sku
        {
            SkuNumber = "SKU-001",
            Article = article,
            Color = color,
            Size = size,
            SalesPrice = 99.95m,
            SalesPriceReduced = 79.95m,
            Stock = 12.375m,
            IsDeleted = false,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        db.Tenants.Add(tenant2);
        db.Articles.Add(article);
        db.Colors.Add(color);
        db.Sizes.Add(size);
        db.Skus.Add(sku);

        var branch10 = new Branch
        {
            Tenant = tenant2,
            TenantNumber = tenant2.TenantNumber,
            Number = 10,
            Name = "Branch 10",
            City = "Berlin"
        };

        var branch20 = new Branch
        {
            Tenant = tenant2,
            TenantNumber = tenant2.TenantNumber,
            Number = 20,
            Name = "Branch 20",
            City = "Hamburg"
        };

        db.Branches.AddRange(branch10, branch20);

        db.BranchStocks.Add(new BranchStock
        {
            Sku = sku,
            Branch = branch10,
            Stock = 7m,
            ModifiedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new SkuService(db);


        // Overall stock
        var overallResult = await service.GetSkuAsync(
            ct: CancellationToken.None,
            tenantNumber: 2,
            sku: "SKU-001",
            branch: null);

        Assert.NotNull(overallResult);
        Assert.Equal(12.375m, overallResult.Stock);

        var overallBranch = Assert.Single(overallResult.BranchStock);
        Assert.Equal(10, overallBranch.Branch);
        Assert.Equal("Branch 10", overallBranch.BranchName);
        Assert.Equal(7m, overallBranch.Stock);

        // Branch 10
        var branch10Result = await service.GetSkuAsync(
            ct: CancellationToken.None,
            tenantNumber: 2,
            sku: "SKU-001",
            branch: 10);

        Assert.NotNull(branch10Result);
        Assert.Equal(7m, branch10Result.Stock);

        var selectedBranch = Assert.Single(branch10Result.BranchStock);
        Assert.Equal(10, selectedBranch.Branch);
        Assert.Equal(7m, selectedBranch.Stock);

        // Existing branch without a stock record
        var branch20Result = await service.GetSkuAsync(
            ct: CancellationToken.None,
            tenantNumber: 2,
            sku: "SKU-001",
            branch: 20);

        Assert.NotNull(branch20Result);
        Assert.Equal(0m, branch20Result.Stock);
        Assert.Empty(branch20Result.BranchStock);
    }
}