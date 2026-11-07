namespace Models.DTOs.CategoryResponse;

public record CategoryResponseDto
{
    public required int Number { get; set; }
    public int? ParentNumber { get; set; }
    public short Rank { get; set; }
    public required string Name { get; set; }
    public string? SearchTerms { get; set; }
    public bool IsActive { get; set; }
}
