using EnergyManagement.Application.DTOs;

namespace EnergyManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}
