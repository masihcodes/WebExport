namespace Models.DTOs.Auth;

public record AuthResponseDto
{
    public required string AccessToken { get; set; }
    public required string TokenType { get; set; }
    public int TenantNumber { get; set; }
    public required int ExpiresIn { get; set; }
    public required DateTime ExpiresAt { get; set; }
}
