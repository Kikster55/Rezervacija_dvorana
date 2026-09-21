using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceBooking.Api.Entities;

/// <summary>Status rezervacije.</summary>
public enum ReservationStatus
{
    Active = 0,
    Cancelled = 1
}

/// <summary>
/// Rezervacija jednog termina na resursu.
/// Vremena se vode u lokalnom vremenu - aplikacija je namijenjena jednoj vremenskoj zoni.
/// </summary>
public class Reservation
{
    public int Id { get; set; }

    public int ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime StartsAt { get; set; }

    public DateTime EndsAt { get; set; }

    public string? Note { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? CancelledAt { get; set; }

    /// <summary>Trajanje rezervacije - koristi se u statistici, ne sprema se u bazu.</summary>
    [NotMapped]
    public TimeSpan Duration => EndsAt - StartsAt;
}
