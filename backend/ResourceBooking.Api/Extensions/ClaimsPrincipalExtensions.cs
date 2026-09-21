using System.Security.Claims;
using ResourceBooking.Api.Entities;
using ResourceBooking.Api.Services;

namespace ResourceBooking.Api.Extensions;

/// <summary>Pomoćne metode za čitanje podataka o prijavljenom korisniku iz tokena.</summary>
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out var userId))
        {
            throw AppException.Unauthorized("Token ne sadrži ispravan identifikator korisnika.");
        }

        return userId;
    }

    public static bool IsAdmin(this ClaimsPrincipal principal) =>
        principal.IsInRole(nameof(UserRole.Admin));
}
