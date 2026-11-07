using Models.DTOs.ArticleResponse;
using WebExportAPI.Db;
using WebExportAPI.Services;
using WebExportAPI.Tests.Infrastructure;

namespace WebExportAPI.Tests.Integration;

[Collection("Database tests")]
public class ArticleServiceTests
{
    [Fact]
    public async Task GetArticlesAsync_ReturnsOnlyRequestedTenantsArticles()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant1 = new Tenant
        {
            TenantNumber = 1,
            Name = "Testfirma 1"
        };

        var tenant2 = new Tenant
        {
            TenantNumber = 2,
            Name = "Testfirma 2"
        };

        db.Tenants.AddRange(tenant1, tenant2);

        db.Articles.AddRange(CreateArticle(tenant1, "A-001"), CreateArticle(tenant1, "A-002"), CreateArticle(tenant2, "B-001"));

        await db.SaveChangesAsync();

        var service = new ArticleService(db);

        var query = new ArticleListQueryDto
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await service.GetArticlesAsync(tenantNumber: 1, query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(new[] { "A-001", "A-002" }, result.Data.Select(article => article.ArticleNumber));
    }


    [Fact]
    public async Task GetArticleAsync_OtherTenant_ReturnsNull()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant1 = new Tenant
        {
            TenantNumber = 1,
            Name = "Testfirma 1"
        };

        var tenant2 = new Tenant
        {
            TenantNumber = 2,
            Name = "Testfirma 2"
        };

        db.Tenants.AddRange(tenant1, tenant2);

        db.Articles.Add(CreateArticle(tenant2, "B-001"));

        await db.SaveChangesAsync();

        var service = new ArticleService(db);

        // Act
        var ownerResult = await service.GetArticleAsync(ct: CancellationToken.None, tenantNumber: 2, articleNumber: "B-001");
        var otherTenantResult = await service.GetArticleAsync(ct: CancellationToken.None, tenantNumber: 1, articleNumber: "B-001");

