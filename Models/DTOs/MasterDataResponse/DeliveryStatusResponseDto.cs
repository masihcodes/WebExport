using Models.Enums;

namespace Models.DTOs.MasterDataResponse;

public record DeliveryStatusResponseDto
{
    public DeliveryStatus Status { get; set; }
    public required string Text { get; set; }
}
