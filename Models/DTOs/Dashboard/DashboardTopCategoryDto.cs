namespace Models.DTOs.Dashboard;

public record DashboardTopCategoryDto
{
    public int CategoryNumber { get; set; }
    public required string CategoryName { get; set; }
    public int ArticleCount { get; set; }
}