using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ResourceBooking.Api.Data;
using ResourceBooking.Api.Middleware;
using ResourceBooking.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Baza ---------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Servisi ------------------------------------------------------------
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<ReservationService>();

// --- Autentifikacija (JWT) ----------------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("Jwt:Key nije postavljen u appsettings.json.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            // Bez tolerancije na isteklo vrijeme - token vrijedi točno koliko piše.
            ClockSkew = TimeSpan.Zero,

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// --- MVC / Swagger ------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Enumi u JSON-u idu kao tekst ("MeetingRoom", "Admin"), ne kao brojevi -
        // čitljivije je i frontend ne mora pamtiti koji broj znači što.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- CORS za React dev poslužitelj --------------------------------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? Array.Empty<string>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// --- Migracije i početni podaci pri pokretanju ---------------------------
// Zgodno za demo aplikaciju: baza se sama kreira i napuni pri prvom startu.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Bez barem jedne migracije baza bi ostala prazna, a seed bi pukao
    // s nejasnom porukom - zato radije odmah kažemo što treba napraviti.
    if (!db.Database.GetMigrations().Any())
    {
        throw new InvalidOperationException(
            "Nema nijedne EF migracije. Pokreni: dotnet ef migrations add InitialCreate -o Data/Migrations");
    }

    db.Database.Migrate();
    DbSeeder.Seed(db);
}

// Obrada grešaka ide prva da uhvati sve što se dogodi niže u lancu.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Kratka provjera da baza radi i da su početni podaci upisani.
app.MapGet("/api/health", async (AppDbContext db) => Results.Ok(new
{
    status = "ok",
    users = await db.Users.CountAsync(),
    resources = await db.Resources.CountAsync(),
    reservations = await db.Reservations.CountAsync()
}));

app.Run();
