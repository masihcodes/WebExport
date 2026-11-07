using Models.Enums;

namespace Models.DTOs.ArticleResponse;

public record ArticleTextDto
{
    public TextType Type { get; set; }
    public required string Text { get; set; }
    public short SortOrder { get; set; }
}