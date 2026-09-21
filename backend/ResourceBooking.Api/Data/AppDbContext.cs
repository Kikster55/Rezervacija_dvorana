using Microsoft.EntityFrameworkCore;
using ResourceBooking.Api.Entities;

namespace ResourceBooking.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
            entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

            // Jedan korisnik po e-mail adresi.
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.Property(r => r.Name).HasMaxLength(150).IsRequired();
            entity.Property(r => r.Location).HasMaxLength(150).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(500);
            entity.Property(r => r.Type).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.Property(r => r.Note).HasMaxLength(300);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(r => r.Resource)
                  .WithMany(r => r.Reservations)
                  .HasForeignKey(r => r.ResourceId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Korisnik se ne briše ako ima rezervacije - povijest ostaje.
            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reservations)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indeks koji ubrzava provjeru preklapanja termina.
            entity.HasIndex(r => new { r.ResourceId, r.StartsAt, r.EndsAt });
        });
    }
}
