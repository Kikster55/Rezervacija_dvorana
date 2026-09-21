using Microsoft.EntityFrameworkCore;
using ResourceBooking.Api.Data;
using ResourceBooking.Api.Dtos;
using ResourceBooking.Api.Entities;

namespace ResourceBooking.Api.Services;

/// <summary>Kreiranje, pregled i otkazivanje rezervacija.</summary>
public class ReservationService
{
    private readonly AppDbContext _db;

    public ReservationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ReservationDto> CreateAsync(int userId, CreateReservationRequest request)
    {
        var resource = await _db.Resources.SingleOrDefaultAsync(r => r.Id == request.ResourceId)
                       ?? throw AppException.NotFound("Resurs nije pronađen.");

        if (!resource.IsActive)
        {
            throw new AppException("Resurs trenutno nije dostupan za rezervaciju.");
        }

        var startsAt = request.StartsAt;
        var endsAt = request.EndsAt;

        if (endsAt <= startsAt)
        {
            throw new AppException("Kraj termina mora biti nakon početka.");
        }

        if (startsAt < DateTime.Now)
        {
            throw new AppException("Termin u prošlosti se ne može rezervirati.");
        }

        if (!BookingRules.IsWithinWorkingHours(startsAt, endsAt, resource.OpeningTime, resource.ClosingTime))
        {
            throw new AppException(
                $"Termin mora biti unutar radnog vremena resursa ({resource.OpeningTime:HH\\:mm} - {resource.ClosingTime:HH\\:mm}) i unutar istog dana.");
        }

        // Ista provjera kao BookingRules.Overlaps, samo napisana tako da je
        // EF Core može prevesti u SQL i izvršiti nad bazom.
        var isTaken = await _db.Reservations.AnyAsync(r =>
            r.ResourceId == resource.Id
            && r.Status == ReservationStatus.Active
            && startsAt < r.EndsAt
            && r.StartsAt < endsAt);

        if (isTaken)
        {
            throw new AppException("Odabrani termin je već rezerviran.");
        }

        var reservation = new Reservation
        {
            ResourceId = resource.Id,
            UserId = userId,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            Status = ReservationStatus.Active,
            CreatedAt = DateTime.Now
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        return await GetDtoAsync(reservation.Id);
    }

    public async Task<IReadOnlyList<ReservationDto>> GetForUserAsync(int userId)
    {
        var reservations = await BaseQuery()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.StartsAt)
            .ToListAsync();

        return reservations.Select(ToDto).ToList();
    }

    public async Task<ReservationStatsDto> GetStatsAsync(int userId)
    {
        var reservations = await _db.Reservations
            .AsNoTracking()
            .Include(r => r.Resource)
            .Where(r => r.UserId == userId)
            .ToListAsync();

        return BookingRules.CalculateStats(reservations);
    }

    /// <summary>Pregled svih rezervacija s filterima - samo za admina.</summary>
    public async Task<IReadOnlyList<ReservationDto>> GetAllAsync(
        int? userId, int? resourceId, DateTime? from, DateTime? to, ReservationStatus? status)
    {
        var query = BaseQuery();

        if (userId is not null)
        {
            var value = userId.Value;
            query = query.Where(r => r.UserId == value);
        }

        if (resourceId is not null)
        {
            var value = resourceId.Value;
            query = query.Where(r => r.ResourceId == value);
        }

        if (from is not null)
        {
            var value = from.Value;
            query = query.Where(r => r.EndsAt >= value);
        }

        if (to is not null)
        {
            var value = to.Value;
            query = query.Where(r => r.StartsAt <= value);
        }

        if (status is not null)
        {
            var value = status.Value;
            query = query.Where(r => r.Status == value);
        }

        var reservations = await query
            .OrderByDescending(r => r.StartsAt)
            .ToListAsync();

        return reservations.Select(ToDto).ToList();
    }

    /// <summary>
    /// Otkazivanje. Korisnik smije otkazati samo svoju rezervaciju, admin bilo koju.
    /// Rezervacija se ne briše nego dobiva status "Cancelled" - povijest ostaje.
    /// </summary>
    public async Task<ReservationDto> CancelAsync(int reservationId, int currentUserId, bool isAdmin)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Resource)
            .Include(r => r.User)
            .SingleOrDefaultAsync(r => r.Id == reservationId)
            ?? throw AppException.NotFound("Rezervacija nije pronađena.");

        if (reservation.UserId != currentUserId && !isAdmin)
        {
            throw AppException.Forbidden("Možeš otkazati samo vlastite rezervacije.");
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            throw new AppException("Rezervacija je već otkazana.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return ToDto(reservation);
    }

    private IQueryable<Reservation> BaseQuery() =>
        _db.Reservations
            .AsNoTracking()
            .Include(r => r.Resource)
            .Include(r => r.User);

    private async Task<ReservationDto> GetDtoAsync(int reservationId)
    {
        var reservation = await BaseQuery().SingleAsync(r => r.Id == reservationId);
        return ToDto(reservation);
    }

    private static ReservationDto ToDto(Reservation r) => new(
        r.Id,
        r.ResourceId,
        r.Resource?.Name ?? string.Empty,
        r.UserId,
        r.User?.FullName ?? string.Empty,
        r.StartsAt,
        r.EndsAt,
        r.Note,
        r.Status,
        r.CreatedAt,
        r.CancelledAt);
}
