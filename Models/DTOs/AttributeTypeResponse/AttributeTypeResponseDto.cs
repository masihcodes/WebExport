namespace Models.DTOs.AttributeTypeResponse;

public record AttributeTypeResponseDto
{
    public short Number { get; set; }
    public required string Name { get; set; }
    public List<AttributeResponseDto> Attributes { get; set; } = new();
}


public record AttributeResponseDto
{
    public short Number { get; set; }
    public required string Name { get; set; }
}
