namespace ResourceBooking.Api.Entities;

/// <summary>Uloga korisnika u sustavu.</summary>
public enum UserRole
{
    User = 0,
    Admin = 1
}

/// <summary>Registrirani korisnik aplikacije.</summary>
public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt hash lozinke - lozinka se nigdje ne sprema u čitljivom obliku.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
