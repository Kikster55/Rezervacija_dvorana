# Sustav za rezervaciju resursa

Web aplikacija za rezervaciju sala za sastanke i opreme. Korisnici pregledavaju
resurse, vide slobodne termine za odabrani dan i rezerviraju ih; administrator
upravlja resursima i svim rezervacijama.

Projekt je napravljen kao pokazni (portfolio) rad koji demonstrira rad s
React frontendom i ASP.NET Core backendom nad SQL Server bazom.

## Tehnologije

| Sloj | Tehnologija |
|---|---|
| Frontend | React 19, TypeScript, Vite, React Router |
| Backend | C# / ASP.NET Core Web API (.NET 10) |
| Baza | SQL Server LocalDB, Entity Framework Core (code-first + migracije) |
| Autentifikacija | JWT, uloge `User` i `Admin`, lozinke hashirane BCryptom |
| Testovi | xUnit, EF Core InMemory |

## Funkcionalnosti

**Korisnik**

- Registracija i prijava
- Pregled i pretraga resursa (po nazivu, lokaciji i vrsti)
- Pregled slobodnih termina resursa za odabrani datum
- Rezervacija slobodnog termina, uz provjeru da se termini ne preklapaju
- Pregled vlastitih rezervacija i statistike (ukupno, aktivno, otkazano,
  rezervirani sati, najčešće korišten resurs)
- Otkazivanje vlastite rezervacije

**Administrator**

- Sve gore navedeno
- Dodavanje, uređivanje i brisanje resursa, uključujući radno vrijeme i
  trajanje termina
- Pregled rezervacija svih korisnika s filtrima
- Otkazivanje bilo koje rezervacije

## Struktura projekta

```
Projekt/
├─ ResourceBooking.sln
├─ backend/
│  ├─ ResourceBooking.Api/
│  │  ├─ Controllers/    HTTP sloj - prima zahtjev, vraća odgovor
│  │  ├─ Services/       poslovna logika (BookingRules, Auth, Resource, Reservation)
│  │  ├─ Data/           AppDbContext, DbSeeder, Migrations
│  │  ├─ Entities/       User, Resource, Reservation
│  │  ├─ Dtos/           modeli koje API prima i vraća
│  │  ├─ Middleware/     pretvaranje iznimaka u JSON odgovore
│  │  └─ Extensions/     čitanje korisnika iz JWT tokena
│  └─ ResourceBooking.Tests/
└─ frontend/
   └─ src/
      ├─ api/            pozivi prema backendu
      ├─ auth/           kontekst prijave i zaštićene rute
      ├─ components/     Navbar, Message
      └─ pages/          ekrani, uključujući admin/
```

Tok podatka je uvijek isti: **Controller → Service → DbContext**. Kontroleri ne
sadrže poslovnu logiku, servisi ne znaju za HTTP.

## Preduvjeti

- .NET 10 SDK
- Node.js 22 ili noviji
- SQL Server Express LocalDB
- EF alat: `dotnet tool install --global dotnet-ef`

## Pokretanje

Backend i frontend se pokreću u zasebnim prozorima terminala i oba moraju
ostati otvorena.

### 1. Backend

```powershell
cd backend\ResourceBooking.Api
dotnet restore
dotnet ef migrations add InitialCreate -o Data/Migrations   # samo prvi put
dotnet run
```

Baza se kreira i puni početnim podacima automatski pri pokretanju.

- API: `https://localhost:7001` (i `http://localhost:5001`)
- Swagger: `https://localhost:7001/swagger`
- Provjera stanja: `https://localhost:7001/api/health`

### 2. Frontend

```powershell
cd frontend
npm install
npm run dev
```

Aplikacija: `http://localhost:5173`

### 3. Testovi

```powershell
cd backend\ResourceBooking.Tests
dotnet test
```

## Demo računi

| E-mail | Lozinka | Uloga |
|---|---|---|
| `admin@demo.hr` | `Admin123!` | Admin |
| `korisnik@demo.hr` | `Korisnik123!` | User |

Novi korisnici registrirani kroz aplikaciju uvijek dobivaju ulogu `User` -
admin se dodjeljuje samo kroz početne podatke.

## Početni podaci

Popis resursa nalazi se u `backend/ResourceBooking.Api/Data/DbSeeder.cs` i
napisan je tako da se lako uređuje. Seed se izvršava samo ako je tablica prazna.
Za ponovno punjenje:

```powershell
dotnet ef database drop -f
dotnet run
```

## API

| Metoda | Ruta | Pristup |
|---|---|---|
| POST | `/api/auth/register` | svi |
| POST | `/api/auth/login` | svi |
| GET | `/api/auth/me` | prijavljeni |
| GET | `/api/resources?search=&type=` | prijavljeni |
| GET | `/api/resources/{id}` | prijavljeni |
| GET | `/api/resources/{id}/availability?date=` | prijavljeni |
| POST / PUT / DELETE | `/api/resources`, `/api/resources/{id}` | Admin |
| POST | `/api/reservations` | prijavljeni |
| GET | `/api/reservations/my` | prijavljeni |
| GET | `/api/reservations/my/stats` | prijavljeni |
| DELETE | `/api/reservations/{id}` | vlasnik ili Admin |
| GET | `/api/reservations?userId=&resourceId=&from=&to=&status=` | Admin |

Datoteka `backend/ResourceBooking.Api/ResourceBooking.Api.http` sadrži
pripremljene zahtjeve za sve endpointe, uključujući očekivane greške.

## Odluke u dizajnu

**Dostupnost se računa, ne sprema.** Umjesto zasebne tablice slobodnih termina,
svaki resurs ima radno vrijeme i trajanje termina. Slobodni termini su svi
termini u radnom vremenu umanjeni za one koji se preklapaju s aktivnim
rezervacijama. Jedna tablica manje, a nema ni rizika da se spremljena
dostupnost raziđe sa stvarnim rezervacijama.

**Provjera preklapanja je `pocetak < tudjiKraj && tudjiPocetak < kraj`.**
Dodir "kraj jednog = početak drugog" nije preklapanje, pa su susjedni termini
dopušteni. Ista formula postoji na dva mjesta: kao čista funkcija
(`BookingRules.Overlaps`) koju pokrivaju testovi, i unutar EF upita koji se
izvršava nad bazom.

**Nema repository sloja.** EF Core `DbContext` već je implementacija tog uzorka,
pa bi dodatni sloj nad njim u projektu ove veličine bio samo ceremonija.

**Ništa se ne briše nepovratno.** Otkazana rezervacija dobiva status
`Cancelled`, a resurs koji ima rezervacije postaje neaktivan umjesto obrisan.
Zahvaljujući tome statistika može razlikovati otkazano od aktivnog, a povijest
ostaje čitljiva.

**Vremena su u lokalnom vremenu, ne UTC.** Aplikacija je namijenjena jednoj
vremenskoj zoni, pa se vrijeme spremljeno u bazi poklapa s onim što korisnik
vidi na ekranu. U sustavu s korisnicima u više zona ispravan izbor bio bi UTC
uz pretvorbu na prikazu.

**Prijava vraća istu poruku za nepostojeći e-mail i krivu lozinku.** Da su
poruke različite, netko bi isprobavanjem adresa mogao doznati tko ima račun.

## Poznata ograničenja

Ovo je pokazni projekt, pa svjesno nema: osvježavanja tokena i odjave na strani
poslužitelja, resetiranja lozinke e-mailom, stranicanja dugačkih popisa,
rezervacija koje traju preko ponoći, niti prilagodbe za mobilne uređaje.
