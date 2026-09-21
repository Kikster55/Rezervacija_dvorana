namespace ResourceBooking.Api.Entities;

/// <summary>Vrsta resursa koji se rezervira.</summary>
public enum ResourceType
{
    MeetingRoom = 0,
    Equipment = 1
}

/// <summary>
/// Resurs koji se može rezervirati (sala ili oprema).
/// Dostupnost se ne sprema u zasebnu tablicu nego se računa iz radnog vremena
/// resursa (OpeningTime - ClosingTime) i trajanja termina (SlotMinutes).
/// </summary>
public class Resource
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ResourceType Type { get; set; }

    public string Location { get; set; } = string.Empty;

    /// <summary>Broj osoba (za sale) odnosno komada (za opremu).</summary>
    public int Capacity { get; set; }

    public string? Description { get; set; }

    /// <summary>Početak radnog vremena, npr. 08:00.</summary>
    public TimeOnly OpeningTime { get; set; } = new(8, 0);

    /// <summary>Kraj radnog vremena, npr. 20:00.</summary>
    public TimeOnly ClosingTime { get; set; } = new(20, 0);

    /// <summary>Trajanje jednog termina u minutama, npr. 60.</summary>
    public int SlotMinutes { get; set; } = 60;

    /// <summary>Neaktivan resurs se ne može rezervirati i ne prikazuje se korisnicima.</summary>
    public bool IsActive { get; set; } = true;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
