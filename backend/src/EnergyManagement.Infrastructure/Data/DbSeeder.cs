using EnergyManagement.Application.Interfaces;
using EnergyManagement.Core.Entities;
using EnergyManagement.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyManagement.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        foreach (var roleName in Enum.GetNames<UserRole>())
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await EnsureUserAsync(userManager, "admin@example.com", "Admin User", "Admin123!", "Admin");
        await EnsureUserAsync(userManager, "manager@example.com", "Manager User", "Manager123!", "Manager");
        await EnsureUserAsync(userManager, "viewer@example.com", "Viewer User", "Viewer123!", "Viewer");

        if (await db.Buildings.AnyAsync())
        {
            return;
        }

        var random = new Random(42);
        var now = DateTime.UtcNow;

        var buildingSeeds = new[]
        {
            ("Riverside Office Tower", "12 Riverside Ave", "Office", 4500.0, 6000.0, 9000.0, 5800.0),
            ("Northgate Warehouse", "88 Northgate Rd", "Industrial", 8000.0, 1800.0, 2500.0, 2200.0),
            ("Maple Residential Complex", "5 Maple Street", "Residential", 3200.0, 2200.0, 6500.0, 2600.0),
            ("Downtown Retail Center", "200 Main Street", "Commercial", 2600.0, 5000.0, 7500.0, 2400.0)
        };

        foreach (var (name, address, type, area, warn, high, energyTarget) in buildingSeeds)
        {
            var building = new Building
            {
                Name = name,
                Address = address,
                Type = type,
                AreaSqm = area,
                WarningThreshold = warn,
                HighThreshold = high,
                EnergyTarget = energyTarget
            };
            db.Buildings.Add(building);
            await db.SaveChangesAsync();

            var meterSeeds = new[]
            {
                (MeterType.Electricity, "kWh", 0.18m),
                (MeterType.Water, "m3", 2.10m),
                (MeterType.Gas, "m3", 0.65m)
            };

            foreach (var (meterType, unit, costPerUnit) in meterSeeds)
            {
                var meter = new Meter
                {
                    BuildingId = building.Id,
                    SerialNumber = $"{building.Id}-{meterType}".ToUpperInvariant(),
                    Type = meterType,
                    Unit = unit,
                    CostPerUnit = costPerUnit
                };
                db.Meters.Add(meter);
                await db.SaveChangesAsync();

                var baseline = meterType switch
                {
                    MeterType.Electricity => 1800.0,
                    MeterType.Water => 400.0,
                    _ => 900.0
                };

                for (var i = 11; i >= 0; i--)
                {
                    var monthDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
                    var seasonalFactor = 1.0 + 0.15 * Math.Sin(monthDate.Month * Math.PI / 6);
                    var noise = 0.9 + random.NextDouble() * 0.3;
                    var value = Math.Round(baseline * seasonalFactor * noise, 1);

                    // Deliberately spike this building's current-month readings (well past the
                    // 20% anomaly threshold, comfortably clearing any seasonal swing) so the
                    // seeded data demonstrates a real anomaly/alert out of the box.
                    if (i == 0 && name == "Downtown Retail Center")
                    {
                        value = Math.Round(value * 2.2, 1);
                    }

                    db.Readings.Add(new Reading
                    {
                        MeterId = meter.Id,
                        Timestamp = monthDate,
                        Value = value
                    });
                }

                await db.SaveChangesAsync();
            }
        }

        var analytics = services.GetRequiredService<IEnergyAnalyticsService>();
        await analytics.ScanAllBuildingsForAnomaliesAsync();
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
