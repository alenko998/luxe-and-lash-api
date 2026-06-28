using LuxeAndLash.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LuxeAndLash.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Service> Services { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Appointment>()
            .HasOne(a => a.User)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.UserId);

        builder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId);

        builder.Entity<Service>().HasData(
            new Service { Id = 1, Name = "Classic Manicure",  Description = "Shaping, cuticle care & polish",    Price = 25, DurationMinutes = 45, IsActive = true },
            new Service { Id = 2, Name = "Gel Manicure",      Description = "Long-lasting gel color, 3-4 weeks", Price = 35, DurationMinutes = 60, IsActive = true },
            new Service { Id = 3, Name = "Acrylic Full Set",  Description = "Acrylic extensions with color",     Price = 55, DurationMinutes = 90, IsActive = true },
            new Service { Id = 4, Name = "Nail Art & Design", Description = "Custom art, gems, chrome, airbrush",Price = 15, DurationMinutes = 30, IsActive = true },
            new Service { Id = 5, Name = "Classic Lashes",    Description = "Natural one-to-one extension",      Price = 45, DurationMinutes = 90, IsActive = true },
            new Service { Id = 6, Name = "Volume Lashes",     Description = "Full & dramatic fans",              Price = 60, DurationMinutes = 120, IsActive = true },
            new Service { Id = 7, Name = "Mega Volume",       Description = "Maximum fullness & drama",          Price = 75, DurationMinutes = 150, IsActive = true },
            new Service { Id = 8, Name = "Lash Lift & Tint",  Description = "Lifts & tints your natural lashes", Price = 40, DurationMinutes = 60, IsActive = true }
        );
    }
}