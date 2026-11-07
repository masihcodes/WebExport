namespace Models.DTOs.ArticleResponse;

public record ArticleAttributeDto
{
    public short AttributeType { get; set; }
    public required string AttributeTypeName { get; set; }
    public short Number { get; set; }
    public required string Name { get; set; }
}
