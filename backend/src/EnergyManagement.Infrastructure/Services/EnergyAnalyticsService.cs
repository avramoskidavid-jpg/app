using EnergyManagement.Application.DTOs;
using EnergyManagement.Application.Interfaces;
using EnergyManagement.Core.Entities;
using EnergyManagement.Core.Enums;
using EnergyManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyManagement.Infrastructure.Services;

public class EnergyAnalyticsService : IEnergyAnalyticsService
{
    private const double AnomalyThresholdPercent = 20.0;

    private readonly AppDbContext _db;

    public EnergyAnalyticsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<double> CalculateMonthlyConsumptionAsync(int buildingId, int year, int month)
    {
        return await _db.Readings
            .Where(r => r.Meter!.BuildingId == buildingId && r.Timestamp.Year == year && r.Timestamp.Month == month)
            .SumAsync(r => (double?)r.Value) ?? 0;
    }

    public decimal CalculateEnergyCost(double consumption, decimal pricePerUnit)
    {
        return (decimal)consumption * pricePerUnit;
    }

    public async Task<AnomalyResult> DetectConsumptionAnomalyAsync(int buildingId)
    {
        var now = DateTime.UtcNow;
        var current = await CalculateMonthlyConsumptionAsync(buildingId, now.Year, now.Month);

        var previousMonths = new List<double>();
        for (var i = 1; i <= 3; i++)
        {
            var date = now.AddMonths(-i);
            previousMonths.Add(await CalculateMonthlyConsumptionAsync(buildingId, date.Year, date.Month));
        }

        var average = previousMonths.Count > 0 ? previousMonths.Average() : 0;

        if (average <= 0)
        {
            return new AnomalyResult(false, current, average, 0);
        }

        var percentAbove = (current - average) / average * 100;
        var isAnomaly = percentAbove > AnomalyThresholdPercent;

        return new AnomalyResult(isAnomaly, current, average, Math.Round(percentAbove, 1));
    }

    public async Task<EnergyTargetResult> CalculateEnergyTargetAsync(int buildingId)
    {
        var building = await _db.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == buildingId);
        if (building is null)
        {
            return new EnergyTargetResult(false, 0, 0, 0);
        }

        var now = DateTime.UtcNow;
        var consumption = await CalculateMonthlyConsumptionAsync(buildingId, now.Year, now.Month);

        var target = building.EnergyTarget;
        var exceeded = target > 0 && consumption > target;
        var percentOfTarget = target > 0 ? Math.Round(consumption / target * 100, 1) : 0;

        return new EnergyTargetResult(exceeded, consumption, target, percentOfTarget);
    }

    public async Task<bool> CheckBuildingForAnomalyAsync(int buildingId)
    {
        var building = await _db.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == buildingId);
        if (building is null) return false;

        var alert = await BuildAlertIfAnomalousAsync(building);
        if (alert is null) return false;

        _db.Alerts.Add(alert);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> ScanAllBuildingsForAnomaliesAsync()
    {
        var buildings = await _db.Buildings.AsNoTracking().ToListAsync();
        var created = 0;

        foreach (var building in buildings)
        {
            var alert = await BuildAlertIfAnomalousAsync(building);
            if (alert is null) continue;

            _db.Alerts.Add(alert);
            created++;
        }

        if (created > 0)
        {
            await _db.SaveChangesAsync();
        }

        return created;
    }

    private async Task<Alert?> BuildAlertIfAnomalousAsync(Building building)
    {
        var anomaly = await DetectConsumptionAnomalyAsync(building.Id);
        if (!anomaly.IsAnomaly) return null;

        var now = DateTime.UtcNow;
        var alreadyAlerted = await _db.Alerts.AnyAsync(a =>
            a.BuildingId == building.Id &&
            a.Status == AlertStatus.Open &&
            a.CreatedAt.Year == now.Year &&
            a.CreatedAt.Month == now.Month);

        if (alreadyAlerted) return null;

        var severity = anomaly.PercentAboveAverage >= 50 ? AlertSeverity.High
            : anomaly.PercentAboveAverage >= 30 ? AlertSeverity.Medium
            : AlertSeverity.Low;

        var message = $"{building.Name} consumption is {anomaly.PercentAboveAverage:0}% above its 3-month average.";

        return new Alert
        {
            BuildingId = building.Id,
            Message = message,
            Severity = severity,
            Status = AlertStatus.Open,
            CreatedAt = now
        };
    }
}
