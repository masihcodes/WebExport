namespace Models.DTOs.ArticleResponse;

public record ArticleListQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
    public string? ArticleNumber { get; set; }
    public int? Category { get; set; }
    public short? Branch { get; set; }
    public DateTimeOffset? ModifiedSince { get; set; }
    public bool IncludeDeleted { get; set; } = false;
}

