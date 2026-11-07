using Models.Enums;

namespace Models.DTOs.ArticleResponse;

public record ArticleRelationDto
{
    public RelationType Type { get; set; }
    public required string ArticleNumber { get; set; }
}
