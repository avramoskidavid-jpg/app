using EnergyManagement.Application.DTOs;
using EnergyManagement.Application.Interfaces;
using EnergyManagement.Core.Enums;
using EnergyManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyManagement.Infrastructure.Services;

public class AlertService : IAlertService
{
    private readonly AppDbContext _db;

    public AlertService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AlertDto>> GetAllAsync(int? buildingId, AlertSeverity? severity, AlertStatus? status)
    {
        var query = _db.Alerts.Include(a => a.Building).AsNoTracking().AsQueryable();

        if (buildingId.HasValue) query = query.Where(a => a.BuildingId == buildingId.Value);
        if (severity.HasValue) query = query.Where(a => a.Severity == severity.Value);
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);

        var alerts = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();

        return alerts.Select(ToDto).ToList();
    }

    public async Task<AlertDto?> ResolveAsync(int id)
    {
        var alert = await _db.Alerts.Include(a => a.Building).FirstOrDefaultAsync(a => a.Id == id);
        if (alert is null) return null;

        if (alert.Status != AlertStatus.Resolved)
        {
            alert.Status = AlertStatus.Resolved;
            alert.ResolvedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return ToDto(alert);
    }

    public async Task<int> GetOpenCountAsync()
    {
        return await _db.Alerts.CountAsync(a => a.Status == AlertStatus.Open);
    }

    private static AlertDto ToDto(Core.Entities.Alert alert)
    {
        return new AlertDto(
            alert.Id,
            alert.BuildingId,
            alert.Building?.Name ?? string.Empty,
            alert.Message,
            alert.Severity,
            alert.Status,
            alert.CreatedAt,
            alert.ResolvedAt);
    }
}