        // Assert
        Assert.NotNull(ownerResult);
        Assert.Equal("B-001", ownerResult.ArticleNumber);
        Assert.Null(otherTenantResult);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 2)]
    public async Task GetArticlesAsync_IncludeDeleted_ReturnsExpectedArticles(bool includeDeleted, int expectedCount)
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant = new Tenant
        {
            TenantNumber = 1,
            Name = "Testfirma 1"
        };

        db.Tenants.Add(tenant);

        var activeArticle = CreateArticle(tenant, "A-001");

        var deletedArticle = CreateArticle(tenant, "A-002");
        deletedArticle.IsDeleted = true;

        db.Articles.AddRange(activeArticle, deletedArticle);

        await db.SaveChangesAsync();

        var service = new ArticleService(db);

        var query = new ArticleListQueryDto
        {
            Page = 1,
            PageSize = 10,
            IncludeDeleted = includeDeleted
        };

        // Act
        var result = await service.GetArticlesAsync(tenantNumber: 1, query: query, ct: CancellationToken.None);

        // Assert
        var expectedArticleNumbers = includeDeleted ? new[] { "A-001", "A-002" } : new[] { "A-001" };
        Assert.Equal(expectedCount, result.TotalCount);
        Assert.Equal(expectedArticleNumbers, result.Data.Select(article => article.ArticleNumber));
    }


    [Theory]
    [InlineData(1, new[] { "A-001", "A-002" })]
    [InlineData(2, new[] { "A-003", "A-004" })]
    [InlineData(3, new[] { "A-005" })]
    [InlineData(4, new string[] { })]
    public async Task GetArticlesAsync_Paging_ReturnsExpectedPage(int page, string[] expectedArticleNumbers)
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant = new Tenant
        {
            TenantNumber = 1,
            Name = "Testfirma 1"
        };

        db.Tenants.Add(tenant);

        db.Articles.AddRange(
            CreateArticle(tenant, "A-005"),
            CreateArticle(tenant, "A-001"),
            CreateArticle(tenant, "A-004"),
            CreateArticle(tenant, "A-002"),
            CreateArticle(tenant, "A-003")
        );

        await db.SaveChangesAsync();

        var service = new ArticleService(db);

        var query = new ArticleListQueryDto
        {
            Page = page,
            PageSize = 2
        };

        // Act
        var result = await service.GetArticlesAsync(tenantNumber: 1, query: query, ct: CancellationToken.None);

        // Assert
        Assert.Equal(page, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(expectedArticleNumbers, result.Data.Select(article => article.ArticleNumber));
    }


    [Fact]
    public async Task GetArticlesAsync_ModifiedSince_IncludesBoundaryAndNewerArticles()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var service = new ArticleService(db);

        var tenant = new Tenant
        {
            TenantNumber = 1,
            Name = "Testfirma 1"
        };

        db.Tenants.Add(tenant);

        var cutoff = new DateTimeOffset(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);

        db.Articles.AddRange(
            CreateArticle(tenant, "A-001", cutoff.AddMinutes(-1)),
            CreateArticle(tenant, "A-002", cutoff),
            CreateArticle(tenant, "A-003", cutoff.AddMinutes(1))
        );

        await db.SaveChangesAsync();

        var query = new ArticleListQueryDto
        {
            Page = 1,
            PageSize = 2,
            ModifiedSince = cutoff
        };

        // Act
        var result = await service.GetArticlesAsync(tenantNumber: 1, query: query, ct: CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(new[] { "A-002", "A-003" }, result.Data.Select(article => article.ArticleNumber));
    }


    [Fact]
    public async Task GetArticlesAsync_ArticleNumber_ReturnsExpectedArticles()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var service = new ArticleService(db);

        var tenant = new Tenant
        {
            TenantNumber = 1,
            Name = "Testfirma 1"
        };

        db.Tenants.Add(tenant);

        db.Articles.AddRange(
            CreateArticle(tenant, "A-001"),
            CreateArticle(tenant, "A-002"),
            CreateArticle(tenant, "A-003")
        );

        await db.SaveChangesAsync();

        var query = new ArticleListQueryDto
        {
            Page = 1,
            PageSize = 3,
            ArticleNumber = "A-002"
        };

        // Act
        var exactResult = await service.GetArticlesAsync(1, query, CancellationToken.None);
        // Assert
        Assert.Equal(new[] { "A-002" }, exactResult.Data.Select(article => article.ArticleNumber));


        query.ArticleNumber = "A-00";
        // Act
        var partialResult = await service.GetArticlesAsync(1, query, CancellationToken.None);
        // Assert
        Assert.Empty(partialResult.Data.Select(article => article.ArticleNumber));


        query.ArticleNumber = "A-999";
        // Act
        var missingResult = await service.GetArticlesAsync(1, query, CancellationToken.None);
        // Assert
        Assert.Empty(missingResult.Data.Select(article => article.ArticleNumber));


        query.ArticleNumber = null;
        // Act
        var unfilteredResult = await service.GetArticlesAsync(1, query, CancellationToken.None);
        // Assert
        Assert.Equal(new[] { "A-001", "A-002", "A-003" }, unfilteredResult.Data.Select(article => article.ArticleNumber));
    }


    [Fact]
    public async Task GetArticlesAsync_Category_ReturnsOnlyMatchingArticles()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var service = new ArticleService(db);

        var tenant = new Tenant { TenantNumber = 1, Name = "Testfirma 1" };
        var otherTenant = new Tenant { TenantNumber = 2, Name = "Testfirma 2" };
        db.Tenants.AddRange(tenant, otherTenant);

        var articles1 = CreateArticle(tenant, "A-001");
        var articles2 = CreateArticle(tenant, "A-002");
        var articles3 = CreateArticle(tenant, "A-003");
        db.Articles.AddRange(articles1, articles2, articles3);

        var category10 = new Category
        {
            TenantNumber = tenant.TenantNumber,
            Tenant = tenant,
            Number = 10,
            Name = "Category 10",
            ModifiedAt = default,
        };

        var category20 = new Category
        {
            TenantNumber = tenant.TenantNumber,
            Tenant = tenant,
            Number = 20,
            Name = "Category 20",
            ModifiedAt = default,
        };

        db.Categories.AddRange(category10, category20);

        db.ArticleCategories.AddRange(
            new ArticleCategory { Article = articles1, Category = category10 },
            new ArticleCategory { Article = articles2, Category = category20 }
        );


        var otherTenantArticle = CreateArticle(otherTenant, "B-001");
        db.Articles.Add(otherTenantArticle);

        var otherTenantCategory10 = new Category
        {
            Tenant = otherTenant,
            TenantNumber = otherTenant.TenantNumber,
            Number = 10,
            Name = "Category 10 - Tenant 2",
            ModifiedAt = DateTimeOffset.UtcNow
        };

        db.Categories.Add(otherTenantCategory10);

        db.ArticleCategories.Add(new ArticleCategory
        {
            Article = otherTenantArticle,
            Category = otherTenantCategory10
        });

        await db.SaveChangesAsync();

        var query = new ArticleListQueryDto
        {
            Page = 1,
            PageSize = 3,
            Category = 10
        };

        // Act
        var exactResult = await service.GetArticlesAsync(1, query, CancellationToken.None);
        // Assert
        Assert.Equal(["A-001"], exactResult.Data.Select(article => article.ArticleNumber));
        Assert.Equal(["A-001"], exactResult.Data.Select(article => article.ArticleNumber));


        query.Category = 99;
        // Act
        var missingResult = await service.GetArticlesAsync(1, query, CancellationToken.None);
        // Assert
        Assert.Empty(missingResult.Data.Select(article => article.ArticleNumber));


        query.Category = null;
        // Act
        var unfilteredResult = await service.GetArticlesAsync(1, query, CancellationToken.None);
        // Assert
        Assert.Equal(["A-001", "A-002", "A-003"], unfilteredResult.Data.Select(article => article.ArticleNumber));
        Assert.Equal(["A-001", "A-002", "A-003"], unfilteredResult.Data.Select(article => article.ArticleNumber));

    }

    [Fact]
    public async Task GetArticlesAsync_IncludeDeleted_FiltersNestedSkus()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var service = new ArticleService(db);

        var tenant = new Tenant
        {
            TenantNumber = 1,
            Name = "Testfirma 1"
        };
        db.Tenants.Add(tenant);

        var article = CreateArticle(tenant, "A-001");
        db.Articles.AddRange(article);

        var color = new Color { Id = 1, Name = "Black" };
        var sizeM = new Size { Id = 1, Name = "M", SizeRange = "S-XL" };
        var sizeL = new Size { Id = 2, Name = "L", SizeRange = "S-XL" };
        var activeSku = new Sku
        {
            SkuNumber = "SKU-ACTIVE",
            Article = article,
            Color = color,
            Size = sizeM,
            SalesPrice = 100m,
            IsDeleted = false,
            ModifiedAt = DateTimeOffset.UtcNow
        };
        var deletedSku = new Sku
        {
            SkuNumber = "SKU-DELETED",
            Article = article,
            Color = color,
            Size = sizeL,
            SalesPrice = 100m,
            IsDeleted = true,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        db.Colors.Add(color);
        db.Sizes.AddRange(sizeM, sizeL);
        db.Skus.AddRange(activeSku, deletedSku);

        await db.SaveChangesAsync();

        var query = new ArticleListQueryDto
        {
            Page = 1,
            PageSize = 1,
            IncludeDeleted = false
        };

        // Act
        var result = await service.GetArticlesAsync(1, query, CancellationToken.None);
        // Assert
        var activeOnlyArticle = Assert.Single(result.Data);
        Assert.Equal(["SKU-ACTIVE"], activeOnlyArticle.Skus.Select(sku => sku.Sku));

        // Act
        query.IncludeDeleted = true;
        var includingDeletedResult = await service.GetArticlesAsync(1, query, CancellationToken.None);
        // Assert
        var includingDeletedArticle = Assert.Single(includingDeletedResult.Data);
        Assert.Equal(["SKU-ACTIVE", "SKU-DELETED"], includingDeletedArticle.Skus.Select(sku => sku.Sku));
    }


    [Fact]
    public async Task GetArticlesAsync_Branch_ReturnsExpectedSkuStock()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var service = new ArticleService(db);

        var tenant = new Tenant
        {
            TenantNumber = 1,
            Name = "Testfirma 1"
        };
        db.Tenants.Add(tenant);

        var article = CreateArticle(tenant, "A-001");
        db.Articles.AddRange(article);

        var color = new Color { Id = 1, Name = "Black" };
        var sizeM = new Size { Id = 1, Name = "M", SizeRange = "S-XL" };
        var activeSku = new Sku
        {
            SkuNumber = "SKU-ACTIVE",
            Article = article,
            Color = color,
            Size = sizeM,
            SalesPrice = 100m,
            IsDeleted = false,
            ModifiedAt = DateTimeOffset.UtcNow,

        };
        activeSku.Stock = 100m;

        var branch10 = new Branch
        {
            TenantNumber = tenant.TenantNumber,
            Tenant = tenant,
            Number = 10,
            Name = "Branch 10",
            City = "Berlin",
        };

        var branch20 = new Branch
        {
            TenantNumber = tenant.TenantNumber,
            Tenant = tenant,
            Number = 20,
            Name = "Branch 20",
            City = "Hamburg",
        };

        db.Branches.AddRange(branch10, branch20);

        db.BranchStocks.Add(new BranchStock
        {
            Sku = activeSku,
            Branch = branch10,
            Stock = 7m,
            ModifiedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();

        var query = new ArticleListQueryDto
        {
            Page = 1,
            PageSize = 10,
            Branch = null
        };

        // Overall stock
        var overallResult = await service.GetArticlesAsync(1, query, CancellationToken.None);

        var overallArticle = Assert.Single(overallResult.Data);
        var overallSku = Assert.Single(overallArticle.Skus);
        Assert.Equal(100m, overallSku.Stock);


        // Branch 10 stock
        query.Branch = 10;

        var branch10Result = await service.GetArticlesAsync(1, query, CancellationToken.None);
        var branch10Article = Assert.Single(branch10Result.Data);
        var branch10Sku = Assert.Single(branch10Article.Skus);

        Assert.Equal(7m, branch10Sku.Stock);


        // Existing branch without a stock record
        query.Branch = 20;

        var branch20Result = await service.GetArticlesAsync(1, query, CancellationToken.None);
        var branch20Article = Assert.Single(branch20Result.Data);
        var branch20Sku = Assert.Single(branch20Article.Skus);

        Assert.Equal(0m, branch20Sku.Stock);
    }

    [Fact]
    public async Task GetArticlesAsync_TenantWithoutArticles_ReturnsEmptyPage()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant = new Tenant { TenantNumber = 1, Name = "Testfirma 1" };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var service = new ArticleService(db);

        var query = new ArticleListQueryDto { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetArticlesAsync(tenantNumber: 1, query: query, ct: CancellationToken.None);

        // Assert
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetArticleAsync_UnknownArticle_ReturnsNull()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var service = new ArticleService(db);

        var tenant = new Tenant { TenantNumber = 1, Name = "Testfirma 1" };
        db.Tenants.Add(tenant);

        db.Articles.Add(CreateArticle(tenant, "A-001"));
        await db.SaveChangesAsync();

        // Existing article
        var existingResult = await service.GetArticleAsync(tenantNumber: 1, articleNumber: "A-001", ct: CancellationToken.None);
        Assert.NotNull(existingResult);

        // Unknown article
        var missingResult = await service.GetArticleAsync(tenantNumber: 1, articleNumber: "A-999", ct: CancellationToken.None);

        Assert.Null(missingResult);
    }

    [Fact]
    public async Task BranchExistsAsync_RespectsTenantAndBranchNumber()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var service = new ArticleService(db);

        var tenant1 = new Tenant { TenantNumber = 1, Name = "Testfirma 1" };
        var tenant2 = new Tenant { TenantNumber = 2, Name = "Testfirma 2" };
        db.Tenants.AddRange(tenant1, tenant2);

        var branch10 = new Branch
        {
            Tenant = tenant1,
            TenantNumber = tenant1.TenantNumber,
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

        await db.SaveChangesAsync();


        // Act
        var tenant1OwnBranch = await service.BranchExistsAsync(tenantNumber: 1, branchNumber: 10, ct: CancellationToken.None);
        var tenant2OwnBranch = await service.BranchExistsAsync(tenantNumber: 2, branchNumber: 20, ct: CancellationToken.None);
        var otherTenantsBranch = await service.BranchExistsAsync(tenantNumber: 1, branchNumber: 20, ct: CancellationToken.None);
        var missingBranch = await service.BranchExistsAsync(tenantNumber: 1, branchNumber: 99, ct: CancellationToken.None);

        // Assert
        Assert.True(tenant1OwnBranch);
        Assert.True(tenant2OwnBranch);
        Assert.False(otherTenantsBranch);
        Assert.False(missingBranch);
    }

    private static Article CreateArticle(Tenant tenant, string articleNumber, DateTimeOffset? modifiedAt = null)
    {
        return new Article
        {
            Tenant = tenant,
            TenantNumber = tenant.TenantNumber,
            ArticleNumber = articleNumber,
            Name = $"Testartikel {articleNumber}",
            Manufacturer = "Testhersteller",
            CreatedAt = modifiedAt ?? DateTimeOffset.UtcNow,
            ModifiedAt = modifiedAt ?? DateTimeOffset.UtcNow

        };
    }
}