using System.ComponentModel.DataAnnotations;
using ResourceBooking.Api.Entities;

namespace ResourceBooking.Api.Dtos;

/// <summary>Resurs kakvog vidi frontend.</summary>
public record ResourceDto(
    int Id,
    string Name,
    ResourceType Type,
    string Location,
    int Capacity,
    string? Description,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    int SlotMinutes,
    bool IsActive);

/// <summary>Podaci za dodavanje i uređivanje resursa (samo admin).</summary>
public class ResourceUpsertRequest
{
    [Required(ErrorMessage = "Naziv je obavezan.")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vrsta resursa je obavezna.")]
    public ResourceType Type { get; set; }

    [Required(ErrorMessage = "Lokacija je obavezna.")]
    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "Kapacitet mora biti između 1 i 1000.")]
    public int Capacity { get; set; } = 1;

    [MaxLength(500)]
    public string? Description { get; set; }

    public TimeOnly OpeningTime { get; set; } = new(8, 0);

    public TimeOnly ClosingTime { get; set; } = new(20, 0);

    [Range(15, 480, ErrorMessage = "Trajanje termina mora biti između 15 i 480 minuta.")]
    public int SlotMinutes { get; set; } = 60;

    public bool IsActive { get; set; } = true;
}

/// <summary>Jedan termin u danu i je li slobodan.</summary>
public record TimeSlotDto(DateTime Start, DateTime End, bool IsAvailable);

/// <summary>Svi termini resursa za odabrani datum.</summary>
public record ResourceAvailabilityDto(
    int ResourceId,
    string ResourceName,
    DateOnly Date,
    IReadOnlyList<TimeSlotDto> Slots);
