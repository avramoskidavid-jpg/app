using EnergyManagement.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EnergyManagement.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Meter> Meters => Set<Meter>();
    public DbSet<Reading> Readings => Set<Reading>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Building>(b =>
        {
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Address).HasMaxLength(300);
        });

        builder.Entity<Meter>(m =>
        {
            m.Property(x => x.SerialNumber).IsRequired().HasMaxLength(100);
            m.Property(x => x.CostPerUnit).HasColumnType("decimal(18,4)");
            m.HasOne(x => x.Building)
                .WithMany(x => x.Meters)
                .HasForeignKey(x => x.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Reading>(r =>
        {
            r.HasOne(x => x.Meter)
                .WithMany(x => x.Readings)
                .HasForeignKey(x => x.MeterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Alert>(a =>
        {
            a.Property(x => x.Message).IsRequired().HasMaxLength(500);
            a.HasOne(x => x.Building)
                .WithMany(x => x.Alerts)
                .HasForeignKey(x => x.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
