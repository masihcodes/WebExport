namespace Models.DTOs.MasterDataResponse;

public record BranchResponseDto
{
    public short Number { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public bool IsActive { get; set; }
}
