using EnergyManagement.Application.DTOs;

namespace EnergyManagement.Application.Interfaces;

public interface IMeterService
{
    Task<List<MeterDto>> GetByBuildingAsync(int buildingId);
    Task<MeterDto?> GetByIdAsync(int id);
    Task<MeterDto?> CreateAsync(MeterCreateDto dto);
    Task<MeterDto?> UpdateAsync(int id, MeterUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
