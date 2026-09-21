using ResourceBooking.Api.Entities;

namespace ResourceBooking.Api.Data;

/// <summary>
/// Početni podaci. Pokreće se pri svakom startu aplikacije, ali upisuje
/// samo ako je odgovarajuća tablica prazna - postojeći podaci se ne diraju.
///
/// ---------------------------------------------------------------------
/// OVDJE UREĐUJ POČETNE PODATKE: lista Resources ispod.
/// Nakon izmjene obriši bazu (dotnet ef database drop -f) i pokreni ponovno.
/// ---------------------------------------------------------------------
/// </summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        SeedUsers(db);
        SeedResources(db);
        db.SaveChanges();
    }

    private static void SeedUsers(AppDbContext db)
    {
        if (db.Users.Any())
        {
            return;
        }

        db.Users.AddRange(
            new User
            {
                Email = "admin@demo.hr",
                FullName = "Admin Demo",
                Role = UserRole.Admin,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!")
            },
            new User
            {
                Email = "korisnik@demo.hr",
                FullName = "Ivan Horvat",
                Role = UserRole.User,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Korisnik123!")
            });
    }

    private static void SeedResources(AppDbContext db)
    {
        if (db.Resources.Any())
        {
            return;
        }

        db.Resources.AddRange(
            new Resource
            {
                Name = "Velika dvorana",
                Type = ResourceType.MeetingRoom,
                Location = "Prizemlje",
                Capacity = 40,
                Description = "Dvorana za veće sastanke i prezentacije, projektor i ozvučenje.",
                OpeningTime = new TimeOnly(8, 0),
                ClosingTime = new TimeOnly(20, 0),
                SlotMinutes = 60
            },
            new Resource
            {
                Name = "Sala Sjever",
                Type = ResourceType.MeetingRoom,
                Location = "1. kat",
                Capacity = 12,
                Description = "Sala za timske sastanke, TV ekran za dijeljenje zaslona.",
                OpeningTime = new TimeOnly(8, 0),
                ClosingTime = new TimeOnly(18, 0),
                SlotMinutes = 60
            },
            new Resource
            {
                Name = "Sala Jug",
                Type = ResourceType.MeetingRoom,
                Location = "1. kat",
                Capacity = 8,
                Description = "Manja sala, pogodna za kraće sastanke.",
                OpeningTime = new TimeOnly(8, 0),
                ClosingTime = new TimeOnly(18, 0),
                SlotMinutes = 30
            },
            new Resource
            {
                Name = "Soba za sastanke A",
                Type = ResourceType.MeetingRoom,
                Location = "2. kat",
                Capacity = 6,
                Description = "Tiha soba za razgovore u četiri oka i videopozive.",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(17, 0),
                SlotMinutes = 30
            },
            new Resource
            {
                Name = "Projektor Epson EB-2250U",
                Type = ResourceType.Equipment,
                Location = "Skladište opreme",
                Capacity = 1,
                Description = "Prijenosni projektor s HDMI i VGA priključkom.",
                OpeningTime = new TimeOnly(8, 0),
                ClosingTime = new TimeOnly(20, 0),
                SlotMinutes = 60
            },
            new Resource
            {
                Name = "Konferencijska oprema (kamera i mikrofon)",
                Type = ResourceType.Equipment,
                Location = "Skladište opreme",
                Capacity = 1,
                Description = "Set za videokonferencije - kamera, mikrofon i zvučnik.",
                OpeningTime = new TimeOnly(8, 0),
                ClosingTime = new TimeOnly(20, 0),
                SlotMinutes = 120
            });
    }
}
