using Microsoft.EntityFrameworkCore;
using ResourceBooking.Api.Data;
using ResourceBooking.Api.Dtos;
using ResourceBooking.Api.Entities;
using ResourceBooking.Api.Services;

using Xunit;

namespace ResourceBooking.Tests;

/// <summary>
/// Testovi servisa nad bazom u memoriji. Svaki test dobiva svoju bazu,
/// pa testovi ne ovise jedan o drugome ni o redoslijedu pokretanja.
/// </summary>
public class ReservationServiceTests
{
    private const int ObicniKorisnikId = 1;
    private const int DrugiKorisnikId = 2;

    /// <summary>Sutrašnji dan - rezervacije moraju biti u budućnosti.</summary>
    private static readonly DateTime Sutra = DateTime.Today.AddDays(1);

    private static DateTime U(int sat, int minuta = 0) => Sutra.AddHours(sat).AddMinutes(minuta);

    // --- Kreiranje --------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ZaSlobodanTermin_KreiraRezervaciju()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        var rezervacija = await service.CreateAsync(ObicniKorisnikId, Zahtjev(U(10), U(11)));

        Assert.Equal(ReservationStatus.Active, rezervacija.Status);
        Assert.Equal(U(10), rezervacija.StartsAt);
        Assert.Equal("Velika dvorana", rezervacija.ResourceName);
        Assert.Equal(1, await db.Reservations.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_ZaPreklapajuciTermin_BacaGresku()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        await service.CreateAsync(ObicniKorisnikId, Zahtjev(U(10), U(11)));

        var greska = await Assert.ThrowsAsync<AppException>(
            () => service.CreateAsync(DrugiKorisnikId, Zahtjev(U(10, 30), U(11, 30))));

        Assert.Equal(400, greska.StatusCode);
        Assert.Equal(1, await db.Reservations.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_ZaSusjedanTermin_Prolazi()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        await service.CreateAsync(ObicniKorisnikId, Zahtjev(U(10), U(11)));
        await service.CreateAsync(DrugiKorisnikId, Zahtjev(U(11), U(12)));

        Assert.Equal(2, await db.Reservations.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_KadJePostojecaRezervacijaOtkazana_TerminJeOpetSlobodan()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        var prva = await service.CreateAsync(ObicniKorisnikId, Zahtjev(U(10), U(11)));
        await service.CancelAsync(prva.Id, ObicniKorisnikId, isAdmin: false);

        var druga = await service.CreateAsync(DrugiKorisnikId, Zahtjev(U(10), U(11)));

        Assert.Equal(ReservationStatus.Active, druga.Status);
    }

    [Fact]
    public async Task CreateAsync_ZaTerminUProslosti_BacaGresku()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        var jucer = DateTime.Today.AddDays(-1);
        var zahtjev = Zahtjev(jucer.AddHours(10), jucer.AddHours(11));

        await Assert.ThrowsAsync<AppException>(() => service.CreateAsync(ObicniKorisnikId, zahtjev));
    }

    [Fact]
    public async Task CreateAsync_ZaTerminIzvanRadnogVremena_BacaGresku()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        // Resurs radi od 08:00 do 20:00.
        await Assert.ThrowsAsync<AppException>(
            () => service.CreateAsync(ObicniKorisnikId, Zahtjev(U(6), U(7))));

        await Assert.ThrowsAsync<AppException>(
            () => service.CreateAsync(ObicniKorisnikId, Zahtjev(U(19), U(21))));
    }

    [Fact]
    public async Task CreateAsync_KadJeKrajPrijePocetka_BacaGresku()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        await Assert.ThrowsAsync<AppException>(
            () => service.CreateAsync(ObicniKorisnikId, Zahtjev(U(12), U(11))));
    }

    [Fact]
    public async Task CreateAsync_ZaNeaktivanResurs_BacaGresku()
    {
        await using var db = NovaBaza();
        var resurs = await db.Resources.SingleAsync(r => r.Id == 1);
        resurs.IsActive = false;
        await db.SaveChangesAsync();

        var service = new ReservationService(db);

        await Assert.ThrowsAsync<AppException>(
            () => service.CreateAsync(ObicniKorisnikId, Zahtjev(U(10), U(11))));
    }

    [Fact]
    public async Task CreateAsync_ZaNepostojeciResurs_BacaNotFound()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        var zahtjev = Zahtjev(U(10), U(11));
        zahtjev.ResourceId = 999;

        var greska = await Assert.ThrowsAsync<AppException>(
            () => service.CreateAsync(ObicniKorisnikId, zahtjev));

        Assert.Equal(404, greska.StatusCode);
    }

    // --- Otkazivanje ------------------------------------------------------

    [Fact]
    public async Task CancelAsync_ZaTudjuRezervaciju_BacaForbidden()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        var rezervacija = await service.CreateAsync(ObicniKorisnikId, Zahtjev(U(10), U(11)));

        var greska = await Assert.ThrowsAsync<AppException>(
            () => service.CancelAsync(rezervacija.Id, DrugiKorisnikId, isAdmin: false));

        Assert.Equal(403, greska.StatusCode);
    }

    [Fact]
    public async Task CancelAsync_KadOtkazujeAdmin_ProlaziIZaTudjuRezervaciju()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        var rezervacija = await service.CreateAsync(ObicniKorisnikId, Zahtjev(U(10), U(11)));
        var otkazana = await service.CancelAsync(rezervacija.Id, DrugiKorisnikId, isAdmin: true);

        Assert.Equal(ReservationStatus.Cancelled, otkazana.Status);
        Assert.NotNull(otkazana.CancelledAt);
    }

    [Fact]
    public async Task CancelAsync_ZaVecOtkazanuRezervaciju_BacaGresku()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        var rezervacija = await service.CreateAsync(ObicniKorisnikId, Zahtjev(U(10), U(11)));
        await service.CancelAsync(rezervacija.Id, ObicniKorisnikId, isAdmin: false);

        await Assert.ThrowsAsync<AppException>(
            () => service.CancelAsync(rezervacija.Id, ObicniKorisnikId, isAdmin: false));
    }

    // --- Statistika -------------------------------------------------------

    [Fact]
    public async Task GetStatsAsync_RacunaSamoRezervacijePrijavljenogKorisnika()
    {
        await using var db = NovaBaza();
        var service = new ReservationService(db);

        await service.CreateAsync(ObicniKorisnikId, Zahtjev(U(10), U(11)));
        await service.CreateAsync(ObicniKorisnikId, Zahtjev(U(12), U(13)));
        await service.CreateAsync(DrugiKorisnikId, Zahtjev(U(14), U(15)));

        var stats = await service.GetStatsAsync(ObicniKorisnikId);

        Assert.Equal(2, stats.Total);
        Assert.Equal(2, stats.Active);
        Assert.Equal(2.0, stats.HoursBooked);
        Assert.Equal("Velika dvorana", stats.MostUsedResource);
    }

    // --- Pomoćne metode ---------------------------------------------------

    private static CreateReservationRequest Zahtjev(DateTime pocetak, DateTime kraj) => new()
    {
        ResourceId = 1,
        StartsAt = pocetak,
        EndsAt = kraj,
        Note = "Test"
    };

    private static AppDbContext NovaBaza()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;

        var db = new AppDbContext(options);

        db.Users.AddRange(
            new User { Id = ObicniKorisnikId, Email = "ivan@demo.hr", FullName = "Ivan Horvat", PasswordHash = "x" },
            new User { Id = DrugiKorisnikId, Email = "ana@demo.hr", FullName = "Ana Anić", PasswordHash = "x" });

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
