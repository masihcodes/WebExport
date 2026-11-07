namespace Models.DTOs.StatusResponse;

public record StatusResponseDto
{
    public required string Status { get; set; }
    public bool DatabaseAvailable { get; set; }
    public required string Version { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
