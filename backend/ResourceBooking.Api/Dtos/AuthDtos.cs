using System.ComponentModel.DataAnnotations;

namespace ResourceBooking.Api.Dtos;

public class RegisterRequest
{
    [Required(ErrorMessage = "E-mail je obavezan.")]
    [EmailAddress(ErrorMessage = "E-mail nije u ispravnom obliku.")]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    [MinLength(6, ErrorMessage = "Lozinka mora imati barem 6 znakova.")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ime i prezime je obavezno.")]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required(ErrorMessage = "E-mail je obavezan.")]
    [EmailAddress(ErrorMessage = "E-mail nije u ispravnom obliku.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Korisnik kakvog vidi frontend - bez hasha lozinke.</summary>
public record UserDto(int Id, string Email, string FullName, string Role);

/// <summary>Odgovor na registraciju i prijavu.</summary>
public record AuthResponse(string Token, UserDto User);
