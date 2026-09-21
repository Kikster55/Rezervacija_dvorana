using Microsoft.EntityFrameworkCore;
using ResourceBooking.Api.Data;
using ResourceBooking.Api.Dtos;
using ResourceBooking.Api.Entities;

namespace ResourceBooking.Api.Services;

/// <summary>Registracija, prijava i dohvat prijavljenog korisnika.</summary>
public class AuthService
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthService(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            throw new AppException("Korisnik s tom e-mail adresom već postoji.");
        }

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);

        // Namjerno ista poruka za nepostojeći e-mail i krivu lozinku -
        // da se izvana ne može zaključiti koje adrese postoje u sustavu.
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw AppException.Unauthorized("Neispravan e-mail ili lozinka.");
        }

        return BuildResponse(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId)
                   ?? throw AppException.NotFound("Korisnik nije pronađen.");

        return ToDto(user);
    }

    private AuthResponse BuildResponse(User user) =>
        new(_tokenService.CreateToken(user), ToDto(user));

    private static UserDto ToDto(User user) =>
        new(user.Id, user.Email, user.FullName, user.Role.ToString());
}
