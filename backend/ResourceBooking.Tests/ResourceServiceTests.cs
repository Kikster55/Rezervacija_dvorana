using Microsoft.EntityFrameworkCore;
using ResourceBooking.Api.Data;
using ResourceBooking.Api.Dtos;
using ResourceBooking.Api.Entities;
using ResourceBooking.Api.Services;

using Xunit;

namespace ResourceBooking.Tests;

public class ResourceServiceTests
{
    private static readonly DateTime Sutra = DateTime.Today.AddDays(1);
    private static readonly DateOnly SutraDatum = DateOnly.FromDateTime(Sutra);

    [Fact]
    public async Task GetAvailabilityAsync_OznacavaRezerviranTerminKaoZauzet()
    {
        await using var db = NovaBaza();

        db.Reservations.Add(new Reservation
        {
            ResourceId = 1,
            UserId = 1,
            StartsAt = Sutra.AddHours(10),
            EndsAt = Sutra.AddHours(11),
            Status = ReservationStatus.Active
        });

        await db.SaveChangesAsync();

        var dostupnost = await new ResourceService(db).GetAvailabilityAsync(1, SutraDatum);

        var zauzeti = dostupnost.Slots.Single(s => s.Start == Sutra.AddHours(10));
        var slobodni = dostupnost.Slots.Single(s => s.Start == Sutra.AddHours(11));

        Assert.False(zauzeti.IsAvailable);
        Assert.True(slobodni.IsAvailable);
    }

    [Fact]
    public async Task GetAvailabilityAsync_OtkazanaRezervacijaNeZauzimaTermin()
    {
        await using var db = NovaBaza();

        db.Reservations.Add(new Reservation
        {
            ResourceId = 1,
            UserId = 1,
            StartsAt = Sutra.AddHours(10),
            EndsAt = Sutra.AddHours(11),
            Status = ReservationStatus.Cancelled
        });

        await db.SaveChangesAsync();

        var dostupnost = await new ResourceService(db).GetAvailabilityAsync(1, SutraDatum);

        Assert.True(dostupnost.Slots.Single(s => s.Start == Sutra.AddHours(10)).IsAvailable);
    }

    [Fact]
    public async Task GetAvailabilityAsync_ZaNeaktivanResurs_NijedanTerminNijeSlobodan()
    {
        await using var db = NovaBaza();

        var resurs = await db.Resources.SingleAsync(r => r.Id == 1);
        resurs.IsActive = false;
        await db.SaveChangesAsync();

        var dostupnost = await new ResourceService(db).GetAvailabilityAsync(1, SutraDatum);

        Assert.NotEmpty(dostupnost.Slots);
        Assert.All(dostupnost.Slots, slot => Assert.False(slot.IsAvailable));
    }

    [Fact]
    public async Task CreateAsync_KadJeZatvaranjePrijeOtvaranja_BacaGresku()
    {
        await using var db = NovaBaza();
        var service = new ResourceService(db);

        var zahtjev = new ResourceUpsertRequest
        {
            Name = "Neispravna sala",
            Type = ResourceType.MeetingRoom,
            Location = "1. kat",
            Capacity = 5,
            OpeningTime = new TimeOnly(18, 0),
            ClosingTime = new TimeOnly(9, 0),
            SlotMinutes = 60
        };

        await Assert.ThrowsAsync<AppException>(() => service.CreateAsync(zahtjev));
    }

    [Fact]
    public async Task DeleteAsync_ZaResursSRezervacijama_SamoGaOznaciNeaktivnim()
    {
        await using var db = NovaBaza();

        db.Reservations.Add(new Reservation
        {
            ResourceId = 1,
            UserId = 1,
            StartsAt = Sutra.AddHours(10),
            EndsAt = Sutra.AddHours(11),
            Status = ReservationStatus.Active
        });

        await db.SaveChangesAsync();

        await new ResourceService(db).DeleteAsync(1);

        var resurs = await db.Resources.SingleAsync(r => r.Id == 1);
        Assert.False(resurs.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_ZaResursBezRezervacija_BriseGa()
    {
        await using var db = NovaBaza();

        await new ResourceService(db).DeleteAsync(1);

        Assert.Equal(0, await db.Resources.CountAsync());
    }

    private static AppDbContext NovaBaza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;

        var db = new AppDbContext(options);

        db.Users.Add(new User { Id = 1, Email = "ivan@demo.hr", FullName = "Ivan Horvat", PasswordHash = "x" });

        db.Resources.Add(new Resource
        {
            Id = 1,
            Name = "Velika dvorana",
            Type = ResourceType.MeetingRoom,
            Location = "Prizemlje",
            Capacity = 40,
            OpeningTime = new TimeOnly(8, 0),
            ClosingTime = new TimeOnly(20, 0),
            SlotMinutes = 60,
            IsActive = true
        });

        db.SaveChanges();
        return db;
    }
}
