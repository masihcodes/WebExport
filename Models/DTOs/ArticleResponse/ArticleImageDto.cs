using Models.Enums;

namespace Models.DTOs.ArticleResponse;

public record ArticleImageDto
{
    public required string FileName { get; set; }
    public ImageType Type { get; set; }
    public short? ColorId { get; set; }
    public short SortOrder { get; set; }
    public bool IsDeleted { get; set; } = false;
}
