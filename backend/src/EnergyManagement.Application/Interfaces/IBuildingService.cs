using EnergyManagement.Application.DTOs;

namespace EnergyManagement.Application.Interfaces;

public interface IBuildingService
{
    Task<List<BuildingDto>> GetAllAsync();
    Task<BuildingDetailsDto?> GetDetailsAsync(int id);
    Task<BuildingDto> CreateAsync(BuildingCreateDto dto);
    Task<BuildingDto?> UpdateAsync(int id, BuildingUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
