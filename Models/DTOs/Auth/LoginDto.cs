using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.Auth;

public record LoginDto
{
    [Required(ErrorMessage = "Bitte geben Sie Ihre Client-ID ein.")]
    [MaxLength(50, ErrorMessage = "Die Client-ID darf höchstens 50 Zeichen lang sein.")]
    public required string ClientId { get; set; }

    [Required(ErrorMessage = "Bitte geben Sie Ihr Client-Secret ein.")]
    [MaxLength(100, ErrorMessage = "Das Client-Secret darf höchstens 100 Zeichen lang sein.")]
    public required string ClientSecret { get; set; }
}