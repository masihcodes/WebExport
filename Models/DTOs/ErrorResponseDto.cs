namespace Models.DTOs;

public record ErrorResponseDto
{
    public required string ErrorCode { get; set; }
    public required string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
