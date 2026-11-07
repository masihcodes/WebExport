using Microsoft.EntityFrameworkCore;

namespace WebExportAPI.Db;


public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Size> Sizes => Set<Size>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<DeliveryStatusText> DeliveryStatusTexts => Set<DeliveryStatusText>();
    public DbSet<MaterialType> MaterialTypes => Set<MaterialType>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<AttributeType> AttributeTypes => Set<AttributeType>();
    public DbSet<Attribute> Attributes => Set<Attribute>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Sku> Skus => Set<Sku>();
    public DbSet<BranchStock> BranchStocks => Set<BranchStock>();
    public DbSet<ArticleCategory> ArticleCategories => Set<ArticleCategory>();
    public DbSet<ArticleText> ArticleTexts => Set<ArticleText>();
    public DbSet<ArticleRelation> ArticleRelations => Set<ArticleRelation>();
    public DbSet<ArticleImage> ArticleImages => Set<ArticleImage>();
    public DbSet<MaterialComposition> MaterialCompositions => Set<MaterialComposition>();
    public DbSet<SizeChart> SizeCharts => Set<SizeChart>();
    public DbSet<ArticleAttribute> ArticleAttributes => Set<ArticleAttribute>();
    public DbSet<ApiClient> ApiClients => Set<ApiClient>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>().Property(t => t.IsActive).HasDefaultValue(true);

        modelBuilder.Entity<Color>().ToTable(table =>
        {
            table.HasCheckConstraint("ck_color_red_range", "red BETWEEN 0 AND 255");
            table.HasCheckConstraint("ck_color_green_range", "green BETWEEN 0 AND 255");
            table.HasCheckConstraint("ck_color_blue_range", "blue BETWEEN 0 AND 255");
        });

        modelBuilder.Entity<Branch>(model =>
        {
            model.Property(b => b.Id).UseIdentityAlwaysColumn();
            model.Property(b => b.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Category>(model =>
        {
            model.Property(c => c.Id).UseIdentityAlwaysColumn();
            model.Property(c => c.ModifiedAt).HasDefaultValueSql("now()");
            model.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => new { c.TenantNumber, c.ParentId })
                .HasPrincipalKey(c => new { c.TenantNumber, c.Id })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Article>(model =>
        {
            model.Property(a => a.Id).UseIdentityAlwaysColumn();
            model.Property(a => a.IsDeleted).HasDefaultValue(false);
            model.Property(a => a.ModifiedAt).HasDefaultValueSql("now()");
            model.ToTable(table =>
            {
                table.HasCheckConstraint("ck_article_delivery_time_range", "delivery_time_max_days >= delivery_time_min_days");
                table.HasCheckConstraint("ck_article_validity_range", "valid_to >= valid_from");
            });
        });

        modelBuilder.Entity<Sku>(model =>
        {
            model.Property(s => s.Id).UseIdentityAlwaysColumn();
            model.Property(s => s.IsDeleted).HasDefaultValue(false);
            model.Property(s => s.ModifiedAt).HasDefaultValueSql("now()");

            model.HasIndex(s => new
            {
                s.ArticleId,
                s.ColorId,
                s.SizeId,
                s.Fit
            }).IsUnique().AreNullsDistinct(false);

            model.ToTable(table =>
            {
                table.HasCheckConstraint("ck_sku_sales_price", "sales_price >= 0");
                table.HasCheckConstraint("ck_sku_sales_price_reduced", "sales_price_reduced < sales_price");
            });
        });

        modelBuilder.Entity<BranchStock>().Property(b => b.ModifiedAt).HasDefaultValueSql("now()");

        modelBuilder.Entity<ArticleText>().Property(t => t.Id).UseIdentityAlwaysColumn();

        modelBuilder.Entity<ArticleRelation>().ToTable(table =>
        {
            table.HasCheckConstraint("ck_article_relation_not_self", "target_article_id <> article_id");
        });

        modelBuilder.Entity<ArticleImage>(model =>
        {
            model.Property(i => i.Id).UseIdentityAlwaysColumn();
            model.Property(i => i.ModifiedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<MaterialComposition>().ToTable(table =>
        {
            table.HasCheckConstraint("ck_material_composition_percentage", "percentage > 0 AND percentage <= 100");
        });

        modelBuilder.Entity<SizeChart>().Property(s => s.Id).UseIdentityAlwaysColumn();

        modelBuilder.Entity<ApiClient>().Property(c => c.Id).UseIdentityAlwaysColumn();
    }
}
