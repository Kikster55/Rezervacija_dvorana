using ResourceBooking.Api.Dtos;
using ResourceBooking.Api.Entities;

namespace ResourceBooking.Api.Services;

/// <summary>
/// Čista logika rezervacija, bez baze i bez HTTP-a. Izdvojena je upravo zato
/// da se može testirati izravno - ovdje su pravila koja pokrivaju unit testovi.
/// </summary>
public static class BookingRules
{
    /// <summary>
    /// Preklapaju li se dva termina. Dodir "kraj jednog = početak drugog"
    /// namjerno NIJE preklapanje - susjedni termini su dopušteni.
    /// </summary>
    public static bool Overlaps(DateTime firstStart, DateTime firstEnd, DateTime secondStart, DateTime secondEnd)
    {
        return firstStart < secondEnd && secondStart < firstEnd;
    }

    /// <summary>
    /// Svi termini u danu, od početka do kraja radnog vremena.
    /// Zadnji termin koji ne stane cijeli do zatvaranja se izostavlja.
    /// </summary>
    public static IReadOnlyList<TimeSlot> GenerateSlots(DateOnly date, TimeOnly opening, TimeOnly closing, int slotMinutes)
    {
        var slots = new List<TimeSlot>();

        if (slotMinutes <= 0 || closing <= opening)
        {
            return slots;
        }

        var dayStart = date.ToDateTime(opening);
        var dayEnd = date.ToDateTime(closing);

        for (var start = dayStart; start.AddMinutes(slotMinutes) <= dayEnd; start = start.AddMinutes(slotMinutes))
        {
            slots.Add(new TimeSlot(start, start.AddMinutes(slotMinutes)));
        }

        return slots;
    }

    /// <summary>
    /// Je li termin unutar radnog vremena resursa. Rezervacija mora počinjati
    /// i završavati isti dan - time izbjegavamo rezervacije preko noći.
    /// </summary>
    public static bool IsWithinWorkingHours(DateTime startsAt, DateTime endsAt, TimeOnly opening, TimeOnly closing)
    {
        if (startsAt.Date != endsAt.Date)
        {
            return false;
        }

        return TimeOnly.FromDateTime(startsAt) >= opening
               && TimeOnly.FromDateTime(endsAt) <= closing;
    }

    /// <summary>
    /// Statistika korisnika. Otkazane rezervacije se broje, ali se ne uračunavaju
    /// u sate ni u najčešće korišteni resurs.
    /// </summary>
    public static ReservationStatsDto CalculateStats(IEnumerable<Reservation> reservations)
    {
        var all = reservations.ToList();
        var active = all.Where(r => r.Status == ReservationStatus.Active).ToList();

        var mostUsed = active
            .GroupBy(r => r.Resource?.Name ?? "-")
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Key)
            .FirstOrDefault();

        var hours = Math.Round(active.Sum(r => (r.EndsAt - r.StartsAt).TotalHours), 1);

        return new ReservationStatsDto(
            Total: all.Count,
            Active: active.Count,
            Cancelled: all.Count(r => r.Status == ReservationStatus.Cancelled),
            MostUsedResource: mostUsed,
            HoursBooked: hours);
    }
}

/// <summary>Jedan termin u rasporedu.</summary>
public readonly record struct TimeSlot(DateTime Start, DateTime End);
