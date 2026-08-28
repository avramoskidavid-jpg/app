using EnergyManagement.Application.DTOs;
using EnergyManagement.Application.Interfaces;
using EnergyManagement.Core.Entities;
using EnergyManagement.Core.Enums;
using EnergyManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyManagement.Infrastructure.Services;

public class BuildingService : IBuildingService
{
    private readonly AppDbContext _db;
    private readonly IEnergyAnalyticsService _analytics;
    private readonly IEnergyPriceService _priceService;

    public BuildingService(AppDbContext db, IEnergyAnalyticsService analytics, IEnergyPriceService priceService)
    {
        _db = db;
        _analytics = analytics;
        _priceService = priceService;
    }

    public async Task<List<BuildingDto>> GetAllAsync()
    {
        var buildings = await _db.Buildings
            .Include(b => b.Meters)
            .ThenInclude(m => m.Readings)
            .AsNoTracking()
            .ToListAsync();

        var electricityPrice = await _priceService.GetCurrentPricePerKwhAsync();

        return buildings.Select(b => ToDto(b, electricityPrice)).OrderBy(b => b.Name).ToList();
    }

    public async Task<BuildingDetailsDto?> GetDetailsAsync(int id)
    {
        var building = await _db.Buildings
            .Include(b => b.Meters)
            .ThenInclude(m => m.Readings)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (building is null) return null;

        var electricityPrice = await _priceService.GetCurrentPricePerKwhAsync();
        var buildingDto = ToDto(building, electricityPrice);

        var meters = building.Meters.Select(m =>
        {
            var latest = m.Readings.OrderByDescending(r => r.Timestamp).FirstOrDefault();
            return new MeterDto(
                m.Id, m.BuildingId, m.SerialNumber, m.Type, m.Unit, m.CostPerUnit,
                latest?.Value, latest?.Timestamp);
        }).OrderBy(m => m.SerialNumber).ToList();

        var monthly = BuildMonthlyConsumption(building, electricityPrice);

        return new BuildingDetailsDto(buildingDto, meters, monthly);
    }

    public async Task<BuildingDto> CreateAsync(BuildingCreateDto dto)
    {
        var building = new Building
        {
            Name = dto.Name,
            Address = dto.Address,
            Type = dto.Type,
            AreaSqm = dto.AreaSqm,
            WarningThreshold = dto.WarningThreshold,
            HighThreshold = dto.HighThreshold,
            EnergyTarget = dto.EnergyTarget
        };

        _db.Buildings.Add(building);
        await _db.SaveChangesAsync();

        var electricityPrice = await _priceService.GetCurrentPricePerKwhAsync();
        return ToDto(building, electricityPrice);
    }

    public async Task<BuildingDto?> UpdateAsync(int id, BuildingUpdateDto dto)
    {
        var building = await _db.Buildings
            .Include(b => b.Meters)
            .ThenInclude(m => m.Readings)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (building is null) return null;

        building.Name = dto.Name;
        building.Address = dto.Address;
        building.Type = dto.Type;
        building.AreaSqm = dto.AreaSqm;
        building.WarningThreshold = dto.WarningThreshold;
        building.HighThreshold = dto.HighThreshold;
        building.EnergyTarget = dto.EnergyTarget;

        await _db.SaveChangesAsync();

        var electricityPrice = await _priceService.GetCurrentPricePerKwhAsync();
        return ToDto(building, electricityPrice);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var building = await _db.Buildings.FindAsync(id);
        if (building is null) return false;

        _db.Buildings.Remove(building);
        await _db.SaveChangesAsync();
        return true;
    }

    private BuildingDto ToDto(Building building, decimal electricityPrice)
    {
        var now = DateTime.UtcNow;
        var (consumption, cost) = ConsumptionForMonth(building, now.Year, now.Month, electricityPrice);

        var status = ConsumptionStatus.Normal;
        if (consumption >= building.HighThreshold) status = ConsumptionStatus.High;
        else if (consumption >= building.WarningThreshold) status = ConsumptionStatus.Warning;

        return new BuildingDto(
            building.Id,
            building.Name,
            building.Address,
            building.Type,
            building.AreaSqm,
            building.WarningThreshold,
            building.HighThreshold,
            building.EnergyTarget,
            building.Meters.Count,
            consumption,
            cost,
            status);
    }

    private (double Consumption, decimal Cost) ConsumptionForMonth(Building building, int year, int month, decimal electricityPrice)
    {
        double consumption = 0;
        decimal cost = 0;

        foreach (var meter in building.Meters)
        {
            var pricePerUnit = meter.Type == MeterType.Electricity ? electricityPrice : meter.CostPerUnit;

            foreach (var reading in meter.Readings.Where(r => r.Timestamp.Year == year && r.Timestamp.Month == month))
            {
                consumption += reading.Value;
                cost += _analytics.CalculateEnergyCost(reading.Value, pricePerUnit);
            }
        }

        return (consumption, cost);
    }

    private List<MonthlyConsumptionDto> BuildMonthlyConsumption(Building building, decimal electricityPrice)
    {
        var result = new List<MonthlyConsumptionDto>();
        var now = DateTime.UtcNow;

        for (var i = 11; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            var (consumption, cost) = ConsumptionForMonth(building, date.Year, date.Month, electricityPrice);
            result.Add(new MonthlyConsumptionDto(date.ToString("MMM yyyy"), consumption, cost));
        }

        return result;
    }
}
