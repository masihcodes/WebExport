namespace Models.DTOs.MasterDataResponse;

public record SizeResponseDto
{
    public short Id { get; set; }
    public required string Name { get; set; }
    public required string SizeRange { get; set; }
    public short SortOrder { get; set; }
}
