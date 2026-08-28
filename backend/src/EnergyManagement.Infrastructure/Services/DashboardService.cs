using EnergyManagement.Application.DTOs;
using EnergyManagement.Application.Interfaces;
using EnergyManagement.Core.Enums;
using EnergyManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyManagement.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly IEnergyAnalyticsService _analytics;
    private readonly IEnergyPriceService _priceService;

    public DashboardService(AppDbContext db, IEnergyAnalyticsService analytics, IEnergyPriceService priceService)
    {
        _db = db;
        _analytics = analytics;
        _priceService = priceService;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var buildings = await _db.Buildings
            .Include(b => b.Meters)
            .ThenInclude(m => m.Readings)
            .AsNoTracking()
            .ToListAsync();

        var now = DateTime.UtcNow;
        var electricityPrice = await _priceService.GetCurrentPricePerKwhAsync();
        double totalConsumption = 0;
        decimal totalCost = 0;

        foreach (var building in buildings)
        {
            foreach (var meter in building.Meters)
            {
                var pricePerUnit = meter.Type == MeterType.Electricity ? electricityPrice : meter.CostPerUnit;

                foreach (var reading in meter.Readings.Where(r => r.Timestamp.Year == now.Year && r.Timestamp.Month == now.Month))
                {
                    totalConsumption += reading.Value;
                    totalCost += _analytics.CalculateEnergyCost(reading.Value, pricePerUnit);
                }
            }
        }

        var activeAlerts = await _db.Alerts.CountAsync(a => a.Status == AlertStatus.Open);

        return new DashboardSummaryDto(buildings.Count, totalConsumption, totalCost, activeAlerts);
    }
}
