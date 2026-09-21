using Microsoft.EntityFrameworkCore;
using ResourceBooking.Api.Data;
using ResourceBooking.Api.Dtos;
using ResourceBooking.Api.Entities;

namespace ResourceBooking.Api.Services;

/// <summary>Dohvat resursa, računanje slobodnih termina i CRUD za admina.</summary>
public class ResourceService
{
    private readonly AppDbContext _db;

    public ResourceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ResourceDto>> GetAllAsync(
        string? search, ResourceType? type, bool includeInactive)
    {
        var query = _db.Resources.AsNoTracking().AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(r => r.IsActive);
        }

        if (type is not null)
        {
            var typeValue = type.Value;
            query = query.Where(r => r.Type == typeValue);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r => r.Name.Contains(term) || r.Location.Contains(term));
        }

        var resources = await query
            .OrderBy(r => r.Name)
            .ToListAsync();

        return resources.Select(ToDto).ToList();
    }

    public async Task<ResourceDto> GetByIdAsync(int id)
    {
        var resource = await _db.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id)
                       ?? throw AppException.NotFound("Resurs nije pronađen.");

        return ToDto(resource);
    }

    /// <summary>Svi termini resursa za zadani datum, s oznakom je li koji zauzet.</summary>
    public async Task<ResourceAvailabilityDto> GetAvailabilityAsync(int resourceId, DateOnly date)
    {
        var resource = await _db.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Id == resourceId)
                       ?? throw AppException.NotFound("Resurs nije pronađen.");

        var slots = BookingRules.GenerateSlots(
            date, resource.OpeningTime, resource.ClosingTime, resource.SlotMinutes);

        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        // Dohvaćamo samo rezervacije koje dodiruju taj dan.
        var taken = await _db.Reservations
            .AsNoTracking()
            .Where(r => r.ResourceId == resourceId
                        && r.Status == ReservationStatus.Active
                        && r.StartsAt < dayEnd
                        && dayStart < r.EndsAt)
            .Select(r => new { r.StartsAt, r.EndsAt })
            .ToListAsync();

        var now = DateTime.Now;

        var result = slots
            .Select(slot => new TimeSlotDto(
                slot.Start,
                slot.End,
                resource.IsActive
                && slot.Start > now
                && !taken.Any(r => BookingRules.Overlaps(slot.Start, slot.End, r.StartsAt, r.EndsAt))))
            .ToList();

        return new ResourceAvailabilityDto(resource.Id, resource.Name, date, result);
    }

    public async Task<ResourceDto> CreateAsync(ResourceUpsertRequest request)
    {
        Validate(request);

        var resource = new Resource();
        Apply(resource, request);

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();

        return ToDto(resource);
    }

    public async Task<ResourceDto> UpdateAsync(int id, ResourceUpsertRequest request)
    {
        Validate(request);

        var resource = await _db.Resources.SingleOrDefaultAsync(r => r.Id == id)
                       ?? throw AppException.NotFound("Resurs nije pronađen.");

        Apply(resource, request);
        await _db.SaveChangesAsync();

        return ToDto(resource);
    }

    /// <summary>
    /// Resurs bez ijedne rezervacije se briše. Ako rezervacije postoje,
    /// samo ga označimo neaktivnim - tako povijest rezervacija ostaje čitljiva.
    /// </summary>
    public async Task<string> DeleteAsync(int id)
    {
        var resource = await _db.Resources
            .Include(r => r.Reservations)
            .SingleOrDefaultAsync(r => r.Id == id)
            ?? throw AppException.NotFound("Resurs nije pronađen.");

        if (resource.Reservations.Count > 0)
        {
            resource.IsActive = false;
            await _db.SaveChangesAsync();
            return "Resurs ima rezervacije pa je označen neaktivnim umjesto obrisan.";
        }

        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync();
        return "Resurs je obrisan.";
    }

    private static void Validate(ResourceUpsertRequest request)
    {
        if (request.ClosingTime <= request.OpeningTime)
        {
            throw new AppException("Kraj radnog vremena mora biti nakon početka.");
        }

        var workingMinutes = (request.ClosingTime - request.OpeningTime).TotalMinutes;

        if (request.SlotMinutes > workingMinutes)
        {
            throw new AppException("Trajanje termina je dulje od radnog vremena resursa.");
        }
    }

    private static void Apply(Resource resource, ResourceUpsertRequest request)
    {
        resource.Name = request.Name.Trim();
        resource.Type = request.Type;
        resource.Location = request.Location.Trim();
        resource.Capacity = request.Capacity;
        resource.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        resource.OpeningTime = request.OpeningTime;
        resource.ClosingTime = request.ClosingTime;
        resource.SlotMinutes = request.SlotMinutes;
        resource.IsActive = request.IsActive;
    }

    private static ResourceDto ToDto(Resource r) => new(
        r.Id, r.Name, r.Type, r.Location, r.Capacity, r.Description,
        r.OpeningTime, r.ClosingTime, r.SlotMinutes, r.IsActive);
}
