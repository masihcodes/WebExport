namespace Models.DTOs.TenantResponse;

public record TenantResponseDto
{
    public short Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BranchCount { get; set; }
}
