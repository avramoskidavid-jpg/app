using EnergyManagement.Application.DTOs;
using EnergyManagement.Application.Interfaces;
using EnergyManagement.Core.Entities;
using EnergyManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyManagement.Infrastructure.Services;

public class MeterService : IMeterService
{
    private readonly AppDbContext _db;

    public MeterService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<MeterDto>> GetByBuildingAsync(int buildingId)
    {
        var meters = await _db.Meters
            .Where(m => m.BuildingId == buildingId)
            .Include(m => m.Readings)
            .AsNoTracking()
            .ToListAsync();

        return meters.Select(ToDto).OrderBy(m => m.SerialNumber).ToList();
    }

    public async Task<MeterDto?> GetByIdAsync(int id)
    {
        var meter = await _db.Meters
            .Include(m => m.Readings)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return meter is null ? null : ToDto(meter);
    }

    public async Task<MeterDto?> CreateAsync(MeterCreateDto dto)
    {
        var buildingExists = await _db.Buildings.AnyAsync(b => b.Id == dto.BuildingId);
        if (!buildingExists) return null;

        var meter = new Meter
        {
            BuildingId = dto.BuildingId,
            SerialNumber = dto.SerialNumber,
            Type = dto.Type,
            Unit = dto.Unit,
            CostPerUnit = dto.CostPerUnit
        };

        _db.Meters.Add(meter);
        await _db.SaveChangesAsync();

        return ToDto(meter);
    }

    public async Task<MeterDto?> UpdateAsync(int id, MeterUpdateDto dto)
    {
        var meter = await _db.Meters.Include(m => m.Readings).FirstOrDefaultAsync(m => m.Id == id);
        if (meter is null) return null;

        meter.SerialNumber = dto.SerialNumber;
        meter.Type = dto.Type;
        meter.Unit = dto.Unit;
        meter.CostPerUnit = dto.CostPerUnit;

        await _db.SaveChangesAsync();

        return ToDto(meter);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var meter = await _db.Meters.FindAsync(id);
        if (meter is null) return false;

        _db.Meters.Remove(meter);
        await _db.SaveChangesAsync();
        return true;
    }

    private static MeterDto ToDto(Meter meter)
    {
        var latest = meter.Readings.OrderByDescending(r => r.Timestamp).FirstOrDefault();
        return new MeterDto(
            meter.Id, meter.BuildingId, meter.SerialNumber, meter.Type, meter.Unit, meter.CostPerUnit,
            latest?.Value, latest?.Timestamp);
    }
}
