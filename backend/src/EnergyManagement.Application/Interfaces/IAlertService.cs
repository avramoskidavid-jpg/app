using EnergyManagement.Core.Enums;
using EnergyManagement.Application.DTOs;

namespace EnergyManagement.Application.Interfaces;

public interface IAlertService
{
    Task<List<AlertDto>> GetAllAsync(int? buildingId, AlertSeverity? severity, AlertStatus? status);
    Task<AlertDto?> ResolveAsync(int id);
    Task<int> GetOpenCountAsync();
}
