namespace EnergyManagement.Application.DTOs;

public record ReadingDto(
    int Id,
    int MeterId,
    DateTime Timestamp,
    double Value,
    decimal Cost,
    string? Notes);

public record ReadingCreateDto(
    int MeterId,
    DateTime Timestamp,
    double Value,
    string? Notes);

public record ReadingUpdateDto(
    DateTime Timestamp,
    double Value,
    string? Notes);
