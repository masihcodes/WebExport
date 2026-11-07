namespace Models.DTOs.CategoryResponse;

public record CategoryTreeResponseDto
{
    public required int Number { get; set; }
    public required string Name { get; set; }
    public short Rank { get; set; }
    public bool IsActive { get; set; }
    public List<CategoryTreeResponseDto> Children { get; set; } = [];
}
