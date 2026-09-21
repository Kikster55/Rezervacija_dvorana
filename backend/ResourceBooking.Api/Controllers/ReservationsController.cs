using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceBooking.Api.Dtos;
using ResourceBooking.Api.Entities;
using ResourceBooking.Api.Extensions;
using ResourceBooking.Api.Services;

namespace ResourceBooking.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _reservationService;

    public ReservationsController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    /// <summary>Kreiranje rezervacije za prijavljenog korisnika.</summary>
    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(CreateReservationRequest request)
    {
        return Ok(await _reservationService.CreateAsync(User.GetUserId(), request));
    }

    /// <summary>Vlastite rezervacije, najnovije prvo.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetMy()
    {
        return Ok(await _reservationService.GetForUserAsync(User.GetUserId()));
    }

    /// <summary>Jednostavna statistika vlastitih rezervacija.</summary>
    [HttpGet("my/stats")]
    public async Task<ActionResult<ReservationStatsDto>> GetMyStats()
    {
        return Ok(await _reservationService.GetStatsAsync(User.GetUserId()));
    }

    /// <summary>Otkazivanje - vlastite rezervacije, a admin bilo koje.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ReservationDto>> Cancel(int id)
    {
        return Ok(await _reservationService.CancelAsync(id, User.GetUserId(), User.IsAdmin()));
    }

    /// <summary>Sve rezervacije svih korisnika - samo admin.</summary>
    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetAll(
        [FromQuery] int? userId,
        [FromQuery] int? resourceId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] ReservationStatus? status)
    {
        return Ok(await _reservationService.GetAllAsync(userId, resourceId, from, to, status));
    }
}
