namespace Models.DTOs.Dashboard;

public record DashboardContentQualityDto
{
    public int ArticlesWithoutDescriptionCount { get; set; }
    public int ArticlesWithoutImageCount { get; set; }
    public int ArticlesWithoutActiveCategoryCount { get; set; }
}
