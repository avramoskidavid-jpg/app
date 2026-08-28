using EnergyManagement.Application.DTOs;
using EnergyManagement.Application.Interfaces;
using EnergyManagement.Core.Entities;
using EnergyManagement.Core.Enums;
using EnergyManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyManagement.Infrastructure.Services;

public class ReadingService : IReadingService
{
    private readonly AppDbContext _db;
    private readonly IEnergyAnalyticsService _analytics;
    private readonly IEnergyPriceService _priceService;

    public ReadingService(AppDbContext db, IEnergyAnalyticsService analytics, IEnergyPriceService priceService)
    {
        _db = db;
        _analytics = analytics;
        _priceService = priceService;
    }

    public async Task<List<ReadingDto>> GetByMeterAsync(int meterId)
    {
        var meter = await _db.Meters.AsNoTracking().FirstOrDefaultAsync(m => m.Id == meterId);
        if (meter is null) return new List<ReadingDto>();

        var readings = await _db.Readings
            .Where(r => r.MeterId == meterId)
            .OrderByDescending(r => r.Timestamp)
            .AsNoTracking()
            .ToListAsync();

        var pricePerUnit = await GetPricePerUnitAsync(meter);
        return readings.Select(r => ToDto(r, pricePerUnit)).ToList();
    }

    public async Task<ReadingDto?> CreateAsync(ReadingCreateDto dto)
    {
        var meter = await _db.Meters.FirstOrDefaultAsync(m => m.Id == dto.MeterId);
        if (meter is null) return null;

        var reading = new Reading
        {
            MeterId = dto.MeterId,
            Timestamp = dto.Timestamp,
            Value = dto.Value,
            Notes = dto.Notes
        };

        _db.Readings.Add(reading);
        await _db.SaveChangesAsync();

        // A new reading can push this month's consumption into anomaly territory -
        // check immediately rather than waiting for the next periodic scan.
        await _analytics.CheckBuildingForAnomalyAsync(meter.BuildingId);

        var pricePerUnit = await GetPricePerUnitAsync(meter);
        return ToDto(reading, pricePerUnit);
    }

    public async Task<ReadingDto?> UpdateAsync(int id, ReadingUpdateDto dto)
    {
        var reading = await _db.Readings.Include(r => r.Meter).FirstOrDefaultAsync(r => r.Id == id);
        if (reading is null) return null;

        reading.Timestamp = dto.Timestamp;
        reading.Value = dto.Value;
        reading.Notes = dto.Notes;

        await _db.SaveChangesAsync();
        await _analytics.CheckBuildingForAnomalyAsync(reading.Meter!.BuildingId);

        var pricePerUnit = await GetPricePerUnitAsync(reading.Meter!);
        return ToDto(reading, pricePerUnit);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reading = await _db.Readings.FindAsync(id);
        if (reading is null) return false;

        _db.Readings.Remove(reading);
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<decimal> GetPricePerUnitAsync(Meter meter)
    {
        return meter.Type == MeterType.Electricity
            ? await _priceService.GetCurrentPricePerKwhAsync()
            : meter.CostPerUnit;
    }

    private ReadingDto ToDto(Reading reading, decimal pricePerUnit)
    {
        var cost = _analytics.CalculateEnergyCost(reading.Value, pricePerUnit);
        return new ReadingDto(reading.Id, reading.MeterId, reading.Timestamp, reading.Value, cost, reading.Notes);
    }
}
