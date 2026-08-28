using EnergyManagement.Application.DTOs;

namespace EnergyManagement.Application.Interfaces;

public interface IReadingService
{
    Task<List<ReadingDto>> GetByMeterAsync(int meterId);
    Task<ReadingDto?> CreateAsync(ReadingCreateDto dto);
    Task<ReadingDto?> UpdateAsync(int id, ReadingUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
