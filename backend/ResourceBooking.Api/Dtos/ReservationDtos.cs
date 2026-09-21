using System.ComponentModel.DataAnnotations;
using ResourceBooking.Api.Entities;

namespace ResourceBooking.Api.Dtos;

public class CreateReservationRequest
{
    [Required(ErrorMessage = "Resurs je obavezan.")]
    public int ResourceId { get; set; }

    [Required(ErrorMessage = "Početak termina je obavezan.")]
    public DateTime StartsAt { get; set; }

    [Required(ErrorMessage = "Kraj termina je obavezan.")]
    public DateTime EndsAt { get; set; }

    [MaxLength(300)]
    public string? Note { get; set; }
}

public record ReservationDto(
    int Id,
    int ResourceId,
    string ResourceName,
    int UserId,
    string UserFullName,
    DateTime StartsAt,
    DateTime EndsAt,
    string? Note,
    ReservationStatus Status,
    DateTime CreatedAt,
    DateTime? CancelledAt);

/// <summary>Jednostavna statistika za prijavljenog korisnika.</summary>
public record ReservationStatsDto(
    int Total,
    int Active,
    int Cancelled,
    string? MostUsedResource,
    double HoursBooked);
