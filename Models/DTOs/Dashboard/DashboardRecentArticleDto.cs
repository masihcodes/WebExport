namespace Models.DTOs.Dashboard;

public record DashboardRecentArticleDto
{
    public required string ArticleNumber { get; set; }
    public required string Name { get; set; }
    public required DateTimeOffset ModifiedAt { get; set; }
}
