using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceBooking.Api.Dtos;
using ResourceBooking.Api.Extensions;
using ResourceBooking.Api.Services;

namespace ResourceBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Registracija novog korisnika. Svaki registrirani korisnik dobiva ulogu "User".</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        return Ok(await _authService.RegisterAsync(request));
    }

    /// <summary>Prijava postojećeg korisnika. Vraća JWT token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        return Ok(await _authService.LoginAsync(request));
    }

    /// <summary>Podaci o trenutno prijavljenom korisniku.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        return Ok(await _authService.GetCurrentUserAsync(User.GetUserId()));
    }
}
