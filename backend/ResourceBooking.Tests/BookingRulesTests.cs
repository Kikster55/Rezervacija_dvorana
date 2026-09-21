using ResourceBooking.Api.Entities;
using ResourceBooking.Api.Services;

using Xunit;

namespace ResourceBooking.Tests;

/// <summary>
/// Testovi čistih pravila - bez baze i bez pokretanja aplikacije.
/// Datum je fiksan da testovi ne ovise o danu kad se pokreću.
/// </summary>
public class BookingRulesTests
{
    private static readonly DateOnly Dan = new(2030, 5, 6);

    private static DateTime U(int sat, int minuta = 0) => Dan.ToDateTime(new TimeOnly(sat, minuta));

    // --- Preklapanje termina ---------------------------------------------

    [Fact]
    public void Overlaps_KadNoviPocinjeUsredPostojeceg_VracaTrue()
    {
        Assert.True(BookingRules.Overlaps(U(10, 30), U(11, 30), U(10), U(11)));
    }

    [Fact]
    public void Overlaps_KadNoviZavrsavaUsredPostojeceg_VracaTrue()
    {
        Assert.True(BookingRules.Overlaps(U(9, 30), U(10, 30), U(10), U(11)));
    }

    [Fact]
    public void Overlaps_KadJeNoviUnutarPostojeceg_VracaTrue()
    {
        Assert.True(BookingRules.Overlaps(U(10, 15), U(10, 45), U(10), U(11)));
    }

    [Fact]
    public void Overlaps_KadNoviObuhvacaPostojeci_VracaTrue()
    {
        Assert.True(BookingRules.Overlaps(U(9), U(12), U(10), U(11)));
    }

    [Fact]
    public void Overlaps_KadSuTerminiIdenticni_VracaTrue()
    {
        Assert.True(BookingRules.Overlaps(U(10), U(11), U(10), U(11)));
    }

    [Fact]
    public void Overlaps_KadJedanZavrsavaGdjeDrugiPocinje_VracaFalse()
    {
        // Susjedni termini su dopušteni - 10-11 i 11-12 se ne sudaraju.
        Assert.False(BookingRules.Overlaps(U(11), U(12), U(10), U(11)));
        Assert.False(BookingRules.Overlaps(U(9), U(10), U(10), U(11)));
    }

    [Fact]
    public void Overlaps_KadSuTerminiPotpunoOdvojeni_VracaFalse()
    {
        Assert.False(BookingRules.Overlaps(U(8), U(9), U(14), U(15)));
    }

    // --- Generiranje termina ---------------------------------------------

    [Fact]
    public void GenerateSlots_ZaPunoRadnoVrijeme_VracaOcekivaniBrojTermina()
    {
        var slots = BookingRules.GenerateSlots(Dan, new TimeOnly(8, 0), new TimeOnly(12, 0), 60);

        Assert.Equal(4, slots.Count);
        Assert.Equal(U(8), slots[0].Start);
        Assert.Equal(U(9), slots[0].End);
        Assert.Equal(U(12), slots[^1].End);
    }

    [Fact]
    public void GenerateSlots_KadZadnjiTerminNeStaneCijeli_IzostavljaGa()
    {
        // Od 8 do 11:30 stanu tri cijela sata, ostatak od 30 minuta se ne nudi.
        var slots = BookingRules.GenerateSlots(Dan, new TimeOnly(8, 0), new TimeOnly(11, 30), 60);

        Assert.Equal(3, slots.Count);
        Assert.Equal(U(11), slots[^1].End);
    }

    [Fact]
    public void GenerateSlots_KadJeZatvaranjePrijeOtvaranja_VracaPraznuListu()
    {
        var slots = BookingRules.GenerateSlots(Dan, new TimeOnly(18, 0), new TimeOnly(8, 0), 60);

        Assert.Empty(slots);
    }

    // --- Radno vrijeme ----------------------------------------------------

    [Fact]
    public void IsWithinWorkingHours_ZaTerminUnutarRadnogVremena_VracaTrue()
    {
        Assert.True(BookingRules.IsWithinWorkingHours(U(10), U(11), new TimeOnly(8, 0), new TimeOnly(20, 0)));
    }

    [Fact]
    public void IsWithinWorkingHours_ZaTerminPrijeOtvaranja_VracaFalse()
    {
        Assert.False(BookingRules.IsWithinWorkingHours(U(7), U(8), new TimeOnly(8, 0), new TimeOnly(20, 0)));
    }

    [Fact]
    public void IsWithinWorkingHours_ZaTerminNakonZatvaranja_VracaFalse()
    {
        Assert.False(BookingRules.IsWithinWorkingHours(U(19), U(21), new TimeOnly(8, 0), new TimeOnly(20, 0)));
    }

    [Fact]
    public void IsWithinWorkingHours_ZaTerminPrekoDvaDana_VracaFalse()
    {
        var pocetak = U(19);
        var kraj = Dan.AddDays(1).ToDateTime(new TimeOnly(9, 0));

        Assert.False(BookingRules.IsWithinWorkingHours(pocetak, kraj, new TimeOnly(8, 0), new TimeOnly(20, 0)));
    }

    // --- Statistika -------------------------------------------------------

    [Fact]
    public void CalculateStats_BrojiAktivneIOtkazaneOdvojeno()
    {
        var stats = BookingRules.CalculateStats(new[]
        {
            Rezervacija("Sala A", U(8), U(9)),
            Rezervacija("Sala A", U(10), U(11)),
            Rezervacija("Sala B", U(12), U(13), ReservationStatus.Cancelled)
        });

        Assert.Equal(3, stats.Total);
        Assert.Equal(2, stats.Active);
        Assert.Equal(1, stats.Cancelled);
    }

    [Fact]
    public void CalculateStats_NeUracunavaOtkazaneUSateNiUNajcesciResurs()
    {
        var stats = BookingRules.CalculateStats(new[]
        {
            Rezervacija("Sala A", U(8), U(10)),
            Rezervacija("Sala B", U(8), U(18), ReservationStatus.Cancelled)
        });

        Assert.Equal(2.0, stats.HoursBooked);
        Assert.Equal("Sala A", stats.MostUsedResource);
    }

    [Fact]
    public void CalculateStats_BezRezervacija_VracaNule()
    {
        var stats = BookingRules.CalculateStats(Array.Empty<Reservation>());

        Assert.Equal(0, stats.Total);
        Assert.Equal(0.0, stats.HoursBooked);
        Assert.Null(stats.MostUsedResource);
    }

    private static Reservation Rezervacija(
        string resurs,
        DateTime pocetak,
        DateTime kraj,
        ReservationStatus status = ReservationStatus.Active) => new()
        {
            StartsAt = pocetak,
            EndsAt = kraj,
            Status = status,
            Resource = new Resource { Name = resurs }
        };
}
